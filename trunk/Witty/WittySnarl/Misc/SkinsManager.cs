using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace Witty
{
    internal class SkinsManager
    {
        internal static NameValueCollection GetSkins()
        {
            NameValueCollection skins = new NameValueCollection();

            foreach (string folder in Directory.GetDirectories(@".\"))
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (string.Compare(fileInfo.Extension, ".xaml", true, CultureInfo.InvariantCulture) == 0)
                    {
                        // Use the first part of the resource file name for the menu item name.
                        //skins.Add(fileInfo.Name.Remove(fileInfo.Name.IndexOf(".xaml")),
                        //    Path.Combine(folder, fileInfo.Name));

                        skins.Add(Path.Combine(folder, fileInfo.Name), Path.Combine(folder, fileInfo.Name));
                    }
                }
            }
            return skins;
        }
    }
}
