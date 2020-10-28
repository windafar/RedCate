using Component;
using NGenerics.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
        static public string ConfigFilePath = "Config.xml";
        private string documentsDBName = "mydb";
        private string documentsDBIp = "(local)";
        List<SercherIndexesDB> indexesServerlists;
        private int defaultPermission = 0;
        private string defaultDbDirPath= @"F:\indexdir";
        int maxIndexCachWordNum = 100000;
        string indexesServiceName;
        string defaultDbUserName="sa";
        string defaultDbPwd="yjdcb";
        private int maxIndexWordStartLocation=3;
        private int uploadThreadNum=5;

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
        /// <summary>
        /// 指定唯一索引机器名
        /// </summary>
        public string IndexesServiceName { get => indexesServiceName; }
        public string DefaultDbUserName { get => defaultDbUserName; set => defaultDbUserName = value; }
        public string DefaultDbPwd { get => defaultDbPwd; set => defaultDbPwd = value; }
        //索引记录文字位置数
        public int MaxIndexWordStartLocation { get => maxIndexWordStartLocation; set => maxIndexWordStartLocation = value; }
        /// <summary>
        /// 上传索引的线程数
        /// </summary>
        public int UploadThreadNum { get => uploadThreadNum; set => uploadThreadNum = value; }

        public Config() 
        {
            try
            {
                var moc = new ManagementClass("Win32_Processor").GetInstances();;
                foreach (ManagementObject mo in moc)
                    indexesServiceName = mo.Properties["ProcessorId"].Value.ToString();
                moc.Dispose();
            }
            catch
            {
                indexesServiceName= "unknown";
            }

        }
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
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseA")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseB")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseC")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseD")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseE")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseF")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseG")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseH")
                    );
            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseI")
                    );

            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseJ")
                    );

            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseK")
                    );

            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseL")
                    );

            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseM")
                    );

            CurrentConfig.IndexesServerlists.Add(
                     new SercherIndexesDB("(local)", "SercherIndexDatabaseN")
                    );

            //test
            //CurrentConfig.IndexesServerlists.ForEach(x => x.DeleDb());

            try
            {
            //为不存在的索引数据库创建，并初始化模板表
            CurrentConfig.IndexesServerlists.ForEach(x =>
            {
                if (!x.GetdbStatus())
                {
                    x.CreateDB();
                    x.CreateIndexTemplateTable();
                }
            });

                CurrentConfig.CreateDocumentDB();
                CurrentConfig.CreateDocumentTable();
            }
            catch (SqlException ex) 
            {
                GlobalMsg.globalMsgHand.Invoke("sql异常：" + ex.Message);
            }
            SaveConfig();
            return CurrentConfig;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        public static bool SaveConfig()
        {
            try
            {
                SerializeHelper.SerializerXml<Config>(CurrentConfig, ConfigFilePath);
            }
            catch (Exception ex)
            {
                GlobalMsg.globalMsgHand.Invoke(ex.Message);
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
                GlobalMsg.globalMsgHand.Invoke(ex.Message);
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

        public void AddIndexesServerlists(ISercherIndexesDB sercherIndexesDB) 
        {
            
            IndexesServerlists.Add((SercherIndexesDB)sercherIndexesDB);
        }

    }
}
