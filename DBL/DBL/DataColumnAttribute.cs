using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBL.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DataColumnAttribute : Attribute
    {
        /// <summary>
        /// 是否主键
        /// </summary>
        private bool m_isIdentity;
        /// <summary>
        /// 字段明
        /// </summary>
        private string m_name;

        /// <summary>
        /// 设置值
        /// </summary>
        public DataColumnAttribute(){}
        public DataColumnAttribute(bool isIdentity) {
            m_isIdentity = isIdentity;
        }
        public DataColumnAttribute(string columnName) {
            m_name = columnName;
        }

        // Properties
        public bool IsIdentity { get { return m_isIdentity; } }
        public string Name { get { return m_name; } }
    }
}


