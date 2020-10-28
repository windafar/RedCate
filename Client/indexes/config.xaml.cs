using Sercher;
using System;
using System.Collections.Generic;
using System.IO;
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

        private void RemoveServiceDb_Click(object sender, RoutedEventArgs e)
        {
            MessageListBox.Items.Add("正在删除索引数据库");
            MainWindow.sercherServerBase.RemoveAllDBData();
            if (MessageBox.Show("删除配置文件吗？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) 
            {
                if (File.Exists(Config.ConfigFilePath))
                    File.Delete(Config.ConfigFilePath);
                Config.Init(true);
                MessageListBox.Items.Add("初始化索引数据库完成");
            }
        }

        private void MessageListBox_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalMsg.globalMsgHand += PrintMsg;
        }

        private void PrintMsg(string msg, object data)
        {
            Dispatcher.Invoke(() =>
            {
                MessageListBox.Items.Add("内部异常：" + msg);
            });
        }

        private void MessageListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalMsg.globalMsgHand -= PrintMsg;

        }

        private void StopIndexButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearIndexesData_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.sercherServerBase.ClearIndexesDocs();
        }
    }
}
