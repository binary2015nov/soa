﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:2.0.50727.3053
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;

// 
// 此源代码由 xsd 自动生成, Version=2.0.50727.3038。
// 

namespace ESB.Core.Schema
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.jn.com/esb/request/20100329")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.jn.com/esb/request/20100329", IsNullable = false)]
    [MessageContract(IsWrapped =true)]
    public partial class 服务请求
    {

        private string 服务名称Field;

        private string 方法名称Field;

        private string 主机名称Field;

        private string 密码Field;

        private System.DateTime 请求时间Field;

        private string 消息内容Field;

        private string 消息编码Field;

        private string 扩展属性Field;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 服务名称
        {
            get
            {
                return this.服务名称Field;
            }
            set
            {
                this.服务名称Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 方法名称
        {
            get
            {
                return this.方法名称Field;
            }
            set
            {
                this.方法名称Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 主机名称
        {
            get
            {
                return this.主机名称Field;
            }
            set
            {
                this.主机名称Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 密码
        {
            get
            {
                return this.密码Field;
            }
            set
            {
                this.密码Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public System.DateTime 请求时间
        {
            get
            {
                return this.请求时间Field;
            }
            set
            {
                this.请求时间Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 消息内容
        {
            get
            {
                return this.消息内容Field;
            }
            set
            {
                this.消息内容Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 消息编码
        {
            get
            {
                return this.消息编码Field;
            }
            set
            {
                this.消息编码Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [MessageBodyMember()]
        public string 扩展属性
        {
            get
            {
                return this.扩展属性Field;
            }
            set
            {
                this.扩展属性Field = value;
            }
        }
    }
}