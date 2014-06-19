using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBL
{
    public static class DatabaseFactory
    {

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="name">配置文件连接名</param>
        /// <returns></returns>
        public static IDBHelper CreateDBServer(string name)
        {
            return new DBHelperDBServer(name);
        }


    }
}
