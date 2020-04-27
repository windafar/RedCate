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
    public class RelationDocumentResult
    {
        public int documentId { set; get; }
        public double dependency { set; get; }
        public Document doc { set; get; }
    }

    public class SercherServerBase
    {
        object lockobj = new object();

        static IConsistentHashLoadBalance<ISercherIndexesDB> hashLoadBalance;

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
            //hashLoadBalance.SetServerDBCount();
            RedBlackTree<string, string> documentIndices_cachList = new RedBlackTree<string, string>();
            var DocumentToatalList = documentDB.GetNotIndexDocument();
            int remainder = DocumentToatalList.Count;
            var remotewords= SercherIndexesDB.GetWords(hashLoadBalance.GetServerNodes());
            var localwords = new HashSet<string>();
            DocumentToatalList.ForEach(x =>
             {
                documentDB.UpdateDocumentStateIndexStatus(x._id,Document.HasIndexed.Indexing);
                System.Diagnostics.Stopwatch watch = new Stopwatch();
                 watch.Start();
                 var file = TextHelper.BeforeEncodingClass.GetText(File.ReadAllBytes(x.Url));
                 var textSplit = Peripheral.Segmenter(file).Where(t =>
                 {
                     string word = t.Word;
                     if ((word[0] < 0x4E00 || word[0] > 0x9FFF)//非中文
                        && (word[0] < 0x41 || word[0] > 0x5a)//非大小字母
                             && (word[0] < 0x61 || word[0] > 0x7a))//非小写字母
                        return false;
                     return true;

                 });
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
                             DocId = x._id,
                             // RelevantContent= file.Substring((int)(Math.Max(token.StartIndex-Config.config.IndexContextLimt/2,0)),100),
                             //Word = text,
                             WordFrequency = 1,
                             BeginIndex = token.StartIndex.ToString(),
                             DocumentWordTotal = wordTotal,
                             Permission = x.Permission==0?Config.CurrentConfig.DefaultPermission: x.Permission
                         };
                 }

                 //转换为脚本并加入全局缓存等待上传
                 foreach (var kvp in documentIndices)
                 {
                  //UpdateIndex(kvp.Key, kvp.Value);
                     if (documentIndices_cachList.ContainsKey(kvp.Key.ToString()))
                     {
                         documentIndices_cachList[kvp.Key] += "," + InsetValueIntoMemory(kvp.Key, new DocumentIndex[1] { kvp.Value },false);
                     }
                     else
                     {
                         documentIndices_cachList.Add(kvp.Key, InsetValueIntoMemory(kvp.Key, new DocumentIndex[1] { kvp.Value }, true));
                     }
                     UpdateCount++;
                    //Console.Write(kvp.Key);
                }


                 remainder--;
                //均衡检查，放在文档的循环中，保证一篇文档的索引在一个数据库中
                watch.Stop();
                 Console.WriteLine("完成缓存文档：" + x.Name + ",速度（/s）：" + documentIndices.Count / watch.Elapsed.TotalSeconds);
                 documentIndices.Clear();
            });

            //对每一个同数据库的词汇的脚本进行组合,创建表
            foreach (var g in localwords.GroupBy(w => hashLoadBalance.FindCloseServerDBsByValue(w).DbName))
            {
                var wordgroup = g.ToArray();
                hashLoadBalance.GetServerNodes().First(x=>x.DbName==g.Key)//!##GroupKey欠妥，不过数据库比较少的时候影响不大
                .CreateIndexTable(wordgroup);
                //foreach (var w in wordgroup)
                //{
                //    localwords.Remove(w);
                //}
            }
            localwords.Clear();
            //对每一个同数据库的词汇的脚本进行组合，上传
            foreach (var g in documentIndices_cachList.AsQueryable().GroupBy(kv => hashLoadBalance.FindCloseServerDBsByValue(kv.Key).DbName))
            {
                //上传此db的inser脚本
                hashLoadBalance.FindCloseServerDBsByValue(g.First().Key)
                .UploadDocumentIndex(g.Select(x => x.Value + ";").ToArray());
            }

            DocumentToatalList.ForEach(x =>
            {
                documentDB.UpdateDocumentStateIndexStatus(x._id, Document.HasIndexed.Indexed);
            });

            documentIndices_cachList.Clear();

        }

        /// <summary>
        /// 以表为单位生成SQL脚本
        /// </summary>
        /// <param name="tableName"></param>
        protected string InsetValueIntoMemory<T>(string tableName, T[] objList,bool inserHead=true)
        {
            string DbName = hashLoadBalance.FindCloseServerDBsByValue(tableName).DbName;
            var Pros = typeof(T).GetProperties();
            var ProsNamelist = Pros.Select(x => x.Name).Where(x => x[0] != '_');//排除私有键属性名
            string Sqlpramslist = "(" + string.Join(",", ProsNamelist) + ")";
            StringBuilder stringBuilder = new StringBuilder();
            if (inserHead)
                stringBuilder.Append("INSERT INTO [" + DbName + "].[dbo].[" + tableName + "]" + Sqlpramslist + " VALUES ");
            foreach (var drow in objList)
            {
                stringBuilder.Append("(");
                foreach (var dcol in Pros)
                {
                    if (dcol.Name[0] == '_') continue;//排除私有键属性值
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
            var wordList = Peripheral.Segmenter(text).Select(x => x.Word).ToArray();
            int docTotal = (int)documentDB.GetDocumentNum();
            List<RelationDocumentResult> relationDocumentResults = new List<RelationDocumentResult>();

            wordList.ToList().ForEach((word) =>
            {
                var db = hashLoadBalance.FindCloseServerDBsByValue(word);
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
                .ForEach(x => x.TableCount = x.GetSercherIndexCollectionCount());
        }

        public void AddIndexesNode(ISercherIndexesDB sercherIndexesDB)
        {
            SetServerDBCount();
            hashLoadBalance.AddHashMap(sercherIndexesDB,
                    db => db.Value.TableCount,
                    maxdb => maxdb.GetSercherIndexCollectionNameList(),
                    (maxdb, tableNamelist) => sercherIndexesDB.ImmigrationOperation(maxdb, tableNamelist)); 
        }
    }

}
