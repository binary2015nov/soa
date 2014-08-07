﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NewLife.Configuration;
using NewLife.Exceptions;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Serialization;

namespace NewLife.Messaging
{
    /// <summary>消息实体基类</summary>
    /// <remarks>
    /// 用消息实体来表达行为和数据，更加直观。
    /// 同时，指定一套序列化和反序列化机制，实现消息实体与传输形式（二进制数据、XML、Json）的互相转换，目前仅支持二进制。
    /// 
    /// 消息实体仿照Windows消息来设计，拥有一部分系统内置消息，同时允许用户自定义消息。
    /// 
    /// 消息模型同时具有扩展功能，关键在于第一个字节。
    /// 普通消息第一个字节表示消息类型，但如果该自己最高位为1，表示使用消息头扩展<see cref="MessageHeader"/>。
    /// 消息头扩展允许指定通道<see cref="MessageHeader.Channel"/>、会话<see cref="MessageHeader.SessionID"/>和消息长度<see cref="MessageHeader.Length"/>（不含扩展头）等。
    /// </remarks>
    public abstract class Message
    {
        #region 属性
        [NonSerialized]
        private MessageHeader _Header;
        /// <summary>消息头。</summary>
        [XmlIgnore]
        public MessageHeader Header { get { return _Header ?? (_Header = new MessageHeader()); } set { _Header = value; } }

        /// <summary>消息类型</summary>
        /// <remarks>第一个字节的第一位决定是否存在消息头。</remarks>
        [XmlIgnore]
        public abstract MessageKind Kind { get; }

        [NonSerialized]
        private Object _UserState;
        /// <summary>在消息处理过程中附带的用户对象。不参与序列化</summary>
        [XmlIgnore]
        public Object UserState { get { return _UserState; } set { _UserState = value; } }
        #endregion

        #region 构造、注册
        static Message()
        {
            Init();
        }

        /// <summary>初始化</summary>
        static void Init()
        {
            var container = ObjectContainer.Current;
            var asm = Assembly.GetExecutingAssembly();
            // 搜索已加载程序集里面的消息类型
            foreach (var item in AssemblyX.FindAllPlugins(typeof(Message), true))
            {
                var msg = TypeX.CreateInstance(item) as Message;
                if (msg != null)
                {
                    if (item.Assembly != asm && msg.Kind < MessageKind.UserDefine) throw new XException("不允许{0}采用小于{1}的保留编码{2}！", item.FullName, MessageKind.UserDefine, msg.Kind);

                    container.Register(typeof(Message), null, msg, msg.Kind);
                }
                //if (msg != null) container.Register<Message>(msg, msg.Kind);
            }
        }
        #endregion

        #region 序列化/反序列化
        ///// <summary>序列化当前消息到流中</summary>
        ///// <param name="stream"></param>
        //public void Write(Stream stream) { Write(stream, SerializationKinds.Binary); }

        /// <summary>序列化当前消息到流中</summary>
        /// <param name="stream"></param>
        /// <param name="rwkind"></param>
        public void Write(Stream stream, RWKinds rwkind = RWKinds.Binary)
        {
            //var writer = new BinaryWriterX(stream);
            var writer = RWService.CreateWriter(rwkind);
            writer.Stream = stream;
            Set(writer.Settings);
            writer.Settings.Encoding = new UTF8Encoding(false);

            if (Debug)
            {
                writer.Debug = true;
                writer.EnableTraceStream();
            }

            // 二进制增加头部
            if (rwkind == RWKinds.Binary)
            {
                // 判断并写入消息头
                if (_Header != null && _Header.UseHeader) Header.Write(writer.Stream);

                // 基类写入编号，保证编号在最前面
                writer.Write((Byte)Kind);
            }
            else
            {
                var n = (Byte)Kind;
                var bts = Encoding.ASCII.GetBytes(n.ToString());
                stream.Write(bts, 0, bts.Length);
            }

            writer.WriteObject(this);
            writer.Flush();
        }

        /// <summary>序列化为数据流</summary>
        /// <param name="rwkind"></param>
        /// <returns></returns>
        public Stream GetStream(RWKinds rwkind = RWKinds.Binary)
        {
            var ms = new MemoryStream();
            Write(ms, rwkind);
            ms.Position = 0;
            return ms;
        }

        ///// <summary>从流中读取消息</summary>
        ///// <param name="stream">数据流</param>
        ///// <returns></returns>
        //public static Message Read(Stream stream) { return Read(stream, false); }

