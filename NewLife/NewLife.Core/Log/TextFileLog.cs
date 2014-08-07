﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using NewLife.Collections;
using NewLife.Exceptions;
using NewLife.Reflection;

namespace NewLife.Log
{
    /// <summary>文本文件日志类。提供向文本文件写日志的能力</summary>
    public class TextFileLog
    {
        #region 构造
        private TextFileLog(String path) { FilePath = path; }

        static DictionaryCache<String, TextFileLog> cache = new DictionaryCache<string, TextFileLog>();
        /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TextFileLog Create(String path)
        {
            if (String.IsNullOrEmpty(path)) path = "Log";

            String key = path.ToLower();
            return cache.GetItem<String>(key, path, (k, p) => new TextFileLog(p));
        }
        #endregion

        #region 属性
        private String _FilePath;
        /// <summary>文件路径</summary>
        public String FilePath
        {
            get { return _FilePath; }
            private set { _FilePath = value; }
        }

        private String _LogPath;
        /// <summary>日志目录</summary>
        public String LogPath
        {
            get
            {
                if (!String.IsNullOrEmpty(_LogPath)) return _LogPath;

                String dir = FilePath;
                if (!Path.IsPathRooted(dir)) dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);

                //保证\结尾
                if (!String.IsNullOrEmpty(dir) && dir.Substring(dir.Length - 1, 1) != @"\") dir += @"\";

                _LogPath = new DirectoryInfo(dir).FullName;
                return _LogPath;
            }
        }

        /// <summary>是否当前进程的第一次写日志</summary>
        private Boolean isFirst = false;
        #endregion

        #region 内部方法
        private StreamWriter LogWriter;

        /// <summary>初始化日志记录文件</summary>
        private void InitLog()
        {
            String path = LogPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            //if (path.Substring(path.Length - 2) != @"\") path += @"\";
            var logfile = Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd") + ".log");
            StreamWriter writer = null;
            int i = 0;
            while (i < 10)
            {
                try
                {
                    var stream = new FileStream(logfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.AutoFlush = true;
                    break;
                }
                catch
                {
                    if (logfile.EndsWith("_" + i + ".log"))
                        logfile = logfile.Replace("_" + i + ".log", "_" + (++i) + ".log");
                    else
                        logfile = logfile.Replace(@".log", @"_0.log");
                }
            }
            if (i >= 10) throw new XException("无法写入日志！");

            if (!isFirst)
            {
                isFirst = true;

                WriteHead(writer);
            }
            LogWriter = writer;
        }

        private void WriteHead(StreamWriter writer)
        {
            var process = Process.GetCurrentProcess();
            var name = String.Empty;
            var asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyTitleAttribute>();
                    if (att != null) name = att.Title;
                }

                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyProductAttribute>();
                    if (att != null) name = att.Product;
                }

                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
                    if (att != null) name = att.Description;
                }
            }
            if (String.IsNullOrEmpty(name))
            {
                try
                {
                    name = process.MachineName;
                }
                catch { }
            }
            // 通过判断LogWriter.BaseStream.Length，解决有时候日志文件为空但仍然加空行的问题
            //if (File.Exists(logfile) && LogWriter.BaseStream.Length > 0) LogWriter.WriteLine();
            // 因为指定了编码，比如UTF8，开头就会写入3个字节，所以这里不能拿长度跟0比较
            if (writer.BaseStream.Length > 10) writer.WriteLine();
            writer.WriteLine("#Software: {0}", name);
            writer.WriteLine("#ProcessID: {0}", process.Id);
            writer.WriteLine("#AppDomain: {0}", AppDomain.CurrentDomain.FriendlyName);
            writer.WriteLine("#BaseDirectory: {0}", AppDomain.CurrentDomain.BaseDirectory);
            writer.WriteLine("#Date: {0:yyyy-MM-dd}", DateTime.Now);
            writer.WriteLine("#Fields: Time ThreadID IsPoolThread ThreadName Message");
        }

        /// <summary>停止日志</summary>
        private void CloseWriter(Object obj)
        {
            var writer = LogWriter;
            if (writer == null) return;
            lock (Log_Lock)
            {
                try
                {
                    if (writer == null) return;
                    writer.Close();
                    writer.Dispose();
                    LogWriter = null;
                }
                catch { }
            }
        }
        #endregion

        #region 异步写日志
        private Timer AutoCloseWriterTimer;
        private object Log_Lock = new object();
        private Boolean LastIsNewLine = true;

        /// <summary>使用线程池线程异步执行日志写入动作</summary>
        /// <param name="e"></param>
        private void PerformWriteLog(WriteLogEventArgs e)
        {
            lock (Log_Lock)
            {
                try
                {
                    // 初始化日志读写器
                    if (LogWriter == null) InitLog();
                    // 写日志
                    if (LastIsNewLine)
                    {
                        // 如果上一次是换行，则这次需要输出行头信息
                        if (e.IsNewLine)
                            LogWriter.WriteLine(e.ToString());
                        else
                        {
                            LogWriter.Write(e.ToString());
                            LastIsNewLine = false;
                        }
                    }
                    else
                    {
                        // 如果上一次不是换行，则这次不需要行头信息
                        var msg = e.Message + e.Exception;
                        if (e.IsNewLine)
                        {
                            LogWriter.WriteLine(msg);
                            LastIsNewLine = true;
                        }
                        else
                            LogWriter.Write(msg);
                    }
                    // 声明自动关闭日志读写器的定时器。无限延长时间，实际上不工作
                    if (AutoCloseWriterTimer == null) AutoCloseWriterTimer = new Timer(new TimerCallback(CloseWriter), null, Timeout.Infinite, Timeout.Infinite);
                    // 改变定时器为5秒后触发一次。如果5秒内有多次写日志操作，估计定时器不会触发，直到空闲五秒为止
                    AutoCloseWriterTimer.Change(5000, Timeout.Infinite);
                }
                catch { }
            }
        }
        #endregion

        #region 写日志
        /// <summary>输出日志</summary>
        /// <param name="msg">信息</param>
        public void Write(String msg)
        {
            var e = new WriteLogEventArgs(msg, null, false);

            PerformWriteLog(e);
        }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(String format, params Object[] args)
        {
            Write(Format(format, args));
        }

        /// <summary>输出日志</summary>
        /// <param name="msg">信息</param>
        public void WriteLine(String msg)
        {
            // 小对象，采用对象池的成本太高了
            var e = new WriteLogEventArgs(msg, null, true);

            PerformWriteLog(e);
        }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine(String format, params Object[] args)
        {
            WriteLine(Format(format, args));
        }

        String Format(String format, Object[] args)
        {
            //处理时间的格式化
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null && args[i].GetType() == typeof(DateTime))
                    {
                        // 根据时间值的精确度选择不同的格式化输出
                        DateTime dt = (DateTime)args[i];
                        if (dt.Millisecond > 0)
                            args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        else if (dt.Hour > 0 || dt.Minute > 0 || dt.Second > 0)
                            args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        else
                            args[i] = dt.ToString("yyyy-MM-dd");
                    }
                }
            }
            return String.Format(format, args);
        }

        /// <summary>输出异常日志</summary>
        /// <param name="ex">异常信息</param>
        public void WriteException(Exception ex)
        {
            var e = new WriteLogEventArgs(null, ex, true);

            PerformWriteLog(e);
        }
        #endregion
    }
}