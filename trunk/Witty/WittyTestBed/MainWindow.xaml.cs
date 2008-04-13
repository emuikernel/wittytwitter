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
using System.Windows.Shapes;

namespace WittyTestBed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TweetScanButton_Click(object sender, RoutedEventArgs e)
        {
            tweetscan ts = new tweetscan();
            ts.Show();
        }

        private void CustomWindowButton_Click(object sender, RoutedEventArgs e)
        {
            CustomWindow cw = new CustomWindow();
            cw.Show();
        }

        private void FollowersButton_Click(object sender, RoutedEventArgs e)
        {
            Followers f = new Followers();
            f.Show();
        }
    }
}
