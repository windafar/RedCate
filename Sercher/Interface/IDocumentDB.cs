using System.Collections.Generic;

namespace Sercher
{
    public interface IDocumentDB : IServerDB
    {
        void DelDocumentById(int documentId);

        /// <summary>
        /// 获取文档通过文档ID
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        Document GetDocumentById(int docid);
        /// <summary>
        /// 获取文档总数
        /// </summary>
        /// <returns></returns>
        int GetIndexedDocumentNum();
        /// <summary>
        /// 获取未被索引的文档
        /// </summary>
        /// <returns></returns>
        List<Document> GetNotIndexDocument();
        void ResetDocumentIndexStatus();
        void ResetDocumentIndexStatus(int docId);
        void UpdateDocumentStateIndexStatus(int docId, string hasIndexed);

        /// <summary>
        ///  更新文档
        /// </summary>
        /// <param name="doc"></param>
        void UploadDocument(Document doc);

    }
}