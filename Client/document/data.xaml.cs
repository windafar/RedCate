using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sercher;
using System.IO;

namespace Client.document
{
    /// <summary>
    /// data.xaml 的交互逻辑
    /// </summary>
    public partial class data : Page
    {
        DocumentDB documentDB;
        public data()
        {
            InitializeComponent();
            documentDB = new DocumentDB(dbName: Config.CurrentConfig.DocumentsDBName, ip: Config.CurrentConfig.DocumentsDBIp);
        }

        private void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            
            ChartView.ItemsSource = documentDB.GetDocuments();
        }

        private string[] SelectFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "文档文件(*.doc,*.xls,*.txt)|*.doc;*.xls;*.txt|All files (*.*)|*.*",
                Multiselect = true,//: \"图像文件(*.bmp, *.jpg)|*.bmp;*.jpg|所有文件(*.*)|*.*\"”

            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileNames;
            }
            else
            {
                return null;
            }
        }

        private void SelectBtuuon_Click(object sender, RoutedEventArgs e)
        {
            SelectBtuuon.IsEnabled = false;
            string[] files = SelectFile();
            if(files!=null)
            foreach (var path in files
                                    .Where(x => x.LastIndexOf(".txt") != -1
                                    || x.LastIndexOf(".doc") != -1
                                    || x.LastIndexOf(".xls") != -1
                                    || x.LastIndexOf(".html") != -1
                                    || x.LastIndexOf(".xhtml") != -1
                ))documentDB.UploadDocument(new Document() { hasIndexed = "no", Name = new FileInfo(path).Name, Url = path });
            SelectBtuuon.IsEnabled = true ;
            ChartView.ItemsSource = documentDB.GetDocuments();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            documentDB.ResetDocumentIndexStatus();
            ChartView.ItemsSource = documentDB.GetDocuments();
        }

        private void ChartView_Click(object sender, RoutedEventArgs e)
        {
            var elem = e.OriginalSource as FrameworkElement;
            if (elem != null && elem.DataContext as Document != null)
            {
                if (elem.Name == "RemoveButton")
                    documentDB.DelDocumentById((elem.DataContext as Document)._id);
                else if (elem.Name == "RemoveIndexButton")
                {
                    elem.IsEnabled = false;
                    documentDB.ResetDocumentIndexStatus((elem.DataContext as Document)._id);
                    if(MessageBox.Show("是否移除索引","",MessageBoxButton.YesNo, MessageBoxImage.Question)==  MessageBoxResult.Yes)
                        MainWindow.sercherServerBase.RemoveIndexByDoc((elem.DataContext as Document));
                    
                    elem.IsEnabled = true;

                }
            }
        }
    }
}
