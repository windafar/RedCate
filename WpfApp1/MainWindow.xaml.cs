using Sercher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public SercherServerBase sercherServerBase { set; get; }

        public MainWindow()
        {
            InitializeComponent();
            Sercher.Config.Init();
            sercherServerBase = new Sercher.SercherServerBase();

        }

        private void IndexDataTreeItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavFrame.Navigate(new Uri("./indexes/data.xaml",UriKind.Relative), sercherServerBase);
        }

        private void DocDataTreeItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavFrame.Navigate(new Uri("./document/data.xaml", UriKind.Relative));
        }

    }
}
