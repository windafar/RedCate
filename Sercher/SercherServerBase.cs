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
using NGenerics.DataStructures.Trees;
using System.Threading;

namespace Sercher
{
    public enum StartMode
    {
        /// <summary>
        /// 只允许基于单词（集合）的分布式模式，相对更快的搜索，更快的启动
        /// </summary>
        Quick,
        /// <summary>
        /// 完全模式，允许任意切割的分布式
        /// </summary>
        Perfect
    }

    public class Progress
    {
        public int totalFileNum { set; get; }
        public int curFileIndex { set; get; }
    }

    public class RelationDocumentResult
    {
        public ObjectId documentIdex { set; get; }
        public double dependency { set; get; }
        public Document doc { set; get; }
    }

    class IndexCach
    {
        public string world;
        public IEnumerable<DocumentIndex> indeies;

        public IndexCach(string world, IEnumerable<DocumentIndex> indeies)
        {
            this.world = world;
            this.indeies = indeies;
        }
    }

    public class SercherServerBase
    {
        object lockobj = new object();
        StartMode startMode;

        ConsistentHashLoadBalance hashLoadBalance = new ConsistentHashLoadBalance();

        public SercherServerBase(StartMode startMode= StartMode.Quick)
        {
            this.startMode = startMode;
        }

        public void BuildSercherIndexToMongoDB( Progress progress = null)
        {
            hashLoadBalance.RemoveAllDBData();
            long UpdateCount = 0;

            hashLoadBalance.SetServerDBCount();
            RedBlackTree<string, List<DocumentIndex>> documentIndices_cachList = new RedBlackTree<string, List<DocumentIndex>>();
            List<Document> documents_cachList = new List<Document>();
            var DocumentToatalList = Helper.GetNotIndexDocument();
            int remainder = DocumentToatalList.Count;
            DocumentToatalList.ForEach(x =>
            {
                x.UpdateDocumentStateToIndexed(Document.HasIndexed.Indexing);
                System.Diagnostics.Stopwatch watch = new Stopwatch();
                watch.Start();
                var file = TextHelper.BeforeEncodingClass.GetText(File.ReadAllBytes(x.Url));
                var textSplit = TextHelper.Segmenter(file);
                Dictionary<string, DocumentIndex> documentIndices = new Dictionary<string, DocumentIndex>();
                foreach (var token in textSplit)
                {
                    string world = token.Word;
                        if (world[0] < 0x4E00 || world[0] > 0x9FFF)//非中文
                            if (world[0] < 0x41 || world[0] > 0x5a)//非大小字母
                                if (world[0] < 0x61 || world[0] > 0x7a)//非小写字母
                                    continue;
                    //记录一个文档的所有相同词汇
                    if (documentIndices.TryGetValue(world, out DocumentIndex documentIndex))
                    {
                        documentIndex.WordFrequency++;
                        documentIndex.BeginIndex.Add(token.StartIndex);
                    }
                    else
                        documentIndices[world] = new DocumentIndex
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
                    //UpdateIndex(kvp.Key, kvp.Value);
                    if (documentIndices_cachList.ContainsKey(kvp.Key.ToString()))
                        documentIndices_cachList[kvp.Key].Add(new DocumentIndex(kvp.Value));
                    else
                        documentIndices_cachList.Add(kvp.Key, new List<DocumentIndex>() { kvp.Value });
                    UpdateCount++;
                    //Console.Write(kvp.Key);

                }
                documents_cachList.Add(x);
                remainder--;
                //均衡检查，放在文档的循环中，保证一篇文档的索引在一个数据库中
                double temp_i = 0.0;
                if (UpdateCount > Config.config.IndexCachSum || remainder == 0)
                {
                    Console.WriteLine("当前准备上传数目："+UpdateCount);
                    foreach (var keyValue in documentIndices_cachList)
                    {
                        temp_i++;
                        string str = "当前上传：" + temp_i / documentIndices_cachList.Count * 100.0 + "%" + ",剩余：" + remainder;
                        backspace(str.Length+8);
                        Console.Write(str);
                        //Thread.Sleep(50);
                        UpdateIndexToServer(keyValue.Key, keyValue.Value.ToArray());
                    }
                    temp_i = 0;
                    documentIndices_cachList.Clear();

                    watch.Stop();
                    documents_cachList.ForEach(xx =>
                    {
                        xx.UpdateDocumentStateToIndexed(Document.HasIndexed.Indexed);

                        var mSeconds = watch.ElapsedMilliseconds;
                        Console.WriteLine("文档" + xx.Name + ",,大小：" + new FileInfo(xx.Url).Length / 1024 + "kb\r\n" + "消耗时间：" + mSeconds / 1000);
                        Debug.Print("文档" + xx.Name + ",,大小：" + new FileInfo(xx.Url).Length / 1024 + "kb\r\n" + "消耗时间：" + mSeconds / 1000);
                    });
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
        void UpdateIndexToServer(string world, DocumentIndex[] documentIndices)
        {
                hashLoadBalance.FindCloseServerDBsByWorld(world)
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
                var db = hashLoadBalance.FindCloseServerDBsByWorld(world);
                db.GetSercherResult(world, docTotal, (x, y) =>
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
        static void backspace(int n)
        {
            for (var i = 0; i < n; ++i)
                Console.Write((char)0x8);
        }
    }

}
///有两个方案，对词再一次分布式 以及 不再分布式词
///前者可以有效解决索引运行时照成mongdb锁的影响，并且可以分布储存数据
///后者可以加速查询，且操作简单，直接在数据库中完成
