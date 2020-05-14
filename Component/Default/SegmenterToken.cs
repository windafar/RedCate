using System;
using System.Collections.Generic;
using System.Text;

namespace Component.Default
{
   public class SegmenterToken
    {
        public SegmenterToken(string word, int startIndex)
        {
            Word = word;
            StartIndex = startIndex;
        }

        public string Word { get; set; }
        public int StartIndex { get; set; }

        public override string ToString() { return Word; }
    }

}
