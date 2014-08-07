using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NewLife.Configuration;
using NewLife.Reflection;

namespace NewLife.Log
{
    /// <summary>��־�࣬�������ٵ��Թ���</summary>
    /// <remarks>
    /// �þ�̬�����д��־��д����ջ��Dump�����ڴ�ȵ��Թ��ܡ�
    /// 
    /// Ĭ��д��־���ı��ļ�����ͨ���ҽ�<see cref="OnWriteLog"/>�¼���������־�����ʽ��
    /// �ı���־�����ʽ�󣬿�ͨ��<see cref="UseFileLog"/>�������ı��ļ������־��
    /// ���ڿ���̨���̣�����ֱ��ͨ��<see cref="UseConsole"/>����������־����ض���Ϊ����̨��������ҿ���Ϊ��ͬ�߳�ʹ�ò�ͬ��ɫ��
    /// </remarks>
    public static class XTrace
    {
        #region д��־
        /// <summary>�ı��ļ���־</summary>
        public static TextFileLog Log;

        private static Boolean _UseFileLog = true;
        /// <summary>ʹ���ļ���־</summary>
        public static Boolean UseFileLog { get { return _UseFileLog; } set { _UseFileLog = value; } }

        /// <summary>��־·��</summary>
        public static String LogPath { get { InitLog(); return Log.LogPath; } }

        /// <summary>�����־</summary>
        /// <param name="msg">��Ϣ</param>
        public static void Write(String msg)
        {
            InitLog();
            if (OnWriteLog != null)
            {
                var e = new WriteLogEventArgs(msg, null, false);
                OnWriteLog(null, e);
            }

            if (UseFileLog) Log.Write(msg);
        }

        /// <summary>д��־</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Write(String format, params Object[] args)
        {
            InitLog();
            if (OnWriteLog != null)
            {
                var msg = String.Format(format, args);
                var e = new WriteLogEventArgs(msg, null, false);
                OnWriteLog(null, e);
            }

            if (UseFileLog) Log.Write(format, args);
        }

        /// <summary>�����־</summary>
        /// <param name="msg">��Ϣ</param>
        public static void WriteLine(String msg)
        {
            InitLog();
            if (OnWriteLog != null)
            {
                var e = new WriteLogEventArgs(msg, null, true);
                OnWriteLog(null, e);
            }

            if (UseFileLog) Log.WriteLine(msg);
        }

        /// <summary>д��־</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(String format, params Object[] args)
        {
            InitLog();
            if (OnWriteLog != null)
            {
                var msg = String.Format(format, args);
                var e = new WriteLogEventArgs(msg, null, true);
                OnWriteLog(null, e);
            }

            if (UseFileLog) Log.WriteLine(format, args);
        }

        /// <summary>����쳣��־</summary>
        /// <param name="ex">�쳣��Ϣ</param>
        public static void WriteException(Exception ex)
        {
            InitLog();
            if (OnWriteLog != null)
            {
                var e = new WriteLogEventArgs(null, ex, true);
                OnWriteLog(null, e);
            }
            if (UseFileLog) Log.WriteException(ex);
        }

        /// <summary>����쳣��־</summary>
        /// <param name="ex">�쳣��Ϣ</param>
        public static void WriteExceptionWhenDebug(Exception ex) { if (Debug) WriteException(ex); }

        //private static event EventHandler<WriteLogEventArgs> _OnWriteLog;
        /// <summary>д��־�¼����󶨸��¼���XTrace�����ٰ���־д����־�ļ���ȥ��</summary>
        //public static event EventHandler<WriteLogEventArgs> OnWriteLog
        //{
        //    add { _OnWriteLog += value; UseFileLog = false; }
        //    remove { _OnWriteLog -= value; }
        //}
        public static event EventHandler<WriteLogEventArgs> OnWriteLog;
        #endregion

