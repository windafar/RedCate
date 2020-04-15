using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercher
{
    /// <summary>
    /// 文档索引数据
    /// </summary>
    /// <remarks>一个单词对应一个文档，一次文档索引产生多个documentindex</remarks>
    [Serializable]
    public class DocumentIndex
    {
        public DocumentIndex()
        {
        }

        public DocumentIndex(DocumentIndex documentIndex)
        {

            DocId = documentIndex.DocId;
            DocumentWorldTotal = documentIndex.DocumentWorldTotal;
            IndexTime = documentIndex.IndexTime;
            _id = documentIndex._id;
            WordFrequency = documentIndex.WordFrequency;
            BeginIndex = documentIndex.BeginIndex;
        }


        public int DocId { set; get; }
        public int DocumentWorldTotal { set; get; }
        /// <summary>
        /// 对应一个句子
        /// </summary>
        public long IndexTime { set; get; }
        [Key]
        public int _id { set; get; }

       // public string Word { set; get; }
        /// <summary>
        /// 当前单词出现在文中的次数
        /// </summary>
        public int WordFrequency { set; get; }
        /// <summary>
        /// 索引词语在文中的位置
        /// </summary>
        public string BeginIndex { set; get; }
    }


    public class Document
    {
        public string Name { set; get; }
        public string Url { set; get; }
        [Key]
        public int _id { set; get; }
        //public long WorldTotal { set; get; }
        public enum HasIndexed { none, Indexed, Indexing }

        [EnumDataType(typeof(HasIndexed))]//这EnumDataType估计有问题
        public int hasIndexed { set; get; }
    }

}
