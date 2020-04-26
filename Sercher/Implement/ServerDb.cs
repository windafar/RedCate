using JiebaNet.Segmenter;
using static Sercher.SercherServerBase;
using Newtonsoft.Json;

using System.Data;
using System.Data.SqlClient;
using System.IO;
using static Sercher.DomainAttributeEx;
using System.ComponentModel.DataAnnotations;

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
        protected string GetSqldbConnectionStr()
        {
            //"Server=joe;Database=AdventureWorks;User ID=sa;Password=test;pooling=true;connection lifetime=0;min pool size = 1;max pool size=40000"
            //if (ip.IndexOf(":") == -1)
            return string.Format("Server={0};Database={1};User ID=sa;Password=123456", this.Ip, this.DbName);
            //else
            // return string.Format("mongodb://{0}", ip);
        }
        static protected string GetSqldbConnectionStr(string ip, string DatabaseName)
        {
            //"Server=joe;Database=AdventureWorks;User ID=sa;Password=test;pooling=true;connection lifetime=0;min pool size = 1;max pool size=40000"
            //if (ip.IndexOf(":") == -1)
            return string.Format("Server={0};Database={1};User ID=sa;Password=123456", ip, DatabaseName);
            //else
            // return string.Format("mongodb://{0}", ip);
        }

        public ServerDB() { }
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
        public bool CreateDB()
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
}
