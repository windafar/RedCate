using System;
using System.Collections.Generic;
using System.Text;
using JiebaNet.Segmenter;
using static Sercher.SercherServerBase;
using Newtonsoft.Json;
using System.Linq;

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using System.IO;
namespace Sercher
{
    public class ServerDB : IServerDB
    {
        string dbName;
        int status;
        string ip;
        public string Ip { get => ip; set => ip = value; }
        public string DbName { get => dbName; set => dbName = value; }
        public int Status { get => status; set => status = value; }
        protected string GetSqldbConnectionStr(string ip, string DatabaseName)
        {
            //"Server=joe;Database=AdventureWorks;User ID=sa;Password=test;pooling=true;connection lifetime=0;min pool size = 1;max pool size=40000"
            //if (ip.IndexOf(":") == -1)
            return string.Format("Server={0};Database={1};User ID=sa;Password=123456", ip, DatabaseName);
            //else
            // return string.Format("mongodb://{0}", ip);
        }

        public ServerDB(string ip,string dbName)
        {
            this.Ip = ip;
            this.DbName = dbName;
        }

        public bool GetdbStatus()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, "master");
            string selectSQL = "select * From master.dbo.sysdatabases where name='" + this.DbName + "'";

            var coo = new SqlConnection(connectionStr);
            //coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter(selectSQL, coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            int value = ds.Tables[0].Rows.Count;
            ds.Dispose();
            adp.Dispose();
            coo.Dispose();
            return value != 0;
        }
        public bool CreateSqlDB()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, "master");
            string createSql = string.Format(@"
                        CREATE DATABASE {0} 
                        ON
                        (
                            NAME = {0},
                            FILENAME = 'E:\{0}_database.mdf',
                            SIZE = 5MB,
                            FILEGROWTH = 100
                        )", this.DbName);
            var coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(createSql, coo);
            int status = sqlCommand.ExecuteNonQuery();
            coo.Dispose();
            return status == -1;
        }
        public void DeleDb()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, "master");
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand("drop database " + this.DbName, coo);
            sqlCommand.ExecuteNonQuery();
            coo.Close();
        }

    }


    public class SercherIndexesDB : ServerDB, ISercherIndexesDB
    {
        int worldCount;


        bool isOkey;//要是删除重复映射的时候词太多太占内存可以用这个字段和带过滤的findbigger方法实时更新可用映射
        public int TableCount { get => worldCount; set => worldCount = value; }


        static Dictionary<string, SqlConnection> SqlConnectionCollection = new Dictionary<string, SqlConnection>();

        public SercherIndexesDB(string ip, string dbName) : base(ip, dbName)
        {
        }

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
                    ", this.DbName, name, "mydb.dbo.documentIndices"));//从模板创建表

                if (i == 5000 || j * 5000 + i == names.Count())
                {
                    j++; i = 0;
                    SqlCommand sqlCommand = new SqlCommand(createBudiler.ToString(), coo);
                    sqlCommand.CommandTimeout = 120;

                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {

                    }
                    createBudiler.Clear();
                }
            }
            coo.Dispose();
        }

        public SqlConnection GetSercherIndexDb()
        {
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
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter("SELECT count(1) from sysobjects where xtype = 'u'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            List<string> list = new List<string>();
            foreach (var item in ds.Tables[0].Columns)
            {
                list.Add(item.ToString());
            }
            return list;
        }

        public void UploadDocumentIndex(string world, DocumentIndex[] documentIndex)
        {
            var coo = GetSercherIndexDb();
            SqlCommand sqlCommand = new SqlCommand(
                Help.insertMuanyQure(this.DbName, world, documentIndex, "mydb.dbo.documentIndices")
                , coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {//若不存在此表

            }
        }

        public void UploadDocumentIndex(string[] sqls)
        {
            var coo = GetSercherIndexDb();
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
                    sqlCommand.CommandTimeout = 120;
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
                    }
                    stringBuilder.Clear();
                }
            }
        }

        public List<DocumentIndex> GetDocumentIndex(string collectionName, int start, int length, out long total)
        {
            throw new NotImplementedException();

            //IMongoDatabase _database = GetSercherIndexDb(serverDB);
            //var collection = _database.GetCollection<DocumentIndex>(collectionName);
            //var f = collection.Find(new BsonDocument("_id", new BsonDocument("$exists", true)));
            //total = f.CountDocuments();
            //return f.Skip(start).Limit(length).ToList();
        }

        public List<DocumentIndex> GetDocumentIndexId(string collectionName)
        {
            throw new NotImplementedException();

            //IMongoDatabase _database = GetSercherIndexDb(serverDB);
            //var collection = _database.GetCollection<DocumentIndex>(collectionName);
            //var f = collection.Find(new BsonDocument("_id", new BsonDocument("$exists", true))); ;
            ////.Project(x=>new worldToDocid {Docid=x.DocId,Worldid=x._id });
            //return f.ToList();
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

        public void GetSercherResultFromSQLDB(string world, int doctotal, Action<int, double> action, int pagesize = 10, int pagenum = 1)
        {
            string func = string.Format(@"DECLARE @ide float
                    select @ide= LOG({0}.0/COUNT(1)) from SercherIndexDatabaseA.dbo.[{1}]
                    SELECT TOP {2} DocId, WordFrequency*1./DocumentWorldTotal*@ide as tfide
                    FROM(
                     SELECT ROW_NUMBER() OVER (ORDER BY WordFrequency*1./DocumentWorldTotal*@ide desc) AS RowNumber,* 
                     FROM SercherIndexDatabaseA.dbo.[{1}] )as A 
                    WHERE RowNumber > {2}*({3}-1)", doctotal, world, pagesize, pagenum);

            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter(func, coo);
            DataSet ds = new DataSet();
            try
            {
                adp.Fill(ds);
            }
            catch (Exception e)
            {//此单词未在索引中
                coo.Dispose();
                return;
            }
            List<string> list = new List<string>();
            for (int r = 0; r < ds.Tables[0].Rows.Count; r++)
            {
                int docid = (int)ds.Tables[0].Rows[r].ItemArray[0];
                double tf_ide = (double)ds.Tables[0].Rows[r].ItemArray[1];
                action(docid, tf_ide);
            }
            coo.Dispose();
        }

        /// <summary>
        /// 迁移数据表
        /// </summary>
        /// <param name="maxdb">最大负载的目标DB</param>
        /// <param name="tableNamelist">需要迁移的表</param>
        /// <returns></returns>
        public object ImmigrationOperation(ISercherIndexesDB maxdb, List<string> tableName)
        {
            //在服务器上创建临时数据库
            //执行select into脚本到临时数据库
            //拷贝数据库文件到目标数据库
            //附加到sql服务
            //注意点：
            //创建临时数据库操作需要额外的空间
            //select into脚本可能会因过多而爆内存异常，需分段执行
            //附加到sql服务的时候可能会有权限问题
            throw new NotImplementedException();
        }
    }

    public class DocumentDB : ServerDB, IDocumentDB
    {
        public string DoctableName { get; }
        public string WordstableName { get; }

        public DocumentDB(string ip, string dbName,string doctableName,string wordstableName) : base(ip, dbName)
        {
            DoctableName = doctableName;
            WordstableName = wordstableName;
        }

        // static string this.DbName = "mydb";//文档库
        // static string DoctableName = "documents";//文档表，目前只有一个表
        // static string docdbIP = "WIN-T9ASCBISP3P\\MYSQL";

        /// <summary>
        /// 更新文档索引标记为已索引
        /// </summary>
        /// <param name="document"></param>
        //public void UpdateDocumentStateToIndexed(this Document document, Document.HasIndexed hasIndexed)
        //{
        //    //var filter = Builders<Document>.Filter.Eq(x => x._id, document._id);
        //    //var updated1 = Builders<Document>.Update.Set(x => x.hasIndexed, hasIndexed);
        //    //var resultx = documentCollction.UpdateOne(filter, updated1);
        //}
        public void UploadDocument(Document doc)
        {
            //var filter = Builders<Document>.Filter.Eq(x => x._id, doc._id);
            //var updated1 = Builders<Document>.Update
            //    .Set(x => x.hasIndexed, doc.hasIndexed)
            //    .Set(x => x.Name, doc.Name)
            //    .Set(x => x.Url, doc.Url);
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(Help.insertMuanyQure(this.DbName, DoctableName, new Document[1] { doc }), coo);
            sqlCommand.ExecuteNonQuery();
            coo.Close();
        }
        public List<Document> GetNotIndexDocument()
        {
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            string connectionStr = GetSqldbConnectionStr(Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where hasIndexed='0'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Close();
            return Help.DataSetToList<Document>(ds);
        }
        public Document GetDocumentById(int docid)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where _id='" + docid + "'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Dispose();
            return Help.DataSetToList<Document>(ds).FirstOrDefault();

            //return documentCollction.Find(x => x._id == docid).First();
        }
        public int GetDocumentNum()
        {//这儿的where之后改一下

            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT count(1) FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where hasIndexed='0'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Dispose();
            return (int)ds.Tables[0].Rows[0].ItemArray[0];
        }


        public void UpdateDocument(Document doc, string ip)
        {
            //var filter = Builders<Document>.Filter.Eq(x => x._id, doc._id);
            //var updated1 = Builders<Document>.Update
            //    .Set(x => x.hasIndexed, doc.hasIndexed)
            //    .Set(x => x.Name, doc.Name)
            //    .Set(x => x.Url, doc.Url);
            //documentCollction.UpdateOne(filter, updated1, new UpdateOptions() { IsUpsert = false });
            ////documentCollction.InsertOne(doc);
        }
        public HashSet<string> GetWords()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter("SELECT word from words", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            HashSet<string> hashset = new HashSet<string>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                hashset.Add((string)ds.Tables[0].Rows[i].ItemArray[0]);
            }
            coo.Dispose();
            return hashset;
        }
        public void UploadWord(string[] words)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlCommand sqlCommand = new SqlCommand(Help.insertMuanyQure(this.DbName, "words", words), coo);
            sqlCommand.ExecuteNonQuery();
            coo.Dispose();
        }

    }
}
