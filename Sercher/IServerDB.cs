using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sercher
{
    public interface IServerDB
    {
        string DbName { get; set; }
        string Ip { get; set; }
        int Status { get; set; }

        bool CreateSqlDB();
        void DeleDb();
        bool GetdbStatus();
    }

    public interface ISercherIndexesDB : IServerDB
    {
        int TableCount { get; set; }
        void CreateIndexTable(string[] names);
        List<DocumentIndex> GetDocumentIndex(string collectionName, int start, int length, out long total);
        List<DocumentIndex> GetDocumentIndexId(string collectionName);
        int GetSercherIndexCollectionCount();
        List<string> GetSercherIndexCollectionNameList();
        SqlConnection GetSercherIndexDb();
        void GetSercherResultFromSQLDB(string world, int doctotal, Action<int, double> action, int pagesize = 10, int pagenum = 1);
        void UploadDocumentIndex(string world, DocumentIndex[] documentIndex);
        void UploadDocumentIndex(string[] sqls);
        object ImmigrationOperation(ISercherIndexesDB maxdb, List<string> tableName);
    }
    public interface IDocumentDB : IServerDB
    {
        Document GetDocumentById(int docid);
        int GetDocumentNum();
        List<Document> GetNotIndexDocument();
        HashSet<string> GetWords();
        void UpdateDocument(Document doc, string ip);
        void UploadDocument(Document doc);
        void UploadWord(string[] words);

    }
}