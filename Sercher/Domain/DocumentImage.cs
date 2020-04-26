using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    [TableInfo("documentImages")]
    public class DocumentImage
    {
        [Key]
        [Identity]
        public int _id { get; set; }
        public int DocId { get; set; }
        public int PicId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Dpi { get; set; }
        public int Source { get; set; }
        public long Hash { get; set; }
    }

}
