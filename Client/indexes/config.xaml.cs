using Sercher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.indexes
{
    /// <summary>
    /// config.xaml 的交互逻辑
    /// </summary>
    public partial class config : Page
    {
        Thread t;
        public config()
        {
            InitializeComponent();
        }
        private void StartIndexesButton_Click(object sender, RoutedEventArgs e)
        {
            t = new Thread(() =>
            {
                MainWindow.sercherServerBase.BuildSercherIndexToSQLDB((pro, msg) =>
                {
                    Dispatcher.Invoke((ThreadStart)(() =>
                    {
                        IndexesProgressBar.Value = pro;
                        MessageListBox.Items.Add(msg);
                    }));
                });
            });
            t.Start();
        }

        private void AddServiceDBButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.sercherServerBase.AddIndexesNode(new SercherIndexesDB(
    "WIN-T9ASCBISP3P\\MYSQL", "Indexes" + DateTime.Now.Ticks.ToString("X")));
        }
    }
}
