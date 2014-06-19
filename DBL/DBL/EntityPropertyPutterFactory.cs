using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DBL
{
    public class EntityPropertyPutterFactory
    {
        /// <summary>
        /// 存储生成的实体操作类
        /// </summary>
        private static readonly Hashtable g_putterHash = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 创建实体操作类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromEntity"></param>
        /// <param name="includeDebugInfo">是否调试</param>
        /// <returns></returns>
        public static IEntityPropertyPutter<T> Create<T>(T fromEntity, string providerName, bool includeDebugInfo) where T : class
        {
            if (fromEntity == null)
            {
                return null;
            }
            if (fromEntity is IEntityPropertyPutter<T>)
            {
                return (IEntityPropertyPutter<T>)fromEntity;
            }
            IEntityPropertyPutter<T> putter = null;
            string fullName = fromEntity.GetType().FullName;
            if (g_putterHash.ContainsKey(fullName))
            {
                return (g_putterHash[fullName] as IEntityPropertyPutter<T>);
            }
            putter = new EntityPropertyPutterMaker { ProviderName = providerName, IncludeDebugInformation = includeDebugInfo }.Make<T>();
            g_putterHash.Add(fullName, putter);
            return putter;
        }
    }


}
