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

        string connectionStr = "mongodb://{0}:27017";
        string documentDataDatabaseName = "DocumentDataDatabase", documentDataCollectionName = "DocumentDataCollection";
        string worldDatabaseName = "WorldDatabase", worldCollectionName = "WorldCollection";
        int indexContextLimt = 100;

        public string GetConnectionStr(string ip) { return string.Format(connectionStr, ip); }
        public string DocumentDataDatabaseName { get => documentDataDatabaseName; set => documentDataDatabaseName = value; }
        public string DocumentDataCollectionName { get => documentDataCollectionName; set => documentDataCollectionName = value; }
        public string WorldDatabaseName { get => worldDatabaseName; set => worldDatabaseName = value; }
        public string WorldCollectionName { get => worldCollectionName; set => worldCollectionName = value; }
        public int IndexContextLimt { get => indexContextLimt; set => indexContextLimt = value; }
    }
}
