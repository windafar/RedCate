using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sercher
{
    public interface ISercherIndexesDB : IServerDB
    {
        int TableCount { get; set; }
        void CreateIndexTable(string[] names);
        void CreateIndexTemplateTable();
        void ReIndexesDocument(Document document);
        int GetSercherIndexCollectionCount();
        List<string> GetSercherIndexCollectionNameList();
        SqlConnection GetSercherIndexDb();
        void GetSercherResultFromIndexesDB(string word, int doctotal, Action<int, double> resultAction, int pagesize = 10, int pagenum = 1, int permission = Int32.MaxValue);
        void UploadDocumentIndex(string word, DocumentIndex[] documentIndex);
        void UploadDocumentIndex(string[] sqls);
        object ImmigrationOperation(ISercherIndexesDB maxdb, List<string> tableName);
    }
}