using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
//using MongoDB.Driver;//test
using System.Diagnostics;
using NGenerics.DataStructures.Trees;
using System.Threading;
using Component;
using Component.Default;
using static Sercher.DomainAttributeEx;

namespace Sercher
{
    public class RelationDocumentResult
    {
        public int documentId { set; get; }
        public double dependency { set; get; }
        public Document doc { set; get; }
    }

    public class SercherServerBase
    {
        object lockobj = new object();

        static protected IConsistentHashLoadBalance<ISercherIndexesDB> hashLoadBalance;

        DocumentDB documentDB;

        public SercherServerBase(bool IsInit=false)
        {
            documentDB = new DocumentDB(dbName: Config.CurrentConfig.DocumentsDBName, ip: Config.CurrentConfig.DocumentsDBIp);


            if (hashLoadBalance == null || IsInit)
                hashLoadBalance = new ConsistentHashLoadBalance<ISercherIndexesDB>(
                    Config.CurrentConfig.IndexesServerlists.Cast<ISercherIndexesDB>().ToList());

        }

        public void BuildSercherIndexToSQLDB()
        {
            //hashLoadBalance.RemoveAllDBData();
            //hashLoadBalance = new ConsistentHashLoadBalance();
            long UpdateCount = 0;
            SetServerDBCount();
            RedBlackTree<string, string> documentIndices_cachList = new RedBlackTree<string, string>();
            var DocumentToatalList = documentDB.GetNotIndexDocument();
            int remainder = DocumentToatalList.Count;
            var remotewords= SercherIndexesDB.GetWords(hashLoadBalance.GetServerNodes());
            var localwords = new HashSet<string>();
            Dictionary<string, TextComponent> textComponent = new Dictionary<string, TextComponent>();//使用到的时候进行缓存
            int curWordCachNum = 0;
            for(int i=0,j=0;i< DocumentToatalList.Count;i++)
            {
                var doc = DocumentToatalList[i];
                documentDB.UpdateDocumentStateIndexStatus(doc._id, Document.HasIndexed.Indexing);
                System.Diagnostics.Stopwatch watch = new Stopwatch();
                watch.Start();
                IEnumerable<SegmenterToken> textSplit = Pretreatment(doc);
                Dictionary<string, DocumentIndex> documentIndices = new Dictionary<string, DocumentIndex>();
                int wordTotal = textSplit.Count();

                foreach (var token in textSplit)
                {
                    string word = token.Word.Trim().ToLower();
                    if (!remotewords.Contains(word))
                        if (!localwords.Contains(word))
                            localwords.Add(word);
                    //记录一个文档的所有相同词汇
                    if (documentIndices.TryGetValue(word, out DocumentIndex documentIndex))
                    {
                        documentIndex.WordFrequency++;
                        documentIndex.BeginIndex += ',' + token.StartIndex.ToString();
                        documentIndex.DocumentWordTotal = wordTotal;
                    }
                    else
                        documentIndices[word] = new DocumentIndex
                        {
                            IndexTime = DateTime.Now.Ticks,
                            DocId = doc._id,
                            // RelevantContent= file.Substring((int)(Math.Max(token.StartIndex-Config.config.IndexContextLimt/2,0)),100),
                            //Word = text,
                            WordFrequency = 1,
                            BeginIndex = token.StartIndex.ToString(),
                            DocumentWordTotal = wordTotal,
                            Permission = doc.Permission == 0 ? Config.CurrentConfig.DefaultPermission : doc.Permission
                        };
                }

                //转换为脚本并加入全局缓存等待上传
                foreach (var kvp in documentIndices)
                {
                    //UpdateIndex(kvp.Key, kvp.Value);
                    if (documentIndices_cachList.ContainsKey(kvp.Key.ToString()))
                    {
                        documentIndices_cachList[kvp.Key] += "," + InsetValueIntoMemory(kvp.Key, new DocumentIndex[1] { kvp.Value }, false);
                    }
                    else
                    {
                        documentIndices_cachList.Add(kvp.Key, InsetValueIntoMemory(kvp.Key, new DocumentIndex[1] { kvp.Value }, true));
                    }
                    UpdateCount++;
                    //Console.Write(kvp.Key);
                }


                remainder--;
                watch.Stop();
                Console.WriteLine("完成缓存文档：" + doc.Name + ",速度（/s）：" + documentIndices.Count / watch.Elapsed.TotalSeconds);
                curWordCachNum += documentIndices.Count;
                documentIndices.Clear();

                if (Config.CurrentConfig.MaxIndexCachWordNum < curWordCachNum)
                {
                    //对每一个同数据库的词汇的脚本进行组合,创建表
                    foreach (var g in localwords.GroupBy(w => hashLoadBalance.FindCloseServerDBsByTableName(w).DbName))
                    {
                        var wordgroup = g.ToArray();
                        hashLoadBalance.GetServerNodes().First(n => n.DbName == g.Key)//!##GroupKey欠妥，不过数据库比较少的时候影响不大
                        .CreateIndexTable(wordgroup);
                        //foreach (var w in wordgroup)
                        //{
                        //    localwords.Remove(w);
                        //}
                    }
                    localwords.Clear();
                    //对每一个同数据库的词汇的脚本进行组合，上传
                    foreach (var g in documentIndices_cachList.AsQueryable().GroupBy(kv => hashLoadBalance.FindCloseServerDBsByTableName(kv.Key).DbName))
                    {
                        //上传此db的inser脚本
                        hashLoadBalance.FindCloseServerDBsByTableName(g.First().Key)
                        .UploadDocumentIndex(g.Select(s => s.Value + ";").ToArray());
                    }

                    documentIndices_cachList.Clear();
                    while(j<=i)
                    {            
                        documentDB.UpdateDocumentStateIndexStatus(DocumentToatalList[j]._id, Document.HasIndexed.Indexed);
                        j++;
                    }

                    curWordCachNum = 0;

                }

            }




        }
        /// <summary>
        /// 对输入文件进行预处理并分词
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IEnumerable<SegmenterToken> Pretreatment(Document x)
        {
            string filetype = x.Url.Substring(x.Url.LastIndexOf(".")).Remove(0,1);
            var segmenterResult = TextComponent.GetInstance(
                   filetype,
                   new FileStream(x.Url, FileMode.Open)
                    ).ToSegmenterResult();
            IEnumerable<SegmenterToken> textSplit = segmenterResult.Where(t =>
            {
                string word = t.Word;
                if ((word[0] < 0x4E00 || word[0] > 0x9FFF)//非中文
                   && (word[0] < 0x41 || word[0] > 0x5a)//非大小字母
                        && (word[0] < 0x61 || word[0] > 0x7a))//非小写字母
                    return false;
                return true;

            });
            return textSplit;
        }

        /// <summary>
        /// 以表为单位生成SQL脚本
        /// </summary>
        /// <param name="tableName"></param>
        protected string InsetValueIntoMemory<T>(string tableName, T[] objList,bool inserHead=true)
        {
            string DbName = hashLoadBalance.FindCloseServerDBsByTableName(tableName).DbName;
            var Pros = typeof(T).GetProperties();
            var ProsNamelist = Pros.Where(x => IdentityAttribute.GetAttribute(x) == null).Select(x => x.Name); ;//排除私有键属性名
            string Sqlpramslist = "(" + string.Join(",", ProsNamelist) + ")";
            StringBuilder stringBuilder = new StringBuilder();
            if (inserHead)
                stringBuilder.Append("INSERT INTO [" + DbName + "].[dbo].[" + tableName + "]" + Sqlpramslist + " VALUES ");
            foreach (var drow in objList)
            {
                stringBuilder.Append("(");
                foreach (var dcol in Pros)
                {
                    if (IdentityAttribute.GetAttribute(dcol) != null) continue;//排除私有键属性值
                    var value = dcol.GetValue(drow).ToString();
                    stringBuilder.Append("'" + value + "',");
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.Append("),");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        public RelationDocumentResult[] Searcher(string text)
        {
            //1.分词，排序搜索结果
            var wordList = TextComponent.SegmentFor(text);
            int docTotal = (int)documentDB.GetDocumentNum();
            List<RelationDocumentResult> relationDocumentResults = new List<RelationDocumentResult>();

            wordList.ToList().ForEach((word) =>
            {
                var db = hashLoadBalance.FindCloseServerDBsByTableName(word);
                db.GetSercherResultFromIndexesDB(word, docTotal, (x, y) =>
                  {
                      lock (lockobj)
                      {
                          relationDocumentResults.Add(new RelationDocumentResult { dependency = y, documentId = x });
                      }
                  });
            });
           //计算每篇文章和搜索的单个单词相关性
           //2.然后，统计整体相关性
            var result = relationDocumentResults
                .GroupBy(x => x.documentId)
                 .Select(doc => new RelationDocumentResult
                 {
                     dependency = doc.Sum(r => r.dependency),//计算单个文档的相关性总和
                     documentId = doc.First().documentId,
                     doc = documentDB.GetDocumentById(doc.First().documentId)
                 }).OrderByDescending(docresult => docresult.dependency)//最后根据相关性总和排序
                 .ToArray();
            return result;
        }
        public void RemoveAllDBData()
        {
            hashLoadBalance.GetServerNodes().ToList().ForEach(x => x.DeleDb());

        }
        /// <summary>
        /// 统计各个数据库的集合数
        /// </summary>
        void SetServerDBCount()
        {
            hashLoadBalance.GetServerNodes().ToList()
                .ForEach(x => x.IndexesTableCount = x.GetSercherIndexCollectionCount());
        }

        public void AddIndexesNode(ISercherIndexesDB newSercherIndexesDB)
        {
            SetServerDBCount();
            hashLoadBalance.AddHashMap(newSercherIndexesDB,
                    db => db.Value.IndexesTableCount,
                    maxdb => maxdb.GetSercherIndexCollectionNameList(),
                    (maxdb, tableNamelist) => newSercherIndexesDB.ImmigrationOperation(maxdb, tableNamelist)); 
        }

        public IEnumerable<KeyValuePair<long, ISercherIndexesDB>> GetHashNodes()
        {
            return hashLoadBalance.GetHashNodes();
        }
    }

}
