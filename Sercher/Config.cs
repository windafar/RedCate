using Component;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sercher
{
    /// <summary>
    /// 初始化全局配置
    /// </summary>
    public class Config
    {
        [XmlIgnore]
        static public Config CurrentConfig;
        [XmlIgnore]
        static string ConfigFilePath = "Config.xml";
        private string documentsDBName = "mydb";
        private string documentsDBIp = "WIN-T9ASCBISP3P\\MYSQL";
        List<SercherIndexesDB> indexesServerlists;
        private int defaultPermission = 0;
        private string defaultDbDirPath=@"E:";
        int maxIndexCachWordNum = 10000000;

        /// <summary>
        /// 文档数据库名
        /// </summary>
        public string DocumentsDBName { get => documentsDBName; set => documentsDBName = value; }
        /// <summary>
        /// 文档数据库IP或者名称
        /// </summary>
        public string DocumentsDBIp { get => documentsDBIp; set => documentsDBIp = value; }
        /// <summary>
        /// 初始化的索引服务列表
        /// </summary>
        public List<SercherIndexesDB> IndexesServerlists { get => indexesServerlists; set => indexesServerlists = value; }
        /// <summary>
        /// DefaultPermission 会在索引或者创建文档时被写入，用来过滤搜索
        /// the DefaultPermission will be write in during indexes and document create
        /// and be used to filter search result
        /// default value is zero, range from int.min to int.max
        /// </summary>
        public int DefaultPermission { get => defaultPermission; set => defaultPermission = value; }
        public string DefaultDbDirPath { get => defaultDbDirPath; set => defaultDbDirPath = value; }
        /// <summary>
        /// 索引所用的最大缓存
        /// </summary>
        public int MaxIndexCachWordNum { get => maxIndexCachWordNum; set => maxIndexCachWordNum = value; }

        public Config() { }
        /// <summary>
        /// 初始化配置，若存在配置文件则使用配置文件
        /// </summary>
        /// <param name="reinit"></param>
        /// <returns></returns>
        static public Config Init(bool reinit = false)
        {
            if (!reinit && (CurrentConfig != null|| Config.LoadConfig())) return CurrentConfig;
            CurrentConfig = new Config();
            CurrentConfig.IndexesServerlists = new List<SercherIndexesDB>();
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseA")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseB")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseC")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseD")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseE")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseF")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseG")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseH")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseI")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseJ")
                    );
            //test
            //CurrentConfig.IndexesServerlists.ForEach(x => x.DeleDb());


            //为不存在的索引数据库创建，并初始化模板表
            CurrentConfig.IndexesServerlists.ForEach(x =>
            {
                if (!x.GetdbStatus())
                {
                    x.CreateDB();
                    x.CreateIndexTemplateTable();
                }
            });
            try
            {
                CurrentConfig.CreateDocumentDB();
                CurrentConfig.CreateDocumentTable();
            }
            catch (SqlException ex) 
            {
                Debug.WriteLine("sql异常：" + ex.Message);
            }
            SaveConfig();
            return CurrentConfig;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        protected static bool SaveConfig()
        {
            try
            {
                SerializeHelper.SerializerXml<Config>(CurrentConfig, ConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        protected static bool LoadConfig()
        {
            try
            {
                CurrentConfig = SerializeHelper.DeserializeXml<Config>(ConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;

        }
        /// <summary>
        /// 创建文档数据库
        /// </summary>
        void CreateDocumentDB()
        {
            ServerDB serverDB = new ServerDB(DocumentsDBIp, DocumentsDBName);
            serverDB.CreateDB();
        }
        /// <summary>
        /// 创建文档表
        /// </summary>
        void CreateDocumentTable()
        {
            DocumentDB serverDB = new DocumentDB(DocumentsDBIp, DocumentsDBName);
            serverDB.CreateDocumentTable();
        }

    }
}