        /// <summary>从流中读取消息</summary>
        /// <param name="stream">数据流</param>
        /// <param name="rwkind"></param>
        /// <param name="ignoreException">忽略异常。如果忽略异常，读取失败时将返回空，并还原数据流位置</param>
        /// <returns></returns>
        public static Message Read(Stream stream, RWKinds rwkind = RWKinds.Binary, Boolean ignoreException = false)
        {
            if (stream == null || stream.Length - stream.Position < 1) return null;

            //var reader = new BinaryReaderX(stream);
            var reader = RWService.CreateReader(rwkind);
            reader.Stream = stream;
            Set(reader.Settings);

            if (Debug)
            {
                reader.Debug = true;
                reader.EnableTraceStream();
            }

            var start = stream.Position;
            // 消息类型，不同序列化方法的识别方式不同
            MessageKind kind = (MessageKind)0;
            Type type = null;
            MessageHeader header = null;

            if (rwkind == RWKinds.Binary)
            {
                // 检查第一个字节
                //var ch = reader.Reader.PeekChar();
                var ch = stream.ReadByte();
                if (ch < 0) return null;
                stream.Seek(-1, SeekOrigin.Current);

                var first = (Byte)ch;

                #region 消息头部扩展
                // 第一个字节的最高位决定是否扩展
                if (MessageHeader.IsValid(first))
                {
                    try
                    {
                        header = new MessageHeader();
                        header.Read(reader.Stream);
                    }
                    catch
                    {
                        stream.Position = start;
                        return null;
                    }

                    // 如果使用了消息头，判断一下数据流长度是否满足
                    if (header.HasFlag(MessageHeader.Flags.Length) && header.Length > stream.Length - stream.Position)
                    {
                        stream.Position = start;
                        return null;
                    }
                }
                #endregion

                // 读取了响应类型和消息类型后，动态创建消息对象
                kind = (MessageKind)(reader.ReadByte() & 0x7F);
            }
            else
            {
                // 前面的数字表示消息种类
                var sb = new StringBuilder();
                Char c;
                while (true)
                {
                    c = (Char)stream.ReadByte();
                    if (c < '0' || c > '9') break;
                    sb.Append(c);
                }
                // 多读了一个，退回去
                stream.Seek(-1, SeekOrigin.Current);
                kind = (MessageKind)Convert.ToByte(sb.ToString());

                //var s = stream.IndexOf(new Byte[] { (Byte)'<' });
                //if (s >= 0)
                //{
                //    var e = stream.IndexOf(new Byte[] { (Byte)'>' }, s + 1);
                //    if (e >= 0)
                //    {
                //        stream.Position = s;
                //        var msgName = Encoding.UTF8.GetString(stream.ReadBytes(e - s - 1));
                //        if (!String.IsNullOrEmpty(msgName) && !msgName.Contains(" ")) type = TypeX.GetType(msgName);
                //    }
                //}
                //var settings = new XmlReaderSettings();
                //settings.IgnoreWhitespace = true;
                //settings.IgnoreComments = true;
                //var xr = XmlReader.Create(stream, settings);
                //while (xr.NodeType != XmlNodeType.Element) { if (!xr.Read())break; }

                //stream.Position = start;
            }

            #region 识别消息类型
            if (type == null) type = ObjectContainer.Current.ResolveType<Message>(kind);
            if (type == null)
            {
                if (ignoreException)
                {
                    stream.Position = start;
                    return null;
                }
                else
                    throw new XException("无法识别的消息类型（Kind={0}）！", kind);
            }
            #endregion

            #region 读取消息
            Message msg;
            if (stream.Position == stream.Length)
                msg = TypeX.CreateInstance(type, null) as Message;
            else
            {
                try
                {
                    msg = reader.ReadObject(type) as Message;
                    if (msg == null) throw new XException("数据格式不正确！");
                }
                catch (Exception ex)
                {
                    if (ignoreException)
                    {
                        stream.Position = start;
                        return null;
                    }

                    var em = ex.Message;
                    if (DumpStreamWhenError)
                    {
                        stream.Position = start;
                        var bin = String.Format("{0:yyyy_MM_dd_HHmmss_fff}.msg", DateTime.Now);
                        bin = Path.Combine(XTrace.LogPath, bin);
                        if (!Path.IsPathRooted(bin)) bin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, bin);
                        bin = Path.GetFullPath(bin);
                        var dir = Path.GetDirectoryName(bin);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        File.WriteAllBytes(bin, stream.ReadBytes());
                        em = String.Format("已Dump数据流到{0}。{1}", bin, em);
                    }
                    var ex2 = new XException("无法从数据流中读取{0}（Kind={1}）消息！{2}", type.Name, kind, em);
                    throw ex2;
                }
            }
            msg.Header = header;
            return msg;
            #endregion
        }

