using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using JiebaNet.Segmenter;
using Component.Default;

namespace Component
{
    [ComponentPram(FileType: "xlsx,xls")]
    class XlsxFile : TextComponent, IDisposable
    {
        public XlsxFile(Stream inputFs) : base(inputFs)
        {
        }

        protected override string ConvertInputToString()
        {
            var text = ConvertXlsxToCsv(inputFs);
            Dispose();//手动关闭文件
            return text;
        }

        /// <summary>
        /// Convert Xlsx To Csv
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="DestinationCsvDirectory"></param>
        /// <remarks>rewrited from network</remarks>
        private string ConvertXlsxToCsv(Stream Source)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(Source, false))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        using (StreamWriter outputFile = new StreamWriter(stream))
                        {
                            foreach (Sheet _Sheet in document.WorkbookPart.Workbook.Descendants<Sheet>())
                            {
                                WorksheetPart _WorksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(_Sheet.Id);
                                Worksheet _Worksheet = _WorksheetPart.Worksheet;

                                SharedStringTablePart _SharedStringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                                SharedStringItem[] _SharedStringItem = _SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                                foreach (var row in _Worksheet.Descendants<Row>())
                                {
                                    StringBuilder _StringBuilder = new StringBuilder();
                                    foreach (Cell _Cell in row)
                                    {
                                        string Value = string.Empty;
                                        if (_Cell.CellValue != null)
                                        {
                                            if (_Cell.DataType != null && _Cell.DataType.Value == CellValues.SharedString)
                                                Value = _SharedStringItem[int.Parse(_Cell.CellValue.Text)].InnerText;
                                            else
                                                Value = _Cell.CellValue.Text;
                                        }
                                        _StringBuilder.Append(string.Format("{0}", Value.Trim()));
                                    }
                                    outputFile.WriteLine(_StringBuilder.ToString().TrimEnd(','));
                                }
                            }
                            stream.Seek(0, SeekOrigin.Begin);
                            return new StreamReader(stream).ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }



        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    inputFs.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }
        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Xlsx()
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
