using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using static Sercher.DomainAttributeEx;
using System.IO;

namespace Sercher
{
    public class SercherIndexesDB : ServerDB, ISercherIndexesDB
    {
        int wordCount;
        bool isOkey;//迁移时要是删除重复映射的时候词太多太占内存可以用这个字段和带过滤的findbigger方法实时更新可用映射
        public int IndexesTableCount { get => wordCount; set => wordCount = value; }
        public string SercherIndexesTableTemplate { get; set; }

        public static HashSet<string> GetWords(IEnumerable<ISercherIndexesDB> sercherIndexesDBs)
        {
            HashSet<string> hashset = new HashSet<string>();
            foreach (var db in sercherIndexesDBs)
            {
                string connectionStr = GetSqldbConnectionStr(db.Ip, db.DbName);
                SqlConnection coo = new SqlConnection(connectionStr);
                SqlDataAdapter adp = new SqlDataAdapter("select name from sysobjects where xtype = 'u'", coo);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    hashset.Add((string)ds.Tables[0].Rows[i].ItemArray[0]);
                }
                coo.Dispose();
            }
            return hashset;
        }

        static Dictionary<string, SqlConnection> SqlConnectionCollection = new Dictionary<string, SqlConnection>();
        private object getindexdbobj = new object();

        public SercherIndexesDB() { }

