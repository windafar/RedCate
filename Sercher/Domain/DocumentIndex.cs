using System;
using System.ComponentModel.DataAnnotations;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    /// <summary>
    /// 文档索引数据
    /// </summary>
    /// <remarks>一个单词对应一个文档，一次文档索引产生多个documentindex</remarks>
    [Serializable]
    [TableInfo("IndexesTemplateTable")]//模板表表名
    public class DocumentIndex
    {
        public DocumentIndex()
        {
        }

        public DocumentIndex(DocumentIndex documentIndex)
        {
            DocId = documentIndex.DocId;
            DocumentWordTotal = documentIndex.DocumentWordTotal;
            IndexTime = documentIndex.IndexTime;
            WordFrequency = documentIndex.WordFrequency;
            BeginIndex = documentIndex.BeginIndex;
        }

        /// <summary>
        /// 索引文档的id
        /// </summary>
        [Key]
        public int DocId { set; get; }
        /// <summary>
        /// 索引文档的的单词总数
        /// </summary>
        public int DocumentWordTotal { set; get; }
        /// <summary>
        /// 创建索引的时间
        /// </summary>
        public long IndexTime { set; get; }

        /// <summary>
        /// 当前单词出现在此文中的次数
        /// </summary>
        public int WordFrequency { set; get; }
        /// <summary>
        /// 索引词语在文中的位置
        /// </summary>
        [FiledInfo("text")]
        public string BeginIndex { set; get; }
        /// <summary>
        /// 创建索引时规定的权限，默认为文档的权限
        /// </summary>
        public int Permission { set; get; }

    }

}