        /// <summary>从流中读取消息</summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static TMessage Read<TMessage>(Stream stream) where TMessage : Message
        {
            return Read(stream) as TMessage;
        }

        static void Set(ReaderWriterSetting setting)
        {
            //setting.IsBaseFirst = true;
            //setting.EncodeInt = true;
            setting.UseTypeFullName = false;

            if (setting is BinarySettings)
            {
                var bset = setting as BinarySettings;
                bset.EncodeInt = true;

                setting.UseObjRef = true;
            }
            else
            {
                //setting.Encoding = new UTF8Encoding(false);
            }
        }
        #endregion

        #region 方法
        /// <summary>探测消息类型，不移动流指针</summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static MessageKind PeekKind(Stream stream)
        {
            Int32 n = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            return (MessageKind)(n & 0x7F);
        }

        /// <summary>探测消息类型，不移动流指针</summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Type PeekType(Stream stream)
        {
            var kind = PeekKind(stream);
            return ObjectContainer.Current.ResolveType<Message>(kind);
        }

        /// <summary>通过首字节和扩展头的Length判断数据流是否消息。</summary>
        /// <remarks>
        /// 首字节作为Kind，然后查找是否有消息注册该Kind。
        /// 对于大小写，一般要求使用扩展头的Length。
        /// </remarks>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Boolean IsMessage(Stream stream)
        {
            //return PeekType(stream) != null;
            var p = stream.Position;
            try
            {
                // 第一个字节很重要
                var n = stream.ReadByte();
                if (n < 0) return false;
                var first = (Byte)n;

                // 如果没用消息头
                if (!MessageHeader.IsValid(first))
                {
                    // 查一下该消息类型是否以注册，如果未注册，直接返回false
                    var type = ObjectContainer.Current.ResolveType<Message>((MessageKind)(first & 0x7F));
                    if (type == null) return false;
                }

                try
                {
                    stream.Position = p;
                    var header = new MessageHeader();
                    header.Read(stream);

                    // 如果使用了消息头，判断一下数据流长度是否满足
                    if (header.HasFlag(MessageHeader.Flags.Length) && header.Length > stream.Length - stream.Position) return false;
                }
                catch
                {
                    // 如果连消息头都读取不了，显然不对
                    return false;
                }

                // 重新判断一下至粗恶类型
                {
                    n = stream.ReadByte();
                    if (n < 0) return false;
                    first = (Byte)n;
                    // 查一下该消息类型是否以注册，如果未注册，直接返回false
                    var type = ObjectContainer.Current.ResolveType<Message>((MessageKind)(first & 0x7F));
                    if (type == null) return false;
                }

                return true;
            }
            finally
            {
                stream.Position = p;
            }
        }

        /// <summary>从源消息克隆设置和可序列化成员数据</summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual Message CopyFrom(Message msg)
        {
            if (msg != null)
            {
                // 遍历可序列化成员
                foreach (var item in ObjectInfo.GetMembers(this.GetType()))
                {
                    item[this] = item[msg];
                }
            }
            return this;
        }
        #endregion

        #region 设置
        [ThreadStatic]
        private static Boolean? _Debug;
        /// <summary>是否调试，输出序列化过程</summary>
        public static Boolean Debug
        {
            get
            {
                if (_Debug == null) _Debug = Config.GetConfig<Boolean>("NewLife.Message.Debug", false);
                return _Debug.Value;
            }
            set { _Debug = value; }
        }

        private static Boolean? _DumpStreamWhenError;
        /// <summary>出错时Dump数据流到文件中</summary>
        public static Boolean DumpStreamWhenError
        {
            get
            {
                if (_DumpStreamWhenError == null) _DumpStreamWhenError = Config.GetConfig<Boolean>("NewLife.Message.DumpStreamWhenError", false);
                return _DumpStreamWhenError.Value;
            }
            set { _DumpStreamWhenError = value; }
        }
        #endregion

        #region 重载
        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Kind < MessageKind.UserDefine)
                return String.Format("Kind={0}", Kind);
            else
                return String.Format("Kind={0} Type={1}", Kind, this.GetType().Name);
        }
        #endregion
    }
}