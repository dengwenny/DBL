using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;

namespace DBL
{
    class DBHelperDBServer : BaseTranHelper
    {
        string provider;
        public DBHelperDBServer(string connStr)
        {
            IsInTransaction = false;
            ConnString = ConfigurationManager.ConnectionStrings[connStr].ConnectionString;
            provider = ConfigurationManager.ConnectionStrings[connStr].ProviderName;
            ProviderName = provider;
            cn = CreateDbConnectionObj();
        }

        private static readonly Hashtable putterHash_provider = Hashtable.Synchronized(new Hashtable());
        /// <summary>
        /// 实例DbProviderFactory
        /// </summary>
        /// <returns></returns>
        public override DbProviderFactory DbFactoryObj
        {
            get
            {
                if (putterHash_provider.ContainsKey(provider))
                {
                    return putterHash_provider[provider] as DbProviderFactory;
                }
                DbProviderFactory pro = DbProviderFactories.GetFactory(provider);
                putterHash_provider.Add(provider, pro);
                return pro;
            }
        }

    }
}
