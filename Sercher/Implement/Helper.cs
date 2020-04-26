using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    static class SqlHelp
    {
        /// <summary>
        /// 拼接sql批量插入语句，最大尺寸限制在1M ,1*1024*1024/102B=10240大概1w条数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <param name="prams"></param>
        /// <returns></returns>
       static public string insertMuanySql<T>(string dbName, string tableName, T[] objList, string CreateFromTempletteTable = "")
        {//不知道一个方法中反射会不会自动缓存，最后测试
            var Pros = typeof(T).GetProperties();
            var ProsNamelist = Pros.Where(x => IdentityAttribute.GetAttribute(x) == null).Select(x => x.Name);//排除自增属性
            string Sqlpramslist = "(" + string.Join(",", ProsNamelist) + ")";
            StringBuilder stringBuilder = new StringBuilder("INSERT INTO [" + dbName + "].[dbo].[" + tableName + "]" + Sqlpramslist + " VALUES ");
            foreach (var drow in objList)
            {
                stringBuilder.Append("(");
                foreach (var dcol in Pros)
                {
                    if (IdentityAttribute.GetAttribute(dcol) != null) continue;//排除自增属性
                    var value = dcol.GetValue(drow).ToString();
                    stringBuilder.Append("'" + value + "',");
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.Append("),");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            if (CreateFromTempletteTable != "")
            {
                StringBuilder createBudiler = new StringBuilder(string.Format(@"
                    if not exists (select * from dbo.sysobjects where id = object_id(N'{0}.dbo.{1}') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
                    begin
                    select * into {0}.dbo.[{1}] from {2}
                    end 
                    ", dbName, tableName, CreateFromTempletteTable));//从模板创建表
                stringBuilder.Insert(0, createBudiler);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// DataSet转换成指定返回类型的实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ds"></param>
        /// <returns></returns>
        static public List<T> DataSetToList<T>(DataSet ds)
        {
            var properties = typeof(T).GetProperties();
            List<T> list = new List<T>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                T temp = System.Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    int colIndex = ds.Tables[0].Columns.IndexOf(pro.Name);
                    if (colIndex > -1)
                    {
                        if (pro.PropertyType == typeof(String))
                        {
                            pro.SetValue(temp, dr[colIndex].ToString(), null);
                        }
                        else if (pro.PropertyType == typeof(int))
                        {
                            pro.SetValue(temp, (int)dr[colIndex], null);
                        }
                        else if (pro.PropertyType == typeof(long))
                        {
                            pro.SetValue(temp, (long)dr[colIndex], null);
                        }
                    }
                }
                list.Add(temp);
            }
            return list;
        }

        static public string ConvertType2Sql(string typename)
        {
            typename = typename.ToLower();
            switch (typename)
            {
                case "string":
                    return "nchar";
                case "int32":
                    return "int";
                case "int64":
                    return "bigint";
                case "long":
                    return "bigint";
                case "bool":
                    return "bit";
                case "double":
                    return "float";
                case "short":
                    return "smallint";
                case "byte":
                    return "tinyint";
                case "object":
                    return "binary";
                default:
                    return typename;
            }
        }

        static public string CreateTableSql<T>(string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("CREATE TABLE [dbo].[{0}](", tableName));
            bool haskey = false;
            foreach (var dcol in typeof(T).GetProperties())
            {
                var filedAttr = FiledInfoAttribute.GetAttribute(dcol);
                string typename = filedAttr != null ? filedAttr.PropertyTypeName : "";
                int len = filedAttr != null ? filedAttr.Len : -1;
                string cannull = filedAttr != null ? filedAttr.CanNull ? "NULL" : "NOT NULL" : "NOT NULL";
                if (string.IsNullOrWhiteSpace(typename))
                    typename = SqlHelp.ConvertType2Sql(dcol.PropertyType.Name);
                stringBuilder.Append(string.Format("[{0}] [{1}]", dcol.Name, typename));
                if (len != -1)
                    stringBuilder.Append(string.Format("({0})", len));
                if (IdentityAttribute.GetAttribute(dcol) != null)
                    stringBuilder.Append("IDENTITY(1,1)");
                stringBuilder.Append(string.Format(" {0}", cannull));
                stringBuilder.Append(",");
                if (KeyAttribute.IsDefined(dcol, typeof(KeyAttribute)))
                {
                    stringBuilder.Append(string.Format(@"CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}] ASC),", tableName, dcol.Name));
                    haskey = true;
                }
            }
            if (!haskey) throw new InvalidOperationException("you need appoint PRIMARY use KeyAttribute " + typeof(T).Name + " above");

            stringBuilder.Append(") ON [PRIMARY]");

            return stringBuilder.ToString();
        }

    }
    /// <summary>
    /// sql迁徙工具相关
    /// </summary>
    public static class SqlConnectionExtension
    {
        /// <summary>
        /// 使用 SqlBulkCopy 向 destinationTableName 表插入数据
        /// </summary>
        /// <typeparam name="TModel">必须拥有与目标表所有字段对应属性</typeparam>
        /// <param name="conn"></param>
        /// <param name="modelList">要插入的数据</param>
        /// <param name="batchSize">SqlBulkCopy.BatchSize</param>
        /// <param name="destinationTableName">如果为 null，则使用 TModel 名称作为 destinationTableName</param>
        /// <param name="bulkCopyTimeout">SqlBulkCopy.BulkCopyTimeout</param>
        /// <param name="externalTransaction">要使用的事务</param>
        public static void BulkCopy<TModel>(this SqlConnection conn, List<TModel> modelList, int batchSize, string destinationTableName = null, int? bulkCopyTimeout = null, SqlTransaction externalTransaction = null)
        {
            bool shouldCloseConnection = false;

            if (string.IsNullOrEmpty(destinationTableName))
                destinationTableName = typeof(TModel).Name;

            DataTable dtToWrite = ToSqlBulkCopyDataTable(modelList, conn, destinationTableName);

            SqlBulkCopy sbc = null;

            try
            {
                if (externalTransaction != null)
                    sbc = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, externalTransaction);
                else
                    sbc = new SqlBulkCopy(conn);

                using (sbc)
                {
                    sbc.BatchSize = batchSize;
                    sbc.DestinationTableName = destinationTableName;

                    if (bulkCopyTimeout != null)
                        sbc.BulkCopyTimeout = bulkCopyTimeout.Value;

                    if (conn.State != ConnectionState.Open)
                    {
                        shouldCloseConnection = true;
                        conn.Open();
                    }

                    sbc.WriteToServer(dtToWrite);
                }
            }
            finally
            {
                if (shouldCloseConnection && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public static DataTable ToSqlBulkCopyDataTable<TModel>(List<TModel> modelList, SqlConnection conn, string tableName)
        {
            DataTable dt = new DataTable();

            Type modelType = typeof(TModel);

            List<SysColumn> columns = GetTableColumns(conn, tableName);
            List<PropertyInfo> mappingProps = new List<PropertyInfo>();

            var props = modelType.GetProperties();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                PropertyInfo mappingProp = props.Where(a => a.Name == column.Name).FirstOrDefault();
                if (mappingProp == null)
                    throw new Exception(string.Format("model 类型 '{0}'未定义与表 '{1}' 列名为 '{2}' 映射的属性", modelType.FullName, tableName, column.Name));

                mappingProps.Add(mappingProp);
                Type dataType = GetUnderlyingType(mappingProp.PropertyType);
                if (dataType.IsEnum)
                    dataType = typeof(int);
                dt.Columns.Add(new DataColumn(column.Name, dataType));
            }

            foreach (var model in modelList)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < mappingProps.Count; i++)
                {
                    PropertyInfo prop = mappingProps[i];
                    object value = prop.GetValue(model);

                    if (GetUnderlyingType(prop.PropertyType).IsEnum)
                    {
                        if (value != null)
                            value = (int)value;
                    }

                    dr[i] = value ?? DBNull.Value;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
        static List<SysColumn> GetTableColumns(SqlConnection sourceConn, string tableName)
        {
            string sql = string.Format("select syscolumns.name,colorder from syscolumns inner join sysobjects on syscolumns.id=sysobjects.id where sysobjects.xtype='U' and sysobjects.name='{0}' order by syscolumns.colid asc", tableName);

            List<SysColumn> columns = new List<SysColumn>();
            using (SqlConnection conn = (SqlConnection)((ICloneable)sourceConn).Clone())
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(sql);
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SysColumn column = new SysColumn();
                        //column.Name = reader.GetDbValue("name");
                        //column.ColOrder = reader.GetDbValue("colorder");
                        column.Name = reader.GetString(1);
                        column.ColOrder = reader.GetInt32(2);
                        columns.Add(column);
                    }
                }
                conn.Close();
            }

            return columns;
        }

        static Type GetUnderlyingType(Type type)
        {
            Type unType = Nullable.GetUnderlyingType(type); ;
            if (unType == null)
                unType = type;

            return unType;
        }

        class SysColumn
        {
            public string Name { get; set; }
            public int ColOrder { get; set; }
        }
    }
    /// <summary>
    /// 序列化工具相关
    /// </summary>
    public static class SerializeHelper
    {
        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        static public byte[] SerializeObject(object pObj)
        {
            if (pObj == null)
                return null;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(_memory, pObj);
            _memory.Position = 0;
            byte[] read = new byte[_memory.Length];
            _memory.Read(read, 0, read.Length);
            _memory.Close();
            return read;
        }
        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
                 /// <returns>object</returns>
        static public object DeserializeObject(byte[] pBytes)
        {
            object _newOjb = null;
            if (pBytes == null)
                return _newOjb;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream(pBytes);
            _memory.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            _newOjb = formatter.Deserialize(_memory);
            _memory.Close();
            return _newOjb;
        }

        static public void SerializerXml<T>(object obj, string path)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            FileStream fs = File.Create(path);
            xs.Serialize(fs, obj);
            fs.Dispose();
        }
        static public T DeserializeXml<T>(string path)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            FileStream fs = File.OpenRead(path);
            T OBJ = (T)xs.Deserialize(fs);
            fs.Dispose();
            return OBJ;
        }


    }

}