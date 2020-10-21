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


namespace Client.document
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
            var documentDB = new DocumentDB(dbName: Config.CurrentConfig.DocumentsDBName, ip: Config.CurrentConfig.DocumentsDBIp);
            ChartView.ItemsSource = documentDB.GetDocuments();
        }
    }
}
