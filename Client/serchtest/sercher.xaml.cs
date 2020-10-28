using Sercher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.serchtest
{
    /// <summary>
    /// sercher.xaml 的交互逻辑
    /// </summary>
    public partial class sercher : Page
    {
        public sercher()
        {
            InitializeComponent();
        }

        private void SercherTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander != null && (expander).DataContext != null)
            {
                dynamic data = (expander).DataContext;
                int num = 5;
                expander.Content = "";
                foreach (var beginindex in data.beginIndex) {
                    if (--num == 0) break;
                    string filestr = Component.Default.TextHelper.BeforeEncodingClass.GetText(File.ReadAllBytes(data.Url));
                    int strt = beginindex - Math.Min(beginindex, 50);
                    int end = Math.Min(filestr.Length, strt + 100);
                    expander.Content += filestr.Substring(strt, end - strt)+"\r\n";
                }
            }
        }

        private void SercherButton_Click(object sender, RoutedEventArgs e)
        {
            SercherListView.ItemsSource =
                   MainWindow.sercherServerBase.Searcher(SercherTextBox.Text)
                       .Select(x => new
                       {
                           x.documentId,
                           x.dependency,
                           x.doc.Name,
                           x.doc.Url,
                           x.beginIndex,
                       });

        }
    }
}
