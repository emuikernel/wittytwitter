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
using TwitterLib;

namespace WittyTestBed
{
    /// <summary>
    /// Interaction logic for Followers.xaml
    /// </summary>
    public partial class Followers : Window
    {
        public Followers()
        {
            InitializeComponent();

            TwitterNet twitter = new TwitterNet();
            FriendsListBox.ItemsSource = twitter.GetFriends();
        }
    }
}
