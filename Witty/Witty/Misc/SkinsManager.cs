using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System;

namespace Witty
{
    internal class SkinsManager
    {
        internal static NameValueCollection GetSkins()
        {
            NameValueCollection skins = new NameValueCollection();

            skins.Add("Aero", "pack://application:,,,/WittySkins;Component/Aero.xaml");
            skins.Add("AeroCompact", "pack://application:,,,/WittySkins;Component/AeroCompact.xaml");
            skins.Add("CoolBlue", "pack://application:,,,/WittySkins;Component/CoolBlue.xaml");

            return skins;
        }

        internal static void ChangeSkin(string skin)
        {
            Uri uri = new Uri("pack://application:,,,/WittySkins;Component/" + skin + ".xaml");
            Application.Current.Resources.Source = uri;
        }
    }
}