        public SercherIndexesDB(string ip, string dbName) : base(ip, dbName)
        {
            SercherIndexesTableTemplate = TableInfoAttribute.GetAttribute(typeof(DocumentIndex)).TableName;
        }
        /// <summary>
        /// 使用selectinto创建若干表
        /// </summary>
        /// <param name="names"></param>
        public void CreateIndexTable(string[] names)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            StringBuilder createBudiler = new StringBuilder();
            coo.Open();
            int i = 0, j = 0;
            foreach (var name in names)
            {//为selectinto语句分段执行
                i++;
                createBudiler.Append(string.Format(@"select * into {0}.dbo.[{1}] from {2};
                    ", this.DbName, name, SercherIndexesTableTemplate));//从模板创建表

                if (i == 500 || j * 500 + i == names.Count())
                {
                    j++; i = 0;
                    SqlCommand sqlCommand = new SqlCommand(createBudiler.ToString(), coo);
                    sqlCommand.CommandTimeout = 600;

                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    createBudiler.Clear();
                }
            }
            coo.Dispose();
        }

        /// <summary>
        /// 使用selectinto复制若干表
        /// </summary>
        /// <param name="names"></param>
        public void CreateIndexTable(string[] names,string SourceDbName)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            StringBuilder createBudiler = new StringBuilder();
            coo.Open();
            int i = 0, j = 0;
            foreach (var name in names)
            {//为selectinto语句分段执行
                i++;
                createBudiler.Append(string.Format(@"select * into [{0}].dbo.[{1}] from [{2}].dbo.[{1}];
                    ", this.DbName, name, SourceDbName));//从模板创建表

                if (i == 5000 || j * 5000 + i == names.Count())
                {
                    j++; i = 0;
                    SqlCommand sqlCommand = new SqlCommand(createBudiler.ToString(), coo);
                    sqlCommand.CommandTimeout = 600;

                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (SqlException e) 
                    {
                        Debug.Print(e.Message);
                    }
                    createBudiler.Clear();
                }
            }
            coo.Dispose();
        }


        public SqlConnection GetSercherIndexDb()
        {//多线程下出错
            SqlConnection sqlConnection;
            if (SqlConnectionCollection.TryGetValue(this.Ip + this.DbName, out var value))
            {
                sqlConnection = value;
            }
            else
            {
                string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
                SqlConnection _client = new SqlConnection(connectionStr);
                try
                {
                    _client.Open();
                }
                catch (Exception e)
                {//若不存在此数据库

                }

                SqlConnectionCollection.Add(this.Ip + this.DbName, _client);
                sqlConnection = _client;
            }
            return sqlConnection;
        }
        public int GetSercherIndexCollectionCount()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter("SELECT count(1) from sysobjects where xtype = 'u'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            return (int)ds.Tables[0].Rows[0].ItemArray[0];
        }
        public List<string> GetSercherIndexCollectionNameList()
        {//这方法好像有问题
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter("SELECT name from sysobjects where xtype = 'u'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            List<string> list = new List<string>();
            foreach (var item in ds.Tables[0].Rows)
            {
                list.Add(((System.Data.DataRow)item).ItemArray[0].ToString());
            }
            return list;
        }

        public void UploadDocumentIndex(string word, DocumentIndex[] documentIndex)
        {
            var coo = GetSercherIndexDb();
            SqlCommand sqlCommand = new SqlCommand(
                SqlHelp.insertMuanySql(this.DbName, word, documentIndex, SercherIndexesTableTemplate)
                , coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { Debug.Print(e.Message); }
        }

        public void UploadDocumentIndex(string[] sqls)
        {
            SqlConnection coo;
            lock (getindexdbobj)
            {
                coo = GetSercherIndexDb();
            }

            int i = 0;
            int j = 0;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var sql in sqls)
            {
                i++;
                stringBuilder.Append(sql);
                if (stringBuilder.Length > 5000000 || i == sqls.Count())
                {
                    SqlTransaction transaction = coo.BeginTransaction();
                    SqlCommand sqlCommand = new SqlCommand(stringBuilder.ToString(), coo);
                    sqlCommand.CommandTimeout = 600;
                    sqlCommand.Transaction = transaction;
                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (SqlException e)
                    {
                        if (e.Number == 2714)
                        {
                            //若已经存在此表 
                        }
                        transaction.Rollback();
                        Debug.WriteLine(e.Message);

                    }
                    stringBuilder.Clear();
                }
            }
        }


        /// <summary>
        /// 计算某个单词的相关性
        /// </summary>
        /// <param name="ArticleTotal">文档总数</param>
        /// <param name="ArctNumInCurWord">当前单词命中文档数</param>
        /// <param name="CurArctWordNum">当前文档单词总数</param>
        /// <param name="CurArctHitNum">当前文档命中单词数</param>
        /// <returns></returns>
        /// <remarks>参考TF-IDE公式，2017年4月23日15:00:33</remarks>
        public double TF_IDE_CountFunc(int ArticleTotal, int ArctNumInCurWord, int CurArctWordNum, int CurArctHitNum)
        {
            double TF = CurArctHitNum / (double)CurArctWordNum;

            double IDE = Math.Log(ArticleTotal / ArctNumInCurWord, 2);
            return TF * IDE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word">搜索的单词</param>
        /// <param name="doctotal">文档总数</param>
        /// <param name="resultAction">对每条结果的返回处理</param>
        /// <param name="pagesize">页面大小</param>
        /// <param name="pagenum">页数</param>
        /// <param name="permission">权限值，大于该值的才会返回，默认为最大值10</param>
        public void GetSercherResultFromIndexesDB(string word, int doctotal, Action<int, double, IList<int>> resultAction, int pagesize = 10, int pagenum = 1,int permission = 0)
        {
            string func = string.Format(@"DECLARE @ide float
                    select @ide= LOG({0}.0*10/COUNT(1)) from {4}.dbo.[{1}]
                    SELECT TOP {2} DocId, WordFrequency*1./DocumentWordTotal*@ide as tfide,BeginIndex
                    FROM(
                     SELECT ROW_NUMBER() OVER (ORDER BY WordFrequency*1./DocumentWordTotal*@ide desc) AS RowNumber,* 
                     FROM {4}.dbo.[{1}] 
                     WHERE  Permission<={5})as A 
                    WHERE RowNumber > {2}*({3}-1)", doctotal, word, pagesize, pagenum, DbName, permission);

            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter(func, coo);
            DataSet ds = new DataSet();
            try
            {
                adp.Fill(ds);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //此单词未在索引中
                //2020年4月20日追记，对不存在的单词不要加入概率估计
                coo.Dispose();
                return;
            }
            List<string> list = new List<string>();
            for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
            {
                int docid = (int)ds.Tables[0].Rows[r].ItemArray[0];
                double tf_ide = (double)ds.Tables[0].Rows[r].ItemArray[1];
                IList<int> beginindex = ((string)ds.Tables[0].Rows[r].ItemArray[2])
                    .Split(new char[1] { ',' },StringSplitOptions.RemoveEmptyEntries)
                    .Select(x=>int.Parse(x)).ToList();
                resultAction(docid, tf_ide, beginindex);
            }
            coo.Dispose();
        }


        /// <summary>
        /// 迁移数据表
        /// </summary>
        /// <param name="maxdb">最大负载的目标DB</param>
        /// <param name="tableNamelist">需要迁移的表</param>
        /// <returns></returns>
        /// <remarks>
        ///过程如下：
        /// 在服务器上创建临时数据库
        ///执行select into脚本到临时数据库
        ///拷贝数据库文件到目标数据库
        ///附加到sql服务
        ///注意点：
        ///创建临时数据库操作需要额外的空间
        ///select into脚本可能会因过多而爆内存异常，需分段执行
        ///附加到sql服务的时候可能会有权限问题
        /// </remarks>
        public object ImmigrationOperation(ISercherIndexesDB maxdb, List<string> tableName)
        {
            ////连接到待迁移数据库并导出到临时数据库
            //SercherIndexesDB sercherIndexesDB = new SercherIndexesDB(maxdb.Ip, "ImmigrationDB");
            //sercherIndexesDB.CreateDB();
            //sercherIndexesDB.CreateIndexTable(tableName.ToArray(), maxdb.DbName);
            //try
            //{
            //    sercherIndexesDB.CreateIndexTemplateTable();
            //}
            //catch (SqlException){ }
            ////1 拷贝出数据库
            ////此处可以使用备份还原的方式进行热转移，也可以自行拷贝ImmigrationDB数据库并手动附加
            ////DataBaseControl dataBaseControl = new DataBaseControl()
            ////{
            ////    ConnectionString = sercherIndexesDB.GetSqldbConnectionStr(),
            ////    DataBaseName = sercherIndexesDB.DbName,
            ////    DataBase_MDF = filepath[0],
            ////    DataBase_LDF = filepath[1],
            ////};
            ////dataBaseControl.detachDB();
            ////
            ////##:2 备份还原的代码如下
            //this.CreateDB();

            //var filepath = this.GetDbFilePath().Item1;//获取当前数据库文件（目标数据库）的位置
            //var NetPath = NetTools.GetShareName(filepath).Item1;//获取目标数据库网络位置
            //sercherIndexesDB.BackupTo(NetPath);//从网络路径备份数据库
            //this.RestoreFrom(filepath);//还原此数据库
            ////##
            ///
            //#下面我直接在本地数据库拷贝
            SercherIndexesDB sercherIndexesDB = new SercherIndexesDB(maxdb.Ip, this.DbName);
            sercherIndexesDB.CreateDB();
            sercherIndexesDB.CreateIndexTable(tableName.ToArray(), maxdb.DbName);
            try
            {
                sercherIndexesDB.CreateIndexTemplateTable();
            }
            catch (SqlException) { }

            return tableName;
        }


        public void CreateIndexTemplateTable()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            string sql = SqlHelp.CreateTableSql<DocumentIndex>(SercherIndexesTableTemplate);
            var coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(sql.ToString(), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { Debug.Print(e.Message); }
            coo.Dispose();
        }

        public void ReIndexesDocument(Document document)
        {
            throw new NotImplementedException();
        }
    }
}