        #region ����
        static XTrace()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static Int32 hasInited = 0;
        static void InitLog()
        {
            if (hasInited > 0 || Interlocked.CompareExchange(ref hasInited, 1, 0) > 0) return;

            Log = TextFileLog.Create(Config.GetConfig<String>("NewLife.LogPath"));

            var asmx = AssemblyX.Create(Assembly.GetExecutingAssembly());
            WriteLine("{0} v{1} Build {2:yyyy-MM-dd HH:mm:ss}", asmx.Name, asmx.FileVersion, asmx.Compile);
        }
        #endregion

        #region ʹ�ÿ���̨���
        private static Int32 init = 0;
        /// <summary>ʹ�ÿ���̨�����־��ֻ�ܵ���һ��</summary>
        /// <param name="useColor"></param>
        public static void UseConsole(Boolean useColor = true)
        {
            if (init > 0 || Interlocked.CompareExchange(ref init, 1, 0) != 0) return;
            if (!Runtime.IsConsole) return;

            if (useColor)
                OnWriteLog += XTrace_OnWriteLog2;
            else
                OnWriteLog += XTrace_OnWriteLog;
        }

        private static void XTrace_OnWriteLog(object sender, WriteLogEventArgs e)
        {
            //Console.WriteLine(e.ToString());
            ConsoleWriteLog(e);
        }

        private static Boolean LastIsNewLine = true;
        private static void ConsoleWriteLog(WriteLogEventArgs e)
        {
            if (LastIsNewLine)
            {
                // �����һ���ǻ��У��������Ҫ�����ͷ��Ϣ
                if (e.IsNewLine)
                    Console.WriteLine(e.ToString());
                else
                {
                    Console.Write(e.ToString());
                    LastIsNewLine = false;
                }
            }
            else
            {
                // �����һ�β��ǻ��У�����β���Ҫ��ͷ��Ϣ
                var msg = e.Message + e.Exception;
                if (e.IsNewLine)
                {
                    Console.WriteLine(msg);
                    LastIsNewLine = true;
                }
                else
                    Console.Write(msg);
            }
        }

        static Dictionary<Int32, ConsoleColor> dic = new Dictionary<Int32, ConsoleColor>();
        static ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Blue };
        private static void XTrace_OnWriteLog2(object sender, WriteLogEventArgs e)
        {
            lock (dic)
            {
                ConsoleColor cc;
                var key = e.ThreadID;
                if (!dic.TryGetValue(key, out cc))
                {
                    //lock (dic)
                    {
                        //if (!dic.TryGetValue(key, out cc))
                        {
                            cc = colors[dic.Count % 7];
                            dic[key] = cc;
                        }
                    }
                }
                var old = Console.ForegroundColor;
                Console.ForegroundColor = cc;
                ConsoleWriteLog(e);
                Console.ForegroundColor = old;
            }
        }
        #endregion

        #region ����WinForm�쳣
        private static Int32 initWF = 0;
        private static Boolean _ShowErrorMessage;
        //private static String _Title;

