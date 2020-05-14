
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sercher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Component;
using Component.Default;

namespace developTest_Component
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
          var rslut=  new DocxFile(new FileStream(@"C:\Users\yjdcb\Desktop\事业单位 扩展 3.docx",FileMode.Open)).ToSegmenterResult();

        }
        [TestMethod]
        public void TestMethod2()
        {
        //    var rslut = new PptxFile(new FileStream(@"C:\Users\yjdcb\Desktop\新建 Microsoft PowerPoint 演示文稿.pptx", FileMode.Open)).ToSegmenterResult();

        }
        [TestMethod]
        public void TestMethod3()
        {
            
        }
    }
}
