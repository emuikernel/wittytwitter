using log4net;
using log4net.Config;

namespace Witty
{
    /// <summary>
    /// Interaction logic for PeopleYouFollow.xaml
    /// </summary>
    public partial class PeopleYouFollow : System.Windows.Window
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        public PeopleYouFollow()
        {
            InitializeComponent();
        }
    }
}