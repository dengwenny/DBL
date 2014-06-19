using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace DBL
{
    public interface IEntityPropertyPutter<T> where T : class
    {
        /// <summary>
        /// 根据传入实体和IDBHealper实例新增
        /// </summary>
        /// <param name="entity">新增的实体</param>
        /// <param name="ih">IDBHealper实例</param>
        /// <returns></returns>
        int addEntity(T entity,IDBHelper ih);
        /// <summary>
        /// 根据传入实体和IDBHealper实例新增
        /// </summary>
        /// <param name="entity">修改的实体</param>
        /// <param name="ih">IDBHealper实例</param>
        /// <returns></returns>
        int updateEntity(T entity, IDBHelper ih);
        /// <summary>
        /// DbDataReader转换为实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dr"></param>
        void PutEntityProperties(T entity, DbDataReader dr);

        /// <summary>
        /// DbDataReader转换为实体,,数据列可以不与实体匹配
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dr"></param>
        void PutEntityProperties(T entity, DbDataReader dr, DataColumnCollection datacolumn);

        /// <summary>
        /// DataRow转换为实体,数据列必须与实体属性匹配
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="datarow"></param>
        void PutEntityProperties(T entity, DataRow datarow);


        /// <summary>
        /// DataRow转换为实体,数据列可以不与实体匹配
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="datarow"></param>
        void PutEntityProperties(T entity, DataRow datarow, DataColumnCollection datacolumn);

        /// <summary>
        /// 异常抛出存储数据字段名
        /// </summary>
        string CurrentDBColName { get; }
        /// <summary>
        /// 异常抛出存储实体属性名
        /// </summary>
        string CurrentPropName { get; }
        /// <summary>
        /// 实体名称
        /// </summary>
        string EntityTypeName { get; }
    }

 

}
