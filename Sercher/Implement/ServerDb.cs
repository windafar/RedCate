using JiebaNet.Segmenter;
using static Sercher.SercherServerBase;
using Newtonsoft.Json;

using System.Data;
using System.Data.SqlClient;
using System.IO;
using static Sercher.DomainAttributeEx;
using System.ComponentModel.DataAnnotations;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Sercher
{
    public class ServerDB : IServerDB
    {
        string dbName;
        int status;
        string ip;
        private string physicaTableSuffix="#)#";
        int eachTableMaxItemNum = 10000000;
        Dictionary<string, List<string>> EachTableDic = new Dictionary<string, List<string>>();
        public string Ip { get => ip; set => ip = value; }
        public string DbName { get => dbName; set => dbName = value; }
        public int Status { get => status; set => status = value; }
        public string PhysicaTableSuffix { get => physicaTableSuffix; set => physicaTableSuffix = value; }
        public int EachTableMaxItemNum { get => eachTableMaxItemNum; set => eachTableMaxItemNum = value; }

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
        public ServerDB(string ip, string dbName)
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
            try
            {
                adp.Fill(ds);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            int value = ds.Tables[0].Rows.Count;
            ds.Dispose();
            adp.Dispose();
            coo.Dispose();
            return value != 0;
        }
        public bool CreateDB(string FileDir = null)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, "master");
            string dbDirPath = Config.CurrentConfig.DefaultDbDirPath;
            string createSql = string.Format(@"
                        CREATE DATABASE {0} 
                        ON
                        (
                            NAME = {0},
                            FILENAME = '{1}\{0}_database.mdf',
                            SIZE = 5MB,
                            FILEGROWTH = 100
                        )", this.DbName, dbDirPath);
            var coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(createSql, coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            coo.Dispose();
            return true;
        }
        public void DeleDb()
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, "master");
            SqlConnection coo = new SqlConnection(connectionStr);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand("drop database " + this.DbName, coo);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            coo.Dispose();
        }

        public Tuple<string, string> GetDbFilePath()
        {
            string sql = string.Format(@"select filename from {0}.dbo.sysfiles", this.DbName);
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            SqlDataAdapter adp = new SqlDataAdapter(sql, coo);
            DataSet ds = new DataSet();
            try
            {
                adp.Fill(ds);
                return new Tuple<string, string>(ds.Tables[0].Rows[0].ItemArray[0] as string,
                   ds.Tables[0].Rows[1].ItemArray[0] as string);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                coo.Dispose();
                return null;
            }

        }
        public bool BackupTo(string path)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            string sql = string.Format(@"Backup database {0} to disk = '{1}'", DbName, path);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(sql.ToString(), coo); ;
            sqlCommand.CommandTimeout = 600;
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                coo.Dispose();
                throw e;
                // return false;
            }
            coo.Dispose();


            return true;
        }
        public bool RestoreFrom(string path)
        {
            string connectionStr = GetSqldbConnectionStr(this.Ip, this.DbName);
            SqlConnection coo = new SqlConnection(connectionStr);
            string sql = string.Format(@"use master;restore database {0} from disk = '{1}' with REPLACE", DbName, path);
            coo.Open();
            SqlCommand sqlCommand = new SqlCommand(sql.ToString(), coo); ;
            sqlCommand.CommandTimeout = 600;
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                coo.Dispose();
                throw e;
            }
            coo.Dispose();


            return true;
        }

        public IDictionary<string, List<string>> GetTableGroup()
        {

            //string connectionStr = GetSqldbConnectionStr(Ip, DbName);
            //SqlConnection coo = new SqlConnection(connectionStr);
            //SqlDataAdapter adp = new SqlDataAdapter("select name from sysobjects where xtype = 'u'", coo);
            //DataSet ds = new DataSet();
            //adp.Fill(ds);
            //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            //{
            //    string value = (string)ds.Tables[0].Rows[i].ItemArray[0];
            //    string key = value.Substring(0, value.IndexOf(PhysicaTableSuffix));
            //    if (dic.ContainsKey(key))
            //        dic[key].Add(value);
            //    else dic[key] = new List<string>() { value };
            //}
            //coo.Dispose();
            //return dic;
            throw new NotImplementedException();
        }
    }
}
