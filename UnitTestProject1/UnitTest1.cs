
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeripheralTool;
using Sercher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            LinkedList<String> nodes = new LinkedList<string>();
            nodes.AddLast("192.168.2.1:8080");
            nodes.AddLast("192.168.2.2:8080");
            nodes.AddLast("192.168.2.3:8080");
            nodes.AddLast("192.168.2.4:8080");
            //ConsistentHashLoadBalance consistentHash = new ConsistentHashLoadBalance(nodes, 160);
            //nsistentHash.printTreeNode();

        }
        [TestMethod]
        public void TesGethash()
        {
            ConsistentHashLoadBalance consistentHash = new ConsistentHashLoadBalance();
           // consistentHash.GetHashByWorld("我都");
        }

        public void mongodbtest()
        {
            
        }
        [TestMethod]
        public void testjiebaSegmenter()
        {
            var text = Peripheral.BeforeEncodingClass.GetText(File.ReadAllBytes("Www.Chnxp.Com.Cn 黑魔导.txt"));
           var u= PeripheralTool.Peripheral.Segmenter(text);
        }
        [TestMethod]
        public void TestIndex()
        {
            foreach (var path in Directory.GetFiles(@"C:\Users\yjdcb\Desktop\新建文件夹", "*.txt", SearchOption.AllDirectories))
                (new SercherIndexesDB { DbName = "mydb", Ip = "WIN-T9ASCBISP3P\\MYSQL" }).UploadDocument(new Document() { hasIndexed = (int)Document.HasIndexed.none, Name = new FileInfo(path).Name, Url = path });

            var se = new SercherServerBase();
            
           // se.BuildSercherIndexToMongoDB();

            


        }
        [TestMethod]
        public void TestSearcher()
        {
           new SercherServerBase().Searcher("三");
        }
    
    }
}
