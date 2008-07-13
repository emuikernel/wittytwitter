using System;
using System.Net;
using log4net;

namespace Witty
{
    public static class WebProxyHelper
    {
        private static Properties.Settings AppSettings = Properties.Settings.Default;
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        public static IWebProxy GetConfiguredWebProxy()
        {   
            WebProxy proxy = null;
            if (AppSettings.UseProxy)
            {
                try
                {
                    proxy = new WebProxy(AppSettings.ProxyServer, AppSettings.ProxyPort);

                    string[] user = AppSettings.ProxyUsername.Split('\\');

                    if (user.Length == 2)
                        proxy.Credentials = new NetworkCredential(user[1], AppSettings.ProxyPassword, user[0]);
                    else
                        proxy.Credentials = new NetworkCredential(AppSettings.ProxyUsername, AppSettings.ProxyPassword);
                }
                catch (UriFormatException ex)
                {
                    logger.Debug(ex.ToString());
                }
            }
            return proxy;
        }
    }
}
