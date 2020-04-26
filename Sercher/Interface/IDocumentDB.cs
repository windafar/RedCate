using System.Collections.Generic;

namespace Sercher
{
    public interface IDocumentDB : IServerDB
    {
        Document GetDocumentById(int docid);
        int GetDocumentNum();
        List<Document> GetNotIndexDocument();
        void UpdateDocument(Document doc, string ip);
        void UploadDocument(Document doc);

    }
}