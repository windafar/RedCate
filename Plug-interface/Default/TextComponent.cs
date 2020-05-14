using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JiebaNet.Segmenter;

/// <summary>
/// 组件暂缓...
/// </summary>
namespace Component.Default
{
    /// <summary>
    /// 文本相关的搜索和索引外围主键
    /// </summary>
    public abstract class TextComponent
    {
        protected readonly Stream inputFs;
        JiebaSegmenter jiebaSegmenter = new JiebaSegmenter();

        public TextComponent(Stream inputFs)
        {
            if (!inputFs.CanRead)
            {
                throw new ArgumentException();
            }

            this.inputFs = inputFs;
        }
        /// <summary>
        /// 将输出为可用于分词的字符串
        /// </summary>
        abstract protected string ConvertInputToString();
        /// <summary>
        /// 指定分词方法
        /// </summary>
        /// <returns></returns>
        virtual protected IEnumerable<SegmenterToken> Segmenter(string segmenterString)
        {
            return jiebaSegmenter.Tokenize(segmenterString, TokenizerMode.Search)
                .Select(x => new SegmenterToken(x.Word, x.StartIndex));
        }
        /// <summary>
        /// 串联各个行为出结果
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SegmenterToken> ToSegmenterResult()
        {
            string segmenterString = ConvertInputToString();
            return Segmenter(segmenterString);
        }

        static public IEnumerable<string> SegmentFor(string segmenterString)
        {
            JiebaSegmenter jiebaSegmenter = new JiebaSegmenter();
            return jiebaSegmenter.Tokenize(segmenterString, TokenizerMode.Search)
                .Select(x => x.Word);

        }

        public static TextComponent GetInstance(string FileType, FileStream CotParam)
        {

            return (TextComponent)Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.BaseType.Name == "TextComponent")
                .Where(x => ComponentPramAttribute.GetAttribute(x).FileType.IndexOf(FileType) != -1)
                .FirstOrDefault()//对于没有指定的直接抛出异常不合理
                .GetConstructor(new Type[] { typeof(FileStream) })
                .Invoke(new FileStream[] {CotParam });
        }

    }

}
