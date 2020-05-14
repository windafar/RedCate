using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sercher
{
    /// <summary>
    /// 此标记类主要用于生成表的配置
    /// </summary>
    class DomainAttributeEx
    {
        /// <summary>
        /// 表名标记
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        [Serializable]
        public class TableInfoAttribute : Attribute
        {
            public TableInfoAttribute(string tableName)
            {
                TableName = tableName;
            }

            /// <summary>
            ///     数据库中表的名称
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            ///     获取元数据的特性
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public static TableInfoAttribute GetAttribute(Type item)
            {
                var excludeFieldAttribute = (TableInfoAttribute)GetCustomAttribute(item, typeof(TableInfoAttribute));
                return excludeFieldAttribute;
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        [Serializable]
        public class FiledInfoAttribute : Attribute
        {
            public FiledInfoAttribute(string propertyTypeName, bool CanNull=false,int len=-1)
            {
                PropertyTypeName = propertyTypeName;
                this.CanNull = CanNull;
                this.Len = len;
            }

            public string PropertyTypeName { get; set; }
            public bool CanNull { get; }
            public int Len { get; }


            public static FiledInfoAttribute GetAttribute(MemberInfo member)
            {
                return (FiledInfoAttribute)GetCustomAttribute(member, typeof(FiledInfoAttribute));
            }

        }

        /// <summary>
        /// 自增标记
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        [Serializable]
        public class IdentityAttribute : Attribute
        {
            public static IdentityAttribute GetAttribute(MemberInfo member)
            {
                return (IdentityAttribute)GetCustomAttribute(member, typeof(IdentityAttribute));
            }
        }

    }
}
