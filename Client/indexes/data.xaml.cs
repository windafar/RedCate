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
using System.Windows.Controls.Primitives;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Client.indexes
{
    /// <summary>
    /// data.xaml 的交互逻辑
    /// </summary>
    public partial class data : Page
    {
        public data()
        {
            InitializeComponent();
        }
        private void Chart_Loaded(object sender, RoutedEventArgs e)
        {
            var total = MainWindow.sercherServerBase
                .GetHashNodes().Count();
            ChartView.ItemsSource = MainWindow.sercherServerBase
                 .GetHashNodes().AsParallel()
                 .Select((x => new
                 {
                     value = x.Value.GetSercherIndexCollectionCount(),
                     totalvalue=x.Value.GetRowTotal(),
                     database = x.Value.DbName,
                     ip=x.Value.Ip,
                     hash = x.Key,
                     db=x.Value
                 })).ToList()
                 .OrderBy(x => x.hash);
            
        }

        private void ChartView_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource as Button != null)
            {
                Button b = e.OriginalSource as Button;
                if (b.Name == "RemoveButton") 
                {
                    dynamic item = b.DataContext;
                    MainWindow.sercherServerBase.RemoveIndexServiceNodes(item.db);
                }
            }
        }

    }
}
