using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sercher
{
    public interface ISercherIndexesDB : IServerDB
    {
        /// <summary>
        /// 表总计数目
        /// </summary>
        int TableCount { get; set; }
        /// <summary>
        /// 创建索引表
        /// </summary>
        /// <param name="names"></param>
        void CreateIndexTable(string[] names);
        /// <summary>
        /// 创建索引模板表
        /// </summary>
        void CreateIndexTemplateTable();
        /// <summary>
        /// 重索引文档
        /// </summary>
        /// <param name="document"></param>
        void ReIndexesDocument(Document document);
        /// <summary>
        /// 获取索引表数目
        /// </summary>
        /// <returns></returns>
        int GetSercherIndexCollectionCount();
        /// <summary>
        /// 获取索引表名集合
        /// </summary>
        /// <returns></returns>
        List<string> GetSercherIndexCollectionNameList();
        /// <summary>
        /// 获取sql连接
        /// </summary>
        /// <returns></returns>
        SqlConnection GetSercherIndexDb();
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="word">搜索的单词</param>
        /// <param name="doctotal">文档总数</param>
        /// <param name="resultAction">对每条结果的返回处理</param>
        /// <param name="pagesize">页面大小</param>
        /// <param name="pagenum">页数</param>
        /// <param name="permission">权限值，大于该值的才会返回，默认为最大值10</param>
        void GetSercherResultFromIndexesDB(string word, int doctotal, Action<int, double> resultAction, int pagesize = 10, int pagenum = 1, int permission = Int32.MaxValue);
        /// <summary>
        /// 上传多个文档索引
        /// </summary>
        /// <param name="word"></param>
        /// <param name="documentIndex"></param>
        void UploadDocumentIndex(string word, DocumentIndex[] documentIndex);
        /// <summary>
        /// 使用sql语句直接更新索引
        /// </summary>
        /// <param name="sqls"></param>
        void UploadDocumentIndex(string[] sqls);
        /// <summary>
        /// 迁移数据表
        /// </summary>
        /// <param name="maxdb">最大负载的目标DB</param>
        /// <param name="tableNamelist">需要迁移的表</param>
        /// <returns></returns>
        object ImmigrationOperation(ISercherIndexesDB maxdb, List<string> tableName);
    }
}