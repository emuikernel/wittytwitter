using System.Windows;
using System.Windows.Controls;

namespace Witty.Controls.Search
{
    public partial class TweetHunt : UserControl
    {
        public TweetHunt()
        {
            InitializeComponent();
            _model = new TweetHuntViewModel();            
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            DataContext = _model;
        }

        private void OnAddSearchItem(object sender, System.Windows.RoutedEventArgs e)
        {
            _model.AddCurrentWordAsSearchTerm();
        }

        private void OnDeleteSearchItem(object sender, System.Windows.RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if(element != null)
            {
                _model.DeleteTerm(element.DataContext as SearchItem);
            }
        }
        
        private TweetHuntViewModel _model;
    }
}
