using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PeripheralTool;
using System.Linq;
using System.Threading.Tasks;
//using MongoDB.Driver;//test
using System.Diagnostics;

namespace Sercher
{
    public class SercherServerBase
    {
        object lockobj = new object(); 
        public class RelationDocumentResult
        {
            public ObjectId documentIdex { set; get; }
            public double dependency { set; get; }
            public Document doc { set; get; }
        }

        private class docwf
        {
           public int WordFrequency;
            public int docworldtotal;
        }

        ConsistentHashLoadBalance hashLoadBalance = new ConsistentHashLoadBalance();

        public void BuildSercherIndexToMongoDB(Progress progress = null)
        {
            //hashLoadBalance.RemoveDBData();
            long UpdateCount = 0;
            hashLoadBalance.SetServerDBCount();
            Helper.GetNotIndexDocument().ForEach(x =>
            {
                x.UpdateDocumentStateToIndexed(Document.HasIndexed.Indexing);
                System.Diagnostics.Stopwatch watch = new Stopwatch();
                watch.Start();
                var file = TextHelper.BeforeEncodingClass.GetText(File.ReadAllBytes(x.Url));
                var textSplit = TextHelper.Segmenter(file);
                Dictionary<string, DocumentIndex> documentIndices = new Dictionary<string, DocumentIndex>();
                foreach (var token in textSplit)
                {
                    string text = token.Word;
                        if (text[0] < 0x4E00 || text[0] > 0x9FFF)//非中文
                            if (text[0] < 0x41 || text[0] > 0x5a)//非大小字母
                                if (text[0] < 0x61 || text[0] > 0x7a)//非小写字母
                                    continue;

                    if (documentIndices.TryGetValue(text, out DocumentIndex documentIndex))
                    {
                        documentIndex.WordFrequency++;
                        documentIndex.BeginIndex.Add(token.StartIndex);
                    }
                    else
                        documentIndices[text] = new DocumentIndex
                        {
                            IndexTime = DateTime.Now,
                            DocId = x._id,
                            //RelevantContent= file.Substring((int)(Math.Max(token.StartIndex-Config.config.IndexContextLimt/2,0)),100),
                            //Word = text,
                            WordFrequency = 1,
                            BeginIndex = new List<int>() { token.StartIndex }
                        };
                }
                foreach (var kvp in documentIndices)
                {
                    kvp.Value.DocumentWorldTotal = documentIndices.Count;
                    UpdateIndex(kvp.Key, kvp.Value);
                    UpdateCount++;
                    Console.Write(kvp.Key);
                }
                x.UpdateDocumentStateToIndexed(Document.HasIndexed.Indexed);
                watch.Stop();
                var mSeconds = watch.ElapsedMilliseconds;
                Console.WriteLine("文档" + x.Name + ",,大小：" + new FileInfo(x.Url).Length / 1024 + "kb\r\n" + "消耗时间：" + mSeconds / 1000);
                Debug.Print("文档" + x.Name + ",,大小：" + new FileInfo(x.Url).Length / 1024 + "kb\r\n" + "消耗时间：" + mSeconds / 1000);
                //均衡检查，放在文档的循环中，保证一篇文档的索引在一个数据库中
                if (UpdateCount > 2000)
                {
                    UpdateCount = 0;
                    hashLoadBalance.SetServerDBCount();
                }

            });

        }
        /// <summary>
        /// 通过单词映射，从索引服务器中选择负载最小的，更新索引
        /// </summary>
        /// <param name="world"></param>
        /// <param name="documentIndex"></param>
        void UpdateIndex(string world, DocumentIndex documentIndices)
        {
                hashLoadBalance.FindCloseServerDBsByWorld(world)
                .OrderBy(x=>x.WorldCount).First()
                .UploadDocumentIndex(world, documentIndices);
        }

        public void Searcher(string text)
        {
            //1.分词，排序搜索结果
            var wordList = TextHelper.Segmenter(text).Select(x => x.Word).ToArray();
            int docTotal = (int)Helper.GetDocumentNum();
            //List<RelationDocumentResult> list = new List<RelationDocumentResult>();
            List<RelationDocumentResult> relationDocumentResults = new List<RelationDocumentResult>();

            Parallel.ForEach(wordList, (world) =>
            {
                var dbList = hashLoadBalance.FindCloseServerDBsByWorld(world);
                int dbNum = dbList.Count();
                dbList.First().GetSercherResult(world, docTotal, (x, y) =>
                  {
                      lock (lockobj)
                      {
                          relationDocumentResults.Add(new RelationDocumentResult { dependency = y, documentIdex = x });
                      }
                  });
            });
           //计算每篇文章和搜索的单个单词相关性
           //2.然后，统计整体相关性
            var result = relationDocumentResults
                .GroupBy(x => x.documentIdex)
                 .Select(doc => new RelationDocumentResult
                 {
                     dependency = doc.Sum(r => r.dependency),//计算单个文档的相关性总和
                     documentIdex = doc.First().documentIdex,
                     doc = Helper.GetDocumentById(doc.First().documentIdex)
                 }).OrderByDescending(docresult => docresult.dependency)//最后根据相关性总和排序
                 .ToArray();
        }

        public class Progress
        {
            public int totalFileNum { set; get; }
            public int curFileIndex { set; get; }
        }
    }

}
///有两个方案，对词再一次分布式 以及 不再分布式词
///前者可以有效解决索引运行时照成mongdb锁的影响，并且可以分布储存数据
///后者可以加速查询，且操作简单，直接在数据库中完成
