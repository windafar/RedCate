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
    [TableInfo("IndexesTemplateTable")]
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

        [Key]
        public int DocId { set; get; }
        public int DocumentWordTotal { set; get; }
        /// <summary>
        /// 对应一个句子
        /// </summary>
        public long IndexTime { set; get; }

       // public string Word { set; get; }
        /// <summary>
        /// 当前单词出现在文中的次数
        /// </summary>
        public int WordFrequency { set; get; }
        /// <summary>
        /// 索引词语在文中的位置
        /// </summary>
        [FiledInfo("text")]
        public string BeginIndex { set; get; }

        public int Permission { set; get; }

    }

}
