using System;
using System.Xml;
using System.Windows.Data;
using System.Windows;
using System.IO;
using System.Security;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using TwitterLib;
using System.Collections;

namespace WittyFollowings
{
    public class IsStringEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            XmlElement str = value as XmlElement;
            if (str != null)
            {
                XmlAttribute attribute = str.Attributes["Description"];
                if (attribute != null)
                {
                    string foo = attribute.Value;
                    if (foo != null && foo.Length > 0)
                    {
                        if (targetType == typeof(Visibility))
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            if (targetType == typeof(Visibility))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public static class Helpers
    {
        public static string[] GetPicturePaths()
        {
            string[] picturePaths = new string[0];

            string[] commandLineArgs = null;
            try
            {
                commandLineArgs = Environment.GetCommandLineArgs();
            }
            catch (NotSupportedException) { }


            if (commandLineArgs != null)
            {
                foreach (string arg in commandLineArgs)
                {
                    picturePaths = GetPicturePaths(arg);
                    if (picturePaths.Length > 0)
                    {
                        break;
                    }
                }
            }

            if (picturePaths.Length == 0)
            {
                //picturePaths = GetPicturePaths(DefaultPicturePath);
                picturePaths = GetTwitterPaths();
            }

            return picturePaths;
        }
        internal static string[] GetPicturePaths(string sourceDirectory)
        {

            if (!string.IsNullOrEmpty(sourceDirectory))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                    if (di.Exists)
                    {
                        FileInfo[] fis = di.GetFiles(DefaultImageSearchPattern);
                        string[] strings = new string[fis.Length];
                        for (int i = 0; i < fis.Length; i++)
                        {
                            strings[i] = fis[i].FullName;
                        }
                        return strings;
                    }
                }
                catch (IOException) { }
                catch (ArgumentException) { }
                catch (SecurityException) { }
            }
            return new string[0];
        }

        internal static string[] GetTwitterPaths()
        {
            TwitterNet twitter = new TwitterNet("a7an", "alan213");
            Users users = twitter.GetFriends();

            ArrayList stringArray = new ArrayList();
            foreach (User user in users)
            {
                //if (user.ImageUrl.Contains(".jpg"))
                    stringArray.Add(user.ImageUrl);
            }
            return (string[])stringArray.ToArray(typeof(string));
        }
        public static BitmapImage[] GetBitmapImages(int maxCount)
        {
            string[] imagePaths = GetPicturePaths();
            if (maxCount < 0)
            {
                maxCount = imagePaths.Length;
            }

            BitmapImage[] images = new BitmapImage[Math.Min(imagePaths.Length, maxCount)];
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = new BitmapImage(new Uri(imagePaths[i]));
            }
            return images;
        }


        public static string DefaultPicturePath = @"C:\Users\Public\Pictures\Sample Pictures\";
        public static string DefaultImageSearchPattern = @"*.jpg";
    }

    public class ImagePathHolder
    {
        public string[] ImagePaths
        {
            get
            {
                return Helpers.GetPicturePaths();
            }
        }
        public BitmapImage[] BitmapImages
        {
            get
            {
                return Helpers.GetBitmapImages(-1);
            }
        }
        public BitmapImage[] BitmapImages6
        {
            get
            {
                return Helpers.GetBitmapImages(6);
            }
        }

    }
}