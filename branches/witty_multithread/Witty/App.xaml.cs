using System.Windows;
using System.Windows.Threading;
using log4net;
using log4net.Config;
using TwitterLib;

namespace Witty
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : System.Windows.Application
    {
        public static readonly ILog Logger = LogManager.GetLogger("Witty.Logging");

        // Global variable for the user
        public static User LoggedInUser = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            DOMConfigurator.Configure();

            Logger.Info("Witty is starting.");

            Properties.Settings appSettings = Witty.Properties.Settings.Default;
            if (appSettings.UpgradeSettings)
            {
                Witty.Properties.Settings.Default.Upgrade();
                appSettings.UpgradeSettings = false;
            }

            if (!string.IsNullOrEmpty(appSettings.Skin))
            {
                try
                {
                    SkinsManager.ChangeSkin(appSettings.Skin);
                }
                catch
                {
                    Logger.Error("Selected skin " + appSettings.Skin + " + not found");
                    // REVIEW: Should witty do something smart here?
                }
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// DispatcherUnhandledException is used to catch all unhandled exceptions.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // log error for debugging.
            App.Logger.Error("Unhandled Exception", e.Exception);

            //REMARK: Should we handle the exception and do something more user-friendly here?
            MessageBox.Show("Witty has encountered an unexpected error. Please restart Witty");
        }
    }
}