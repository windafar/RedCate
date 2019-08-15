using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using Sercher;

namespace mongodb
{

    public class Program
    {
        static void Main(string[] args)
        {
            var se = new SercherServerBase();

            se.BuildSercherIndexToMongoDB();
        }

        static void testskip()
        {
            string connectionStr = "mongodb://localhost:27017";
            MongoClient _client = new MongoClient(connectionStr);
            MongoDatabaseBase _database = _client.GetDatabase("FileServer") as MongoDatabaseBase;
            var cc = _database.GetCollection<BsonDocument>("SercherIndex");
            cc.AsQueryable().ToList().ForEach(x => Console.WriteLine(x.ToJson()));
            Console.WriteLine("\r\n");
            Console.WriteLine("\r\n");
            Console.WriteLine("\r\n");
            var findFluent = cc.Find(new BsonDocument("_id", new BsonDocument("$exists", true)));
            Console.WriteLine("\r\nlimit(1):\r\n" + findFluent.Limit(1).ToList().ToJson());
            findFluent.Skip(1);//=========>result: it's will move just in first!!
            Console.WriteLine("\r\nSkip(1) then limit(1):\r\n" + findFluent.Limit(1).ToList().ToJson());
            Console.WriteLine("\r\n ones anagin then limit(1):\r\n" + findFluent.Limit(1).ToList().ToJson());

            //test skip if not move cur=========>result:NOT
            findFluent.Skip(1);
            findFluent.Skip(1);
            Console.WriteLine("\r\nSkip(1)*2 then limit(1):\r\n" + findFluent.Limit(1).ToList().ToJson());

            //test skip if move cur=========>result:NOT
            findFluent.Skip(1);
            findFluent = findFluent.Skip(1);
            Console.WriteLine("\r\ncoutiue Skip(1) and limit\r\n" + findFluent.Limit(1).ToList().ToJson());

            //test Cursor next 
            //var cur = findFluent.ToCursor();
            var cur = cc.Find(new BsonDocument("_id", new BsonDocument("$exists", true))).Skip(1).Limit(1).ToCursor();
            do
            {
                Console.WriteLine("\r\nCursor\r\n" + cur.MoveNext().ToJson() + ":" + cur.Current.ToJson());
            }
            while (cur.MoveNext());

            //test ToEnumerable
            var ff = cc.Find(new BsonDocument("_id", new BsonDocument("$exists", true))).Skip(1).Limit(1);
            Console.WriteLine("\r\nToEnumerable\r\n" + ff.ToEnumerable().ToList().ToJson());
            Console.WriteLine("\r\nToEnumerable\r\n" + ff.ToEnumerable().ToList().ToJson());


            Console.ReadKey();
            ///remark:IFindFluent.skip在命令中的确只有一次调用机会。。。
            ///eg:IFindFluent.skip(1).skip(1)在命令中是只执行最后一个，游标只固定最后一个
            ///并且调用完skip的游标会进行迭代，且会继续使用相同的limit配置
            ///这点在使用IFindFluent.ToEnumerable使倒是挺合理的，针对选择进行列表化
        }

        static public void testUpdate()
        {
            string connectionStr = "mongodb://localhost:27017";
            MongoClient _client = new MongoClient(connectionStr);
            MongoDatabaseBase _database = _client.GetDatabase("FileServer") as MongoDatabaseBase;

            var document = new BsonDocument
            {
            { "address" , new BsonDocument
                {
                    { "street", "2 Avenue" },
                    { "zipcode", "10075" },
                    { "building", "1480" },
                    { "coord", new BsonArray { 73.9557413, 40.7720266 } }
                }
            },
            { "borough", "Manhattan" },
            { "cuisine", "Italian" },
            { "grades", new BsonArray
                {
                    new BsonDocument
                    {
                        { "date", new DateTime(2014, 10, 1, 0, 0, 0, DateTimeKind.Utc) },
                        { "grade", "A" },
                        { "score", 11 }
                    },
                    new BsonDocument
                    {
                        { "date", new DateTime(2014, 1, 6, 0, 0, 0, DateTimeKind.Utc) },
                        { "grade", "B" },
                        { "score", 17 }
                    }
                }
            },
            { "name", "Vella2" },
            { "restaurant_id", "41704620" }
        };

            var collection = _database.GetCollection<BsonDocument>("SercherIndex") as MongoCollectionBase<BsonDocument>;
            collection.InsertOne(document);//API：插入文档到集合
            //BsonDocument bbs = new BsonDocument { { "name", "Vella" } };


            var dd = collection.AsQueryable().First(x => x["name"] == "Vella");
            ///////////////////////////更新-BsonDocument///////////////////////
            /////使用MongoDB.Driver.BsonDocumentUpdateDefinition<TDocument>方式/////
            var result = collection.UpdateMany(x => x["name"] == "Vella", new BsonDocument {
                    { "$set", new BsonDocument { { "borough", "745454545454545" } } } });
            //Collection.UpdateOne(x => x.Name == CurVaildvalue.Name,
            //    new BsonDocument {
            //    { "docs",new BsonArray(CurVaildvalue.docs)},
            //    { "Frequency",CurVaildvalue.Frequency},
            //    { "WordSource",CurVaildvalue.WordSource}
            //});//此种方法更新只适合简单类型，new BsonArray(CurVaildvalue.docs)是失败的。

            ///////////////更新-使用构建器/////////////////////////////////
            var filter = Builders<BsonDocument>.Filter.Lt("counter", 10);
            var updated1 = Builders<BsonDocument>.Update.Inc("counter", 100);
            var resultx = collection.UpdateManyAsync(filter, updated1).Result;

            //var updated2 = Builders<SercherContext>.Update.Push<SercherContext>("", new SercherContext());

        }

        static public void UploadIndexToMongodb()
        {
            string connectionStr = "mongodb://localhost:27017";
            string databaseName = "FileServer";
            string IndexDocumentCollectionName = "indexDocument";
            MongoClient _client = new MongoClient(connectionStr);
            IMongoDatabase _database = _client.GetDatabase(databaseName);

            IMongoCollection<filewithid> IndexDocumentCollection = _database.GetCollection<filewithid>(IndexDocumentCollectionName);



            var filelist = Directory.GetFiles(@"E:\资料", "*.*", SearchOption.AllDirectories)
                  .Where(x => x.EndsWith("txt") || x.EndsWith("html") || x.EndsWith("xhtml"))
                  .ToArray();
            var f = filelist.Select(x => new filewithid { filepath = x }).ToList();
            IndexDocumentCollection.InsertMany(f);
        }

        static public void testcommand()
        {
            string connectionStr = "mongodb://localhost:27017";
            MongoClient _client = new MongoClient(connectionStr);
            MongoDatabaseBase _database = _client.GetDatabase("music") as MongoDatabaseBase;
            IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>("namespaces");
            var s = _database.ListCollections().ToList().Count;
            //var value= collection.Find(new BsonDocument("", "")).First();
            //int num = _database.RunCommand(new JsonCommand<int>("{db:'stats'}"));
        }
        class filewithid
        {
            public string filepath { set; get; }
            public ObjectId _id { set; get; }
        }

    }
}
///关于mongodb
///坑是有点多的，对于运用到搜索引擎中需要注意的，总结下
///1，注意全库写锁：索引时，写入时，文档中Array字段频繁更新
///2，保证文档细粒度
///3，索引问题：针对Array类型的字段索引好像没啥效果，尽量少用
///