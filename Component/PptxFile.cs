using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Linq;
using D = DocumentFormat.OpenXml.Drawing;
using System.IO.Packaging;
using Component.Default;

namespace Component
{
    [ComponentPram(FileType: "pptx,ppt")]
    public class PptxFile : TextComponent, IDisposable
    {
        public PptxFile(Stream inputFs) : base(inputFs)
        {
            
        }
        protected override string ConvertInputToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (var presentationDocument = DocumentFormat.OpenXml.Packaging.PresentationDocument.Open("测试.pptx", false))
            {
                var presentationPart = presentationDocument.PresentationPart;
                var presentation = presentationPart.Presentation;

                // 先获取页面
                var slideIdList = presentation.SlideIdList;


                foreach (var slideId in slideIdList.ChildElements.OfType<SlideId>())
                {
                    // 获取页面内容
                    SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);

                    var slide = slidePart.Slide;

                    foreach (var paragraph in
                        slidePart.Slide
                            .Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                    {
                        // 获取段落
                        // 在 PPT 文本是放在形状里面
                        foreach (var text in
                            paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                        {
                            // 获取段落文本，这样不会添加文本格式
                            stringBuilder.AppendLine(text.Text);
                        }
                    }
                }
            }

            Dispose();
            return stringBuilder.ToString();

        }

        #region 备用
        public static void GetSlideTitles(string presentationFile, string store)
        {
            // Open the presentation as read-only.
            using (PresentationDocument presentationDocument =
                PresentationDocument.Open(presentationFile, false))
            {
                GetSlideTitles(presentationDocument, store);
            }
        }

        public static void GetSlideTitles(PresentationDocument presentationDocument, string store)
        {
            if (presentationDocument == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            // Get a PresentationPart object from the PresentationDocument object.
            PresentationPart presentationPart = presentationDocument.PresentationPart;

            if (presentationPart != null &&
                presentationPart.Presentation != null)
            {
                // Get a Presentation object from the PresentationPart object.
                Presentation presentation = presentationPart.Presentation;

                if (presentation.SlideIdList != null)
                {


                    // Get the title of each slide in the slide order.
                    foreach (var slideId in presentation.SlideIdList.Elements<SlideId>())
                    {
                        SlidePart slidePart = presentationPart.GetPartById(slideId.RelationshipId) as SlidePart;

                        // Get the slide title.
                        GetSlide(slidePart, store);

                        // An empty title can also be added.

                    }

                }

            }

        }

        // Get the title string of the slide.
        public static void GetSlide(SlidePart slidePart, string store)
        {
            if (slidePart == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            // Declare a paragraph separator.
            string titleSeparator = null;

            if (slidePart.Slide != null)
            {
                // Find all the title shapes.
                var shapes = from shape in slidePart.Slide.Descendants<Shape>()
                             where IsTitleShape(shape)
                             select shape;

                StringBuilder titleText = new StringBuilder();

                foreach (var shape in shapes)
                {
                    // Get the text in each paragraph in this shape.
                    foreach (var paragraph in shape.TextBody.Descendants<D.Paragraph>())
                    {
                        // Add a line break.
                        titleText.Append(titleSeparator);

                        foreach (var text in paragraph.Descendants<D.Text>())
                        {
                            titleText.Append(text.Text);
                        }

                        titleSeparator = "\n";
                    }
                }
                if (titleText.Length == 0)
                    return;
                LinkedList<string> texts = new LinkedList<string>();

                foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    StringBuilder allText = new StringBuilder();
                    foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                    {
                        allText.Append(text.Text);
                    }
                    if (allText.Length > 0)
                    {
                        if (allText.ToString() == titleText.ToString()) ;
                        else texts.AddLast(allText.ToString());
                    }
                }
                if (texts.Count > 0)
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(store, true);

                    file.Write("{\"Title\":\"" + titleText.ToString() + "\",");
                    file.Write("\"Content\":\"");
                    string inter = "";
                    foreach (var text in texts)
                    {
                        file.Write(inter + text);
                        inter = ",";
                    }
                    file.WriteLine("\"}");
                    file.Close();
                }

            }

            return;
        }

        // Determines whether the shape is a title shape.
        private static bool IsTitleShape(Shape shape)
        {
            var placeholderShape = shape.NonVisualShapeProperties.ApplicationNonVisualDrawingProperties.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                switch ((PlaceholderValues)placeholderShape.Type)
                {
                    // Any title shape.
                    case PlaceholderValues.Title:

                    // A centered title.
                    case PlaceholderValues.CenteredTitle:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        private static void FixPowerpoint(string fileName)
        {
            //Opening the package associated with file
            using (Package wdPackage = Package.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                //Uri of the printer settings part
                var binPartUri = new Uri("/ppt/printerSettings/printerSettings1.bin", UriKind.Relative);
                if (wdPackage.PartExists(binPartUri))
                {
                    //Uri of the presentation part which contains the relationship
                    var presPartUri = new Uri("/ppt/presentation.xml", UriKind.RelativeOrAbsolute);
                    var presPart = wdPackage.GetPart(presPartUri);
                    //Getting the relationship from the URI
                    var presentationPartRels =
                        presPart.GetRelationships().Where(a => a.RelationshipType.Equals("http://schemas.openxmlformats.org/officeDocument/2006/relationships/printerSettings",
                            StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                    if (presentationPartRels != null)
                    {
                        //Delete the relationship
                        presPart.DeleteRelationship(presentationPartRels.Id);
                    }

                    //Delete the part
                    wdPackage.DeletePart(binPartUri);
                }
                wdPackage.Close();
            }
        }

        public IEnumerable<SegmenterToken> ToResult()
        {
            string pptlistPath = "【马赛克】";
            System.IO.StreamReader pptlist = new System.IO.StreamReader(pptlistPath);
            string ppt;
            while ((ppt = pptlist.ReadLine()) != null)
            {
                Console.WriteLine(ppt);
                string pptname = "【马赛克】" + ppt + ".pptx";
                string storepath = "【马赛克】" + ppt + ".jl";

                System.IO.StreamWriter file = new System.IO.StreamWriter(storepath, false);
                file.Close();
                FixPowerpoint(pptname);
                GetSlideTitles(pptname, storepath);

            }
            pptlist.Close();


            return new List<SegmenterToken>();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.inputFs.Dispose();
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~PptxFile()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
