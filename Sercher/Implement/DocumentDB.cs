﻿using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Data.SqlClient;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    public class DocumentDB : ServerDB, IDocumentDB
    {
        public string DoctableName { get => TableInfoAttribute.GetAttribute(typeof(Document)).TableName; }

        public DocumentDB() { }
        public DocumentDB(string ip, string dbName) : base(ip, dbName)
        {
            
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
            SqlCommand sqlCommand = new SqlCommand(SqlHelp.insertMuanySql(this.DbName, DoctableName, new Document[1] { doc }), coo);
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
            return SqlHelp.DataSetToList<Document>(ds);
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
            return SqlHelp.DataSetToList<Document>(ds).FirstOrDefault();

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

        public void CreateDocumentTable()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            string sql = SqlHelp.CreateTableSql<Document>(DoctableName);
            var coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(sql.ToString(), coo);
            int status = sqlCommand.ExecuteNonQuery();
            coo.Dispose();
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

    }
}