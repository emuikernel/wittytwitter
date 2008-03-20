using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using log4net;
using System.Windows.Forms;

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
