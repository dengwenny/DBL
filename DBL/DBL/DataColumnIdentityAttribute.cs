using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBL.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DataColumnIdentityAttribute : Attribute
    {
        /// <summary>
        /// 是否主键
        /// </summary>
        private bool m_isIdentity;

        //设置值
        public DataColumnIdentityAttribute() { }
        public DataColumnIdentityAttribute(bool isIdentity) {
            m_isIdentity = isIdentity;
        }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsIdentity { get { return m_isIdentity; } }
    }
}
