using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Witty
{
    internal class SkinsManager
    {
        internal static NameValueCollection GetSkins()
        {
            NameValueCollection skins = new NameValueCollection();

            var rootFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            foreach (string folder in Directory.GetDirectories(rootFolder))
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (string.Compare(fileInfo.Extension, ".xaml", true, CultureInfo.InvariantCulture) == 0)
                    {
                        // Use the first part of the resource file name for the menu item name.
                        //skins.Add(fileInfo.Name.Remove(fileInfo.Name.IndexOf(".xaml")),
                        //    Path.Combine(folder, fileInfo.Name));

                        skins.Add(fileInfo.FullName.Substring(rootFolder.Length + 1), fileInfo.FullName);
                    }
                }
            }
            return skins;
        }
    }
}
