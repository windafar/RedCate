
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sercher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace developTest_core
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
           // ConsistentHashLoadBalance consistentHash = new ConsistentHashLoadBalance();
           // consistentHash.GetHashByWorld("我都");
        }

        public void mongodbtest()
        {
            
        }
        [TestMethod]
        public void testjiebaSegmenter()
        {
          //  var text = Peripheral.BeforeEncodingClass.GetText(File.ReadAllBytes("Www.Chnxp.Com.Cn 黑魔导.txt"));
        //   var u= PeripheralTool.Peripheral.Segmenter(text);
        }
        [TestMethod]
        public void TestIndex()
        {
            Config.Init(true);
            foreach (var path in Directory.GetFiles(@"D:\资料\yuliao\穿越小说2011-5-3\穿越小说2011-5-3", "*.*", SearchOption.AllDirectories)
                .Where(x => x.LastIndexOf(".txt") != -1
                || x.LastIndexOf(".doc") != -1
                || x.LastIndexOf(".xls") != -1
                || x.LastIndexOf(".xhtml") != -1
                ))
                (new DocumentDB { DbName = "mydb", Ip = "WIN-T9ASCBISP3P\\MYSQL" }).UploadDocument(new Document() { hasIndexed = "no", Name = new FileInfo(path).Name, Url = path });

            var se = new SercherServerBase();          
            se.BuildSercherIndexToSQLDB();
        }
        [TestMethod]
        public void TestPretreatment()
        {
            //SercherServerBase.Pretreatment(new Document { Name = "text", Url = @"C:\Users\yjdcb\Desktop\行测笔记.docx" });
           var r= SercherServerBase.Pretreatment(new Document { Name = "text", Url = @"C:\Users\yjdcb\OneDrive\Documents\heart of woods.xlsx" });

        }

        [TestMethod]
        public void TestSearcher()
        {
        Config.Init();
          var list= new SercherServerBase().Searcher("头上");
            
        }
        [TestMethod]
        public void TestInit()
        {
           Config.Init();
        }
    
    }
}
