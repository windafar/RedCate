using System;
using System.Collections.Generic;
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

        public string DocumentsDBName { get => documentsDBName; set => documentsDBName = value; }
        public string DocumentsDBIp { get => documentsDBIp; set => documentsDBIp = value; }
        public List<SercherIndexesDB> IndexesServerlists { get => indexesServerlists; set => indexesServerlists = value; }
        /// <summary>
        /// the permisson will be write in during indexes and document create
        /// and be used to filter search result
        /// default value is zero, range from int.min to int.max
        /// </summary>
        public int DefaultPermission { get => defaultPermission; set => defaultPermission = value; }
        public Config() { }
        static public Config Init(bool reload = false)
        {
            if (!reload && CurrentConfig != null) return CurrentConfig;
            if (Config.LoadConfig()) return CurrentConfig;
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
            CurrentConfig.IndexesServerlists.ForEach(x => x.DeleDb());


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
            SaveConfig();
            return CurrentConfig;
        }

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

        void CreateDocumentDB()
        {
            ServerDB serverDB = new ServerDB(DocumentsDBIp, DocumentsDBName);
            serverDB.CreateDB();
        }

        void CreateDocumentTable()
        {
            DocumentDB serverDB = new DocumentDB(DocumentsDBIp, DocumentsDBName);
            serverDB.CreateDocumentTable();
        }

        void InitComponentList()
        {
            var ComponentList = new List<KeyValuePair<string, string[]>>();
            ComponentList.Add(new KeyValuePair<string, string[]>("docx", new string[] 
            { "FileComponent.TextConvert", "TextComponent.TraditionalConvert" }));
            ComponentList.Add(new KeyValuePair<string, string[]>("docx", new string[] 
            { "FileComponent.ImageConvert" }));

        }

    }
}
