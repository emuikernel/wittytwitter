using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System;
using System.Collections.Generic;

namespace Witty
{
    internal class SkinsManager
    {
        internal static List<string> GetSkins()
        {
            List<string> skins = new List<string>();

            skins.Add("Aero");
            skins.Add("AeroCompact");
            skins.Add("CoolBlue");

            return skins;
        }

        internal static void ChangeSkin(string skin)
        {
            //Uri uri = new Uri("pack://application:,,,/WittySkins;Component/" + skin + ".xaml");
            //Application.Current.Resources.Source = uri;

            Uri resourceLocator = new Uri("WittySkins;component/" + Path.ChangeExtension(skin, ".xaml"), UriKind.RelativeOrAbsolute);
            Application.Current.Resources =  Application.LoadComponent(resourceLocator) as ResourceDictionary;
        }
    }
}
