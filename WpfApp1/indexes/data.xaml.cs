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
                 .GetHashNodes().ToList()
                 .Select((x => new
                 {
                     value = x.Value.GetSercherIndexCollectionCount(),
                     database = x.Value.DbName,
                     ip=x.Value.Ip,
                     hash = x.Key,
                     db=x.Value
                 }))
                 .OrderBy(x => x.hash);
            
        }

        private void ChartView_Click(object sender, RoutedEventArgs e)
        {
            dynamic item;
            if (e.OriginalSource as ButtonBase != null)
            {
                //item = ((ButtonBase)e.OriginalSource).DataContext;
                //ISercherIndexesDB db = item.db as ISercherIndexesDB;
                //long hash= (long)item.hash;
                MainWindow.sercherServerBase.AddIndexesNode(new SercherIndexesDB(
                    "WIN-T9ASCBISP3P\\MYSQL", "SercherIndexDatabaseZZ"));
            }
        }
    }
}
