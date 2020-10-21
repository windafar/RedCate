using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Start.xaml 的交互逻辑
    /// </summary>
    public partial class Start : Page
    {
        public Start()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Sercher.Config.Init();
            Message.Text += "IndexIP:" + Sercher.Config.CurrentConfig
                .IndexesServerlists.Select((x) => x.Ip).Aggregate((x, y) => x + "/r/n" + y + "/r/n");
            Message.Text += "docIP:" + Sercher.Config.CurrentConfig.DocumentsDBIp;
        }
    }
}
