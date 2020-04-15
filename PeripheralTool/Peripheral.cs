using JiebaNet.Segmenter;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeripheralTool
{
    public class Peripheral
    {
        static public IEnumerable<Token> Segmenter(string str)
        {
            JiebaSegmenter jiebaSegmenter = new JiebaSegmenter();
            return jiebaSegmenter.Tokenize(str, TokenizerMode.Search);
            //return jiebaSegmenter.CutForSearch(BeforeEncodingClass.GetText(text));//细粒度切分
        }
    }
}
