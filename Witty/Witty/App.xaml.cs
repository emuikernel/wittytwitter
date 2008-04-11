using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Windows;
using TwitterLib;
using log4net;
using log4net.Config;
using System.Windows.Threading;

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
                    ResourceDictionary rd = new ResourceDictionary();
                    rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(appSettings.Skin, UriKind.Relative)) as ResourceDictionary);
                    Application.Current.Resources = rd;
                }
                catch
                {
                    Logger.Error("Selected skin not found");
                    // REVIEW: Should witty do something smart here?
                }
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Get the collection of skins
        /// </summary>
        public static NameValueCollection Skins
        {
            get
            {
                return SkinsManager.GetSkins();
            }
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