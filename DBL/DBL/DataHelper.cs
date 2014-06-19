using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;


namespace DBL
{
    public class DataHelper
    {
        private bool m_includeDebugInfo;

        private IDBHelper idbhelper;

        public IDBHelper Idbhelper
        {
            get { return idbhelper; }
            set { idbhelper = value; }
        }

        public DataHelper(IDBHelper idbh)
        {
            Idbhelper = idbh;
        }
        /// <summary>
        /// datarow转实体,数据列和实体一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datarow"></param>
        /// <returns></returns>
        public T CreateEntity<T>(DataRow datarow) where T : class, new()
        {
            T entity = Activator.CreateInstance<T>();
            this.PutEntityProperties<T>(entity, datarow);
            return entity;
        }
        /// <summary>
        /// datarow转实体,数据列和实体列一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public T CreateEntity<T>(DbDataReader dataReader) where T : class, new()
        {
            T entity = Activator.CreateInstance<T>();
            this.PutEntityProperties<T>(entity, dataReader);
            return entity;
        }
        /// <summary>
        /// datarow转实体,数据列可以和实体列不一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datarow"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public T CreateEntity<T>(DataRow datarow,DataColumnCollection dc) where T : class, new()
        {
            T entity = Activator.CreateInstance<T>();
            this.PutEntityProperties<T>(entity, datarow,dc);
            return entity;
        }
        /// <summary>
        /// datarow转实体,数据列可以和实体列不一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public T CreateEntity<T>(DbDataReader dataReader, DataColumnCollection dc) where T : class, new()
        {
            T entity = Activator.CreateInstance<T>();
            this.PutEntityProperties<T>(entity, dataReader,dc);
            return entity;
        }

        public int InsertEntity<T>(T entity) where T : class, new()
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            int num = 0;
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                num = putter.addEntity(entity, idbhelper);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
            return num;
        }
        public void PutEntityProperties<T>(T entity, DataRow datarow,DataColumnCollection dc) where T : class
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                putter.PutEntityProperties(entity, datarow,dc);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
        }
        public void PutEntityProperties<T>(T entity, DataRow datarow) where T : class
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                putter.PutEntityProperties(entity, datarow);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
        }
        public void PutEntityProperties<T>(T entity, DbDataReader dataReader, DataColumnCollection dc) where T : class
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                putter.PutEntityProperties(entity, dataReader,dc);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
        }
        public void PutEntityProperties<T>(T entity,DbDataReader dataReader) where T : class
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                putter.PutEntityProperties(entity, dataReader);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
        }
        public int UpdateEntity<T>(T entity) where T : class, new()
        {
            IEntityPropertyPutter<T> putter = EntityPropertyPutterFactory.Create<T>(entity, idbhelper.ProviderName, this.IncludeDebugInformation);
            int num = 0;
            if (putter == null)
            {
                throw new NullReferenceException("设置器为空 ( Null Putter )");
            }
            try
            {
                num = putter.updateEntity(entity, idbhelper);
            }
            catch (Exception exception)
            {
                if (this.IncludeDebugInformation)
                {
                    string format = null;
                    format = "从数据库字段 {0} 读取值并赋给属性 {1} 时出错(实体类型: {2})";
                    throw new Exception(string.Format(format, putter.CurrentDBColName, putter.CurrentPropName, putter.EntityTypeName), exception);
                }
                throw exception;
            }
            return num;
        }        
        public bool IncludeDebugInformation
        {
            get
            {
                return this.m_includeDebugInfo;
            }
            set
            {
                this.m_includeDebugInfo = value;
            }
        }
    }


}
