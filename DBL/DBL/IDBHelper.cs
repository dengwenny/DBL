using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;

namespace DBL
{
    public interface IDBHelper
    {
        string ProviderName { get; }
        
        /// <summary>
        /// 创建DbCommand
        /// </summary>
        /// <returns></returns>
        DbCommand CreateDbCommandObj();

        /// <summary>
        /// 创建adapter适配器
        /// </summary>
        /// <returns></returns>
        DbDataAdapter CreateDataAdapterObj();

        /// <summary>
        /// 给command设置执行语句类型参数connection.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        void BuildQueryCommand(IDbCommand command, string sql, CommandType type);

        /// <summary>
        /// 打开连接,用open方法,会打开实例创建的连接
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭实例创建的连接
        /// </summary>
        void Close();

        #region 常规查询
        /// <summary>
        /// 执行SQL语句返回受影响的行数
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="para">参数数组</param>
        /// <returns></returns>
        int ExecuteNonQuery(string sql, CommandType type);

        /// <summary>
        /// 返回的结果
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="para">参数数组</param>
        /// <returns></returns>
        object ExecuteScalar(string sql, CommandType type);
        /// <summary>
        /// 执行SQL语句返回结果集
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="para">参数数组</param>
        /// <returns></returns>
        DataSet ExecuteTable(string sql, CommandType type);

        /// <summary>
        /// 执行SQL语句返回结果集
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <returns></returns>
        DataSet ExecuteSql(string sql);

        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <param name="sql">要执行的存储过程</param>
        /// <returns></returns>
        DataSet ExecuteProcedure(string procedure);

        /// <summary>
        /// 执行存储过程分页获取数据集
        /// </summary>
        /// <param name="pageSize">每页最大行数</param>
        /// <param name="pageIndex">获取页的编号</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="para">参数数组</param>
        /// <returns></returns>
        DataSet ExecuteTableByPage(int pageSize, int pageIndex, string sql, CommandType type);


        #endregion

        /// <summary>
        /// 设置每次command需要的参数,BuildQueryCommand会把参数绑定给command.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbtype"></param>
        /// <param name="value"></param>
        void setDBparameter(string name, DbType dbtype, object value);

        #region 事务处理
        /// <summary>
        /// 表示当前数据库公共访问层的实例是否在事务模式中
        /// </summary>
        bool IsInTransaction { get; }
        /// <summary>
        /// 开始事务,打开实例的连接(会执行Open()打开连接),开启连接的事物状态
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 结束事务将所有存储中的仓储对象中位提交的修改保存至物理存储中
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚操作，抛弃所有未提交的修改，并重置事物模式
        /// </summary>
        void Rollback();
        #endregion
    }
}
