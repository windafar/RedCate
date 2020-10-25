using System.ComponentModel.DataAnnotations;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    [TableInfo("documents")]
    public class Document
    {
        /// <summary>
        /// 文档名
        /// </summary>
        [FiledInfo("nchar",false,200)]
        public string Name { set; get; }
        /// <summary>
        /// 文档位置
        /// </summary>
        [FiledInfo("text", false)]
        public string Url { set; get; }
        /// <summary>
        /// 文档id
        /// </summary>
        [Key]
        [Identity]
        public int _id { set; get; }
        /// <summary>
        /// yes,no,pro{IndexServiceName}
        /// </summary>
        [FiledInfo("nchar", false, 1024)]
        public string hasIndexed { set; get; }
        /// <summary>
        /// 文档权限，默认为0，范围在int.Min~int.Max
        /// </summary>
        public int Permission { set; get; }
    }

}
