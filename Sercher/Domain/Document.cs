using System.ComponentModel.DataAnnotations;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    [TableInfo("documents")]
    public class Document
    {
        [FiledInfo("nchar",false,200)]
        public string Name { set; get; }
        [FiledInfo("text", false)]
        public string Url { set; get; }
        [Key]
        [Identity]
        public int _id { set; get; }
        //public long WordTotal { set; get; }
        public enum HasIndexed { none, Indexed, Indexing }

        [EnumDataType(typeof(HasIndexed))]//这EnumDataType估计有问题
        public int hasIndexed { set; get; }

        public int Permission { set; get; }
    }

}
