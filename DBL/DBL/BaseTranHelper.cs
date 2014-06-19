using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace DBL
{
    class BaseTranHelper : IDBHelper
    {

        /// <summary>
        /// 实例DbConnection
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateDbConnectionObj()
        {
            return DbFactoryObj.CreateConnection();
        }
        
        /// <summary>
        /// 实例DbCommand
        /// </summary>
        /// <returns></returns>
        public DbCommand CreateDbCommandObj()
        {
            return DbFactoryObj.CreateCommand();
        }
        /// <summary>
        /// 实例DbParameter
        /// </summary>
        /// <returns></returns>
        public DbParameter CreateDbParameterObj()
        {
            return DbFactoryObj.CreateParameter();
        }
        /// <summary>
        /// 实例DbDataAdapter
        /// </summary>
        public DbDataAdapter CreateDataAdapterObj()
        {
            return DbFactoryObj.CreateDataAdapter();
        }
        public virtual DbProviderFactory DbFactoryObj{
            get {
                return null;
            }
        }

        


        private List<DbParameter> paras = null;

        public void initDbParameter() {
            paras = new List<DbParameter>();
        }
        public void setDBparameter(string name,DbType dbtype,object value) {
            if (paras == null) {
                initDbParameter();
            }
            DbParameter dbpara = CreateDbParameterObj();
            dbpara.ParameterName = name;
            dbpara.DbType = dbtype;
            dbpara.Value = value;
            paras.Add(dbpara);
        }

        public DbConnection cn;
        public DbTransaction tran;

        public string ProviderName
        {
            get;
            set;
        }


        public void Open()
        {
            if (cn.State != ConnectionState.Open)
            {
                if (cn == null)
                {
                    cn = CreateDbConnectionObj();
                }
                if (cn.State.Equals(ConnectionState.Closed))
                {
                    cn.ConnectionString = ConnString;
                    cn.Open();
                }
            }
        }

        public void Close()
        {
            if (cn.State == ConnectionState.Open)
            {
                cn.Close();
            }
        }

        public string ConnString = string.Empty;
        //protected string ConnString = "Data Source=.;Initial Catalog=DBUser;Integrated Security=True";
        
        /// <summary>
        /// 给SqlCommand属性赋值、添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <param name="para"></param>
        public void BuildQueryCommand(IDbCommand command, string sql, CommandType type)
        {
            command.CommandText = sql;
            command.CommandType = type;
            command.Connection = cn;
            if (IsInTransaction && tran!=null)//如果不是出于事务状态,给cmd设置事务
                command.Transaction = tran;
            if (paras!=null && paras.Count > 0)
            {
                foreach (DbParameter item in paras)
                {
                    if ((item.Direction == ParameterDirection.InputOutput || item.Direction == ParameterDirection.Input) && (item.Value == null))
                    {
                        item.Value = DBNull.Value;
                    }
                    command.Parameters.Add(item);
                }
                paras = null;
            }
        }


        #region 常规查询

        public object ExecuteScalar(string sql, CommandType type)
        {
            try
            {
                object result = null;
                using (DbCommand cmd = CreateDbCommandObj())
                {
                    BuildQueryCommand(cmd,sql, type);
                    //如果不是出于事务状态，则开启数据库连接，否则给cmd设置事务
                    if (!IsInTransaction)
                        Open();
                    //执行SQL语句
                    result = cmd.ExecuteScalar();
                    //如果出于事务状态，则存在未保存的修改
                }
                return result;
            }
            catch (Exception e)
            {
                if (IsInTransaction)
                    Rollback();
                throw e;
            }
            finally
            {
                if (!IsInTransaction)
                    Close();
            }
        }
        public int ExecuteNonQuery(string sql, CommandType type)
        {
            try
            {
                int result = 0;
                using (DbCommand cmd = CreateDbCommandObj())
                {
                    BuildQueryCommand(cmd, sql, type);
                    //如果不是出于事务状态，则开启数据库连接，否则给cmd设置事务
                    if (!IsInTransaction)
                        Open();
                    //执行SQL语句
                    result = cmd.ExecuteNonQuery();
                    //如果出于事务状态，则存在未保存的修改
                }
                return result;
            }
            catch (Exception e)
            {
                if (IsInTransaction)
                {
                    Rollback();
                }
                throw e;
            }
            finally
            {
                if (!IsInTransaction)
                {
                    Close();
                }
            }
        }

        public DataSet ExecuteSql(string sql)
        {
            return ExecuteTable(sql, CommandType.Text);
        }

        public DataSet ExecuteProcedure(string procedure)
        {
            return ExecuteTable(procedure, CommandType.StoredProcedure);
        }

        public DataSet ExecuteTable(string sqlorpro, CommandType type)
        {
            try
            {
                DataSet ds = new DataSet();
                if (!IsInTransaction)
                    Open();
                //创建SqlCommand 对象
                using (DbCommand cmd = CreateDbCommandObj())
                {
                    BuildQueryCommand(cmd, sqlorpro, type);
                    //创建数据适配器对象 用于填充Dataset                    
                    DbDataAdapter da = CreateDataAdapterObj();
                    da.SelectCommand = cmd;
                    //填充DataSet 对象
                    da.Fill(ds);
                    cmd.Parameters.Clear();
                }
                return ds;
            }
            catch (Exception e)
            {
                if (IsInTransaction)
                    Rollback();
                throw e;
            }
            finally
            {
                if (!IsInTransaction)
                    Close();
            }
        }

        public DataSet ExecuteTableByPage(int pageSize, int pageIndex, string sql, CommandType type)
        {
            try
            {
                using (DataSet ds = new DataSet())
                {
                    if (!IsInTransaction)
                        Open();
                    //创建SqlCommand 对象
                    using (DbCommand cmd = CreateDbCommandObj())
                    {
                        BuildQueryCommand(cmd, sql, type);
                        //创建数据适配器对象 用于填充Dataset
                        DbDataAdapter da = CreateDataAdapterObj();
                        da.SelectCommand = cmd;
                        //填充DataSet 对象
                        da.Fill(ds, (pageIndex * pageSize), pageSize, "table1");
                    }
                    return ds;
                }
            }
            catch (Exception e)
            {
                if (IsInTransaction)
                    Rollback();
                throw e;
            }
            finally
            {
                if (!IsInTransaction)
                    Close();
            }
        }


        #endregion

        #region 事务处理
        public bool IsInTransaction
        {
            get;
            set;
        }

        public void BeginTransaction()
        {
            try
            {
                Open();
                tran = cn.BeginTransaction();
                IsInTransaction = true;
            }
            catch
            {
                IsInTransaction = false;
            }
        }


        public void Commit()
        {
            try
            {
                tran.Commit();
                Close();
                IsInTransaction = false;
            }
            catch
            {
                Rollback();
            }
            
        }

        public void Rollback()
        {
            tran.Rollback();
            Close();
            IsInTransaction = false;
        }
        #endregion
        
    }
}
