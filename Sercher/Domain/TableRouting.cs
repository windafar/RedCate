using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sercher.Domain
{
    class TableRouting
    {
        private string physicaSuffixList = DateTime.Now.Year.ToString();
        private char suffixSplit = ';';
        private string virtualTableName;

        public TableRouting() { }
        public string VirtualTableName { get => virtualTableName; set => virtualTableName = value; }
        public string PhysicaSuffixList { get => physicaSuffixList; set => physicaSuffixList = value; }
        IEnumerable<string> PhysicalTableNames { get => physicaSuffixList.Split(suffixSplit).Select(x => virtualTableName + x); }

    }
}
