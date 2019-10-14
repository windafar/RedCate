using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using JiebaNet.Segmenter;
using MongoDB.Bson;
using static Sercher.SercherServerBase;
using Newtonsoft.Json;

namespace Sercher
{
   static public class Helper
    {
        static IMongoCollection<Document> documentCollction = new MongoClient(Config.config.GetConnectionStr("localhost"))
                 .GetDatabase(Config.config.DocumentDataDatabaseName)
                 .GetCollection<Document>(Config.config.DocumentDataCollectionName);
        static Dictionary<string, IMongoDatabase> MongoDatabaseConnected=new Dictionary<string, IMongoDatabase>();

        static public IMongoDatabase GetSercherIndexDb(this ServerDB serverDB)
        {
            IMongoDatabase _database;
            if (MongoDatabaseConnected.TryGetValue(serverDB.Ip + serverDB.DbName, out var value))
            {
                _database = value;
            }
            else
            {
                string connectionStr = Config.config.GetConnectionStr(serverDB.Ip);
                //MongoClient _client = new MongoClient(new MongoClientSettings { MinConnectionPoolSize = 120, WriteConcern = new WriteConcern(0), MaxConnectionLifeTime = TimeSpan.FromSeconds(1800), Server = new MongoServerAddress("localhost") });
                MongoClient _client = new MongoClient(connectionStr);

                _database = _client.GetDatabase(serverDB.DbName);
                MongoDatabaseConnected.Add(serverDB.Ip + serverDB.DbName, _database);
            }
             return _database;
        }
        static public int GetSercherIndexCollectionCount(string ip,string databaseName)
        {
            string connectionStr = Config.config.GetConnectionStr(ip);
            MongoClient _client = new MongoClient(connectionStr);
            IMongoDatabase _database = _client.GetDatabase(databaseName);
            int Collectionum = _database.ListCollectionNames().ToList().Count;
            return Collectionum;
        }
        static public List<string> GetSercherIndexCollectionNameList(this ServerDB serverDB)
        {
            IMongoDatabase _database = GetSercherIndexDb(serverDB);
            return _database.ListCollectionNames().ToList();
        }

        static public void CopyCollection(this ServerDB sourceDB,string collectionName, ServerDB destDb)
        {
            string connectionStrSource = Config.config.GetConnectionStr(sourceDB.Ip);
            MongoClient _client = new MongoClient(connectionStrSource);
            IMongoDatabase _database = _client.GetDatabase(sourceDB.DbName);
            string connectionStrdst = Config.config.GetConnectionStr(destDb.Ip);

            var collection = _database.GetCollection<DocumentIndex>(collectionName);
            var findcur = collection.Find(new BsonDocument("_id", new BsonDocument("$exists", true)));
            int curNum = 0, total = (int)findcur.CountDocuments();

            var dest = new MongoClient(connectionStrdst).GetDatabase(destDb.DbName)
                            .GetCollection<DocumentIndex>(collectionName);

            while (curNum < total) {
               var docs= findcur.Skip(curNum).Limit(1000).ToEnumerable();
                dest.InsertMany(docs);
                curNum += 1000;
            }


        }

        static public void DelCollectionAsync(this ServerDB serverDB, string collectionName)
        {
            IMongoDatabase _database = GetSercherIndexDb(serverDB);
            _database.DropCollectionAsync(collectionName);
        }

        /// <summary>
        /// 更新文档索引标记为已索引
        /// </summary>
        /// <param name="document"></param>
        static public void UpdateDocumentStateToIndexed(this Document document,Document.HasIndexed hasIndexed)
        {
            var filter = Builders<Document>.Filter.Eq(x => x._id, document._id);
            var updated1 = Builders<Document>.Update.Set(x => x.hasIndexed, hasIndexed);
            var resultx = documentCollction.UpdateOne(filter, updated1);
        }
        static public void UploadDocument(Document doc,string ip)
        {
            //var filter = Builders<Document>.Filter.Eq(x => x._id, doc._id);
            //var updated1 = Builders<Document>.Update
            //    .Set(x => x.hasIndexed, doc.hasIndexed)
            //    .Set(x => x.Name, doc.Name)
            //    .Set(x => x.Url, doc.Url);
            //    documentCollction.UpdateOne(filter, updated1,new UpdateOptions() { IsUpsert=true});
            documentCollction.InsertOne(doc);

        }
        static public void UpdateDocument(Document doc, string ip)
        {
            var filter = Builders<Document>.Filter.Eq(x => x._id, doc._id);
            var updated1 = Builders<Document>.Update
                .Set(x => x.hasIndexed, doc.hasIndexed)
                .Set(x => x.Name, doc.Name)
                .Set(x => x.Url, doc.Url);
            documentCollction.UpdateOne(filter, updated1, new UpdateOptions() { IsUpsert = false });
            //documentCollction.InsertOne(doc);
        }
        static public void DelDocumentById(ObjectId docid, string ip)
        {
            var filter = Builders<Document>.Filter.Eq(x => x._id, docid);
            documentCollction.DeleteOne(filter);
        }

        static public void DelIndexAndDocument(this ServerDB serverDB,string CollectionName, ObjectId docId)
        {
            serverDB.GetSercherIndexDb().GetCollection<DocumentIndex>(CollectionName)
                .DeleteOne(x => x.DocId == docId);
                ;
            documentCollction.DeleteOne(x => x._id == docId);
        }

        static public void UploadDocumentIndex(string world,DocumentIndex[] documentIndex, string ip,string databaseName)
        {
            IMongoDatabase _database = GetSercherIndexDb(new ServerDB { DbName = databaseName, Ip = ip, WorldCount = 0 });
            //string connectionStr = Config.config.GetConnectionStr(ip);
            //MongoClient _client = new MongoClient(connectionStr);
            //IMongoDatabase _database = _client.GetDatabase(databaseName);
            // _database.CreateCollection(documentIndices[0].Word);
            var collection = _database.GetCollection<DocumentIndex>(world);
            //var filter = Builders<DocumentIndex>.Filter.Eq(x => x._id, documentIndex._id);
            //var updated1 = Builders<DocumentIndex>.Update
            //    .Set(x => x.BeginIndex,documentIndex.BeginIndex)
            //    .Set(x=>x.DocId,documentIndex.DocId)
            //    .Set(x => x.IndexTime, documentIndex.IndexTime)
            //    .Set(x => x.Word, documentIndex.Word)
            //    .Set(x => x.WordFrequency, documentIndex.WordFrequency)
            //    ;
            //collection.FindOneAndUpdate<DocumentIndex>(filter, updated1,new FindOneAndUpdateOptions<DocumentIndex, DocumentIndex>() { IsUpsert=true});
            collection.InsertMany(documentIndex, new InsertManyOptions { BypassDocumentValidation = false, IsOrdered = false }) ;
        }
        static public void UploadDocumentIndex(this ServerDB serverDB,string world, DocumentIndex[] documentIndex)
        {
            UploadDocumentIndex(world,documentIndex, serverDB.Ip,serverDB.DbName);
        }

        static public List<Document> GetNotIndexDocument()
        {
           return documentCollction.Find(x => x.hasIndexed!= Document.HasIndexed.Indexed).ToList();
        }

        static public List<DocumentIndex> GetDocumentIndex(this ServerDB serverDB,string collectionName,int start,int length, out long total)
        {
            IMongoDatabase _database = GetSercherIndexDb(serverDB);
            var collection = _database.GetCollection<DocumentIndex>(collectionName);
            var f = collection.Find(new BsonDocument("_id", new BsonDocument("$exists", true)));
            total = f.CountDocuments();
            return f.Skip(start).Limit(length).ToList();
        }

        static public List<DocumentIndex> GetDocumentIndexId(this ServerDB serverDB, string collectionName)
        {
            IMongoDatabase _database = GetSercherIndexDb(serverDB);
            var collection = _database.GetCollection<DocumentIndex>(collectionName);
            var f = collection.Find(new BsonDocument("_id", new BsonDocument("$exists", true))); ;
            //.Project(x=>new worldToDocid {Docid=x.DocId,Worldid=x._id });
            return f.ToList();
        }
        static public Document GetDocumentById(ObjectId docid)
        {
           return documentCollction.Find(x => x._id == docid).First();
        }
        static public long GetDocumentNum()
        {
            return documentCollction.CountDocuments(x => x._id != null);
        }
        static public long GetCollectionDocumentNum(this ServerDB serverDB, string collectionName)
        {
            IMongoDatabase _database = GetSercherIndexDb(serverDB);
            var collection = _database.GetCollection<DocumentIndex>(collectionName);
            return collection.CountDocuments(x=>x._id!=null);
        }

        static public void DeleDb(this ServerDB serverDB)
        {
            string connectionStrSource = Config.config.GetConnectionStr(serverDB.Ip);
            MongoClient _client = new MongoClient(connectionStrSource);
            _client.DropDatabase(serverDB.DbName);
            MongoDatabaseConnected.Remove(serverDB.Ip + serverDB.DbName);
        }

        /// <summary>
        /// 计算某个单词的相关性
        /// </summary>
        /// <param name="ArticleTotal">文档总数</param>
        /// <param name="ArctNumInCurWord">当前单词命中文档数</param>
        /// <param name="CurArctWordNum">当前文档单词总数</param>
        /// <param name="CurArctHitNum">当前文档命中单词数</param>
        /// <returns></returns>
        /// <remarks>参考TF-IDE公式，2017年4月23日15:00:33</remarks>
        static public double TF_IDE_CountFunc(int ArticleTotal, int ArctNumInCurWord, int CurArctWordNum, int CurArctHitNum)
        {
            double TF = CurArctHitNum / (double)CurArctWordNum;

            double IDE = Math.Log(ArticleTotal / ArctNumInCurWord, 2);
            return TF * IDE;
        }
        /// <summary>
        /// TF-IDE算法在数据库的实现
        /// </summary>
        /// <param name="serverDB"></param>
        /// <param name="world"></param>
        /// <param name="doctotal"></param>
        /// <param name="action"></param>
        public static void GetSercherResult(this ServerDB serverDB, string world,int doctotal,Action<ObjectId,double> action)
        {
            string func =string.Format(@"(function(h,d){0}var g=db.getCollection(h);var m=g.find({0}{1});var b=g.count();var i=0;var n=[];for(var e=0;e<b;e++){0}var l=m[e].DocumentWorldTotal;var c=m[e].WordFrequency;var k=c/l;var a=Math.log(d/b,2);i=k*a;var f={0}{1};f.id=m[e].DocId;f.tf_ide=i;n.push(f){1}return n{1})('{2}',{3});", "{","}", world, doctotal);
            //string func = string.Format(@"(function(world,doctotal)
            //                {0}
            //                    var collection = db.getCollection(world);
            //                    var docindexs= collection.find({0}{1});
    
            //                    var worldHitDoc=collection.count();
            //                    var tf_ide=0;
            //                    var result=[];
            //                    for(var j=0; j<worldHitDoc; j++)
            //                    {0}
            //                        var docWorldTotal= docindexs[j].DocumentWorldTotal
            //                        var docHitWorld = docindexs[j].WordFrequency
            //                        var TF = docHitWorld/docWorldTotal;
            //                        var IDE = Math.log(doctotal/worldHitDoc,2);
            //                        tf_ide = TF*IDE;
            //                        var obj={0}{1};
            //                        obj.id=docindexs[j].DocId;
            //                        obj.tf_ide=tf_ide
            //                        result.push(obj);
            //                    {1}
            //                    return result;
            //                    {1})('{2}',{3});", "{", "}", world, doctotal);

            string connectionStrSource = Config.config.GetConnectionStr(serverDB.Ip);
            MongoClient _client = new MongoClient(connectionStrSource);
            var ser= _client.GetServer();
            var s = (ser.GetDatabase(serverDB.DbName)).Eval(func, world, doctotal);
            var js = s.AsBsonArray.GetEnumerator();
            while (js.MoveNext())
            {
                var document = js.Current.AsBsonDocument;
                var id = (ObjectId)document.GetValue("id");
                var tf_ide = (double)document.GetValue("tf_ide");
                action(id, tf_ide);
            } 
        }

    }
}
