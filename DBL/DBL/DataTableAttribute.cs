using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBL.Mapping
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DataTableAttribute : Attribute
    {
        
        private string m_name;
        
        public DataTableAttribute() { }
        public DataTableAttribute(string tableName) {
            Name = tableName;
            m_name = tableName;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
    }
}
