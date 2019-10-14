using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercher
{
   public class Config
    {
        static public Config config = new Config();

        string documentDataDatabaseName = "DocumentDataDatabase", documentDataCollectionName = "DocumentDataCollection";
        string worldDatabaseName = "WorldDatabase", worldCollectionName = "WorldCollection";
        int indexContextLimt = 100;
        int indexCachSum = 100000;
        public string GetConnectionStr(string ip)
        {
            if (ip.IndexOf(":") == -1)
                return string.Format("mongodb://{0}:{1}", ip, "27017");
            else
                return string.Format("mongodb://{0}", ip);
        }
        public string DocumentDataDatabaseName { get => documentDataDatabaseName; set => documentDataDatabaseName = value; }
        public string DocumentDataCollectionName { get => documentDataCollectionName; set => documentDataCollectionName = value; }
        public string WorldDatabaseName { get => worldDatabaseName; set => worldDatabaseName = value; }
        public string WorldCollectionName { get => worldCollectionName; set => worldCollectionName = value; }
        public int IndexContextLimt { get => indexContextLimt; set => indexContextLimt = value; }
        /// <summary>
        /// the cach size during the index be upload 
        /// </summary>
        public int IndexCachSum { get => indexCachSum; set => indexCachSum = value; }
    }
}