        /// <summary>����WinForm�쳣����¼��־����ָ���Ƿ���<see cref="MessageBox"/>��ʾ��</summary>
        /// <param name="showErrorMessage">��Ϊ�����쳣ʱ���Ƿ���ʾ��ʾ��Ĭ����ʾ</param>
        public static void UseWinForm(Boolean showErrorMessage = true)
        {
            _ShowErrorMessage = showErrorMessage;

            if (initWF > 0 || Interlocked.CompareExchange(ref initWF, 1, 0) != 0) return;
            //if (!Application.MessageLoop) return;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLine("" + e.ExceptionObject);
            if (e.IsTerminating)
            {
                WriteLine("�쳣�˳���");
                //XTrace.WriteMiniDump(null);
                if (_ShowErrorMessage && Application.MessageLoop) MessageBox.Show("" + e.ExceptionObject, "�쳣�˳�", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (_ShowErrorMessage && Application.MessageLoop) MessageBox.Show("" + e.ExceptionObject, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteException(e.Exception);
            if (_ShowErrorMessage && Application.MessageLoop) MessageBox.Show("" + e.Exception, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region ����
        private static Boolean? _Debug;
        /// <summary>�Ƿ���ԡ��������ָ����ֵ����ֻ��ʹ�ô���ָ����ֵ������ÿ�ζ���ȡ���á�</summary>
        public static Boolean Debug
        {
            get
            {
                if (_Debug != null) return _Debug.Value;

                try
                {
                    //return Config.GetConfig<Boolean>("NewLife.Debug", Config.GetConfig<Boolean>("Debug", false));
                    return Config.GetMutilConfig<Boolean>(false, "NewLife.Debug", "Debug");
                }
                catch { return false; }
            }
            set { _Debug = value; }
        }

        private static String _TempPath;
        /// <summary>��ʱĿ¼</summary>
        public static String TempPath
        {
            get
            {
                if (_TempPath != null) return _TempPath;

                TempPath = Config.GetConfig<String>("NewLife.TempPath", "XTemp");
                return _TempPath;
            }
            set
            {
                _TempPath = value;
                if (String.IsNullOrEmpty(_TempPath)) _TempPath = "XTemp";
                if (!Path.IsPathRooted(_TempPath)) _TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _TempPath);
                _TempPath = Path.GetFullPath(_TempPath);
            }
        }
        #endregion

        #region Dump
        /// <summary>д��ǰ�̵߳�MiniDump</summary>
        /// <param name="dumpFile">�����ָ�������Զ�д����־Ŀ¼</param>
        public static void WriteMiniDump(String dumpFile)
        {
            if (String.IsNullOrEmpty(dumpFile))
            {
                dumpFile = String.Format("{0:yyyyMMdd_HHmmss}.dmp", DateTime.Now);
                if (!String.IsNullOrEmpty(LogPath)) dumpFile = Path.Combine(LogPath, dumpFile);
            }

            MiniDump.TryDump(dumpFile, MiniDump.MiniDumpType.WithFullMemory);
        }

        /// <summary>
        /// ����Ҫʹ����windows 5.1 �Ժ�İ汾��������windows�ܾɣ��Ͱ�Windbg�����dll����������һ�㶼û�����⡣
        /// DbgHelp.dll ��windows�Դ��� dll�ļ� ��
        /// </summary>
        static class MiniDump
        {
            [DllImport("DbgHelp.dll")]
            private static extern Boolean MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 processId,
            IntPtr fileHandle,
            MiniDumpType dumpType,
           ref MinidumpExceptionInfo excepInfo,
            IntPtr userInfo,
            IntPtr extInfo);

            /// <summary>MINIDUMP_EXCEPTION_INFORMATION</summary>
            struct MinidumpExceptionInfo
            {
                public UInt32 ThreadId;
                public IntPtr ExceptionPointers;
                public UInt32 ClientPointers;
            }

            [DllImport("kernel32.dll")]
            private static extern uint GetCurrentThreadId();

            public static Boolean TryDump(String dmpPath, MiniDumpType dmpType)
            {
                //ʹ���ļ��������� .dmp�ļ�
                using (FileStream stream = new FileStream(dmpPath, FileMode.Create))
                {
                    //ȡ�ý�����Ϣ
                    Process process = Process.GetCurrentProcess();

                    // MINIDUMP_EXCEPTION_INFORMATION ��Ϣ�ĳ�ʼ��
                    MinidumpExceptionInfo mei = new MinidumpExceptionInfo();

                    mei.ThreadId = (UInt32)GetCurrentThreadId();
                    mei.ExceptionPointers = Marshal.GetExceptionPointers();
                    mei.ClientPointers = 1;

                    //������õ�Win32 API
                    Boolean res = MiniDumpWriteDump(
                    process.Handle,
                    process.Id,
                    stream.SafeFileHandle.DangerousGetHandle(),
                    dmpType,
                   ref mei,
                    IntPtr.Zero,
                    IntPtr.Zero);

                    //��� stream
                    stream.Flush();
                    stream.Close();

                    return res;
                }
            }

            public enum MiniDumpType
            {
                None = 0x00010000,
                Normal = 0x00000000,
                WithDataSegs = 0x00000001,
                WithFullMemory = 0x00000002,
                WithHandleData = 0x00000004,
                FilterMemory = 0x00000008,
                ScanMemory = 0x00000010,
                WithUnloadedModules = 0x00000020,
                WithIndirectlyReferencedMemory = 0x00000040,
                FilterModulePaths = 0x00000080,
                WithProcessThreadData = 0x00000100,
                WithPrivateReadWriteMemory = 0x00000200,
                WithoutOptionalData = 0x00000400,
                WithFullMemoryInfo = 0x00000800,
                WithThreadInfo = 0x00001000,
                WithCodeSegs = 0x00002000
            }
        }
        #endregion

        #region ����ջ
        /// <summary>��ջ���ԡ�
        /// �����ջ��Ϣ�����ڵ���ʱ������������ġ�
        /// ����������ɴ�����־�������á�
        /// </summary>
        public static void DebugStack()
        {
            var msg = GetCaller(2, 0, Environment.NewLine);
            WriteLine("���ö�ջ��" + Environment.NewLine, msg);
        }

        /// <summary>��ջ���ԡ�</summary>
        /// <param name="maxNum">��󲶻��ջ������</param>
        public static void DebugStack(int maxNum)
        {
            var msg = GetCaller(2, maxNum, Environment.NewLine);
            WriteLine("���ö�ջ��" + Environment.NewLine, msg);
        }

        /// <summary>��ջ����</summary>
        /// <param name="start">��ʼ��������0��DebugStack��ֱ�ӵ�����</param>
        /// <param name="maxNum">��󲶻��ջ������</param>
        public static void DebugStack(int start, int maxNum)
        {
            // ����������ǰ���
            if (start < 1) start = 1;
            var msg = GetCaller(start + 1, maxNum, Environment.NewLine);
            WriteLine("���ö�ջ��" + Environment.NewLine, msg);
        }

        /// <summary>��ȡ����ջ</summary>
        /// <param name="start"></param>
        /// <param name="maxNum"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static String GetCaller(int start = 1, int maxNum = 0, String split = null)
        {
            // ����������ǰ���
            if (start < 1) start = 1;
            var st = new StackTrace(start, true);

            if (String.IsNullOrEmpty(split)) split = "<-";

            Type last = null;
            var asm = Assembly.GetEntryAssembly();
            var entry = asm == null ? null : asm.EntryPoint;

            var sb = new StringBuilder();
            int count = st.FrameCount;
            if (maxNum > 0 && maxNum < count) count = maxNum;
            for (int i = 0; i < count; i++)
            {
                var sf = st.GetFrame(i);
                var method = sf.GetMethod();
                // ����<>���͵���������

                if (method == null || String.IsNullOrEmpty(method.Name) || method.Name[0] == '<' && method.Name.Contains(">")) continue;

                //var name = method.ToString();
                //// ȥ��ǰ��ķ�������
                //var p = name.IndexOf(" ");
                //if (p >= 0) name = name.Substring(p + 1);

                var mix = MethodInfoX.Create(method);
                var type = method.DeclaringType ?? method.ReflectedType;
                if (type != null)
                {
                    if (type != last)
                        sb.Append(mix.Name);
                    else
                        sb.Append(mix.TinyName);
                }
                else
                    sb.Append(mix.Name);
                //sb.Append(MethodInfoX.Create(method).Name);

                // �����������ڵ㣬���Խ�����
                if (method == entry) break;

                if (i < count - 1) sb.Append(split);

                last = type;
            }
            return sb.ToString();
        }
        #endregion
    }
}