using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Component
{
    class DocxFile : TextComponent
    {
        public DocxFile(string inputData) : base(inputData)
        {
        }

        public DocxFile(FileStream inputFs) : base(inputFs)
        {
        }
        protected override TextComponent Process()
        {

            return base.Process();
        }
    }
}
