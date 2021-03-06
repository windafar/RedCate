﻿using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Data.SqlClient;
using static Sercher.DomainAttributeEx;
using System.Diagnostics;
using System;

namespace Sercher
{
    public class DocumentDB : ServerDB, IDocumentDB
    {
        public string DoctableName { get => TableInfoAttribute.GetAttribute(typeof(Document)).TableName; }

        public DocumentDB() { }
        public DocumentDB(string ip, string dbName) : base(ip, dbName)
        {
            
        }

        /// <summary>
        /// 更新文档索引标记为已索引
        /// </summary>
        /// <param name="document"></param>
        /// <remarks>2020年4月27日编写，未测试</remarks>
        public void UpdateDocumentStateIndexStatus(int docId,string hasIndexed)
        {
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(string.Format("update {0} set hasIndexed='{1}' where _id={2}", DoctableName, hasIndexed,docId), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { GlobalMsg.globalMsgHand.Invoke(e.Message); }
            coo.Close();

        }
        public void UploadDocument(Document doc)
        {
            //var filter = Builders<Document>.Filter.Eq(x => x._id, doc._id);
            //var updated1 = Builders<Document>.Update
            //    .Set(x => x.hasIndexed, doc.hasIndexed)
            //    .Set(x => x.Name, doc.Name)
            //    .Set(x => x.Url, doc.Url);
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(SqlHelp.insertMuanySql(this.DbName, DoctableName, new Document[1] { doc }), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { GlobalMsg.globalMsgHand.Invoke(e.Message); }
            coo.Dispose();
        }
        public List<Document> GetNotIndexDocument()
        {
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where hasIndexed='no'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Close();
            return SqlHelp.DataSetToList<Document>(ds);
        }

        public void DelDocumentById(int documentId)
        {
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(string.Format(@"DELETE FROM [{0}] WHERE _id = {1}", DoctableName, documentId), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { GlobalMsg.globalMsgHand.Invoke(e.Message); }
            coo.Dispose();
        }

        public List<Document> GetDocuments()
        {
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM[" + this.DbName + "].[dbo].[" + DoctableName + "]", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Close();
            return SqlHelp.DataSetToList<Document>(ds);
        }

        public Document GetDocumentById(int docid)
        {
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where _id='" + docid + "'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Dispose();
            return SqlHelp.DataSetToList<Document>(ds).FirstOrDefault();

            //return documentCollction.Find(x => x._id == docid).First();
        }
        public int GetIndexedDocumentNum()
        {//这儿的where之后改一下

            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlDataAdapter adp = new SqlDataAdapter("SELECT count(1) FROM[" + this.DbName + "].[dbo].[" + DoctableName + "] where hasIndexed='yes'", coo);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            coo.Dispose();
            return (int)ds.Tables[0].Rows[0].ItemArray[0];
        }

        public void CreateDocumentTable()
        {
            string connectionStr = GetSqldbConnectionStr();
            string sql = SqlHelp.CreateTableSql<Document>(DoctableName);
            var coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(sql.ToString(), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { GlobalMsg.globalMsgHand.Invoke(e.Message); }
            coo.Dispose();
        }


        public void ResetDocumentIndexStatus()
        {
            string connectionStr = GetSqldbConnectionStr();
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(string.Format("update {0} set hasIndexed='{1}'", DoctableName, "no"), coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException e) { GlobalMsg.globalMsgHand.Invoke(e.Message); }
            finally {coo.Close(); }
            

        }

    }
}
