using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.Windows.Markup;
using System.Windows.Documents;

namespace TwitterLib
{
    /// <summary>
    /// Represents the status post for a Twitter User.
    /// </summary>
    [Serializable]
    public class Tweet : INotifyPropertyChanged, IEquatable<Tweet>
    {
        #region Private fields

        private double id;
        private DateTime? dateCreated;
        private string text;
        private string source;
        private User user;

        #endregion

        #region Public Properties
        /// <summary>
        /// The Tweet id 
        /// </summary>
        public double Id
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        /// <summary>
        /// Date and time the tweet was added
        /// </summary>
        public DateTime? DateCreated
        {
            get { return dateCreated; }
            set
            {
                if (value != dateCreated)
                {
                    dateCreated = value;
                    OnPropertyChanged("DateCreated");
                    OnPropertyChanged("RelativeTime");
                }
            }
        }

        /// <summary>
        /// The Tweet text
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    OnPropertyChanged("Text");
                    OnPropertyChanged("Hyperlink");
                }
            }
        }

        /// <summary>
        /// The Tweet source
        /// </summary>
        public string Source
        {
            get { return "from " + source; }
            set
            {
                if (value != source)
                {
                    source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        public string Hyperlink
        {
            get { return StringUtils.GetHyperlink(text); }
        }

        /// <summary>
        /// Twitter User associated with the Tweet
        /// </summary>
        public User User
        {
            get { return user; }
            set
            {
                if (value != user)
                {
                    user = value;
                    OnPropertyChanged("User");
                }
            }
        }

        /// <summary>
        /// How long ago the tweet was added based on DatedCreated and DateTime.Now
        /// </summary>
        public string RelativeTime
        {
            get
            {
                if (!dateCreated.HasValue)
                    return string.Empty;

                DateTime StatusCreatedDate = (DateTime)dateCreated;

                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - StatusCreatedDate.Ticks);
                double delta = ts.TotalSeconds;

                if (delta < 60)
                {
                    return ts.Seconds + " seconds ago";
                }
                else if (delta < 120)
                {
                    return "about a minute ago";
                }
                else if (delta < (45 * 60))
                {
                    return ts.Minutes + " minutes ago";
                }
                else if (delta < (90 * 60))
                {
                    return "about an hour ago";
                }
                else if (delta < (24 * 60 * 60))
                {
                    return "about " + ts.Hours + " hours ago";
                }
                else if (delta < (48 * 60 * 60))
                {
                    return "1 day ago";
                }
                else
                {
                    return ts.Days + " days ago";
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// INotifyPropertyChanged requires a property called PropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the event for the property when it changes.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IEquatable<Tweet> Members

        /// <summary>
        /// Tweets are the same if they have the same Id.
        /// Collection.Contains needs IEquatable implemented to be effective. 
        /// </summary>
        public bool Equals(Tweet other)
        {
            return (this.Id == other.Id);
        }

        #endregion
    }

    /// <summary>
    /// Collection of Tweets
    /// </summary>
    [Serializable]
    public class Tweets : ObservableCollection<Tweet>
    {
        #region Class Constants

        private class Const
        {
            public const string ApplicationFolderName = "Witty";

            public const string SaveFileName = "lastupdated.xaml";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Absolute file path to the last updated file.  The save file is stored in the user's MyDocuments folder under this application folder.
        /// </summary>
        private static string SaveFileAbsolutePath
        {
            get
            {
                string applicationFolderAbsolutePath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Const.ApplicationFolderName);

                // Create the application directory if it doesn't already exist
                if (!Directory.Exists(applicationFolderAbsolutePath))
                    Directory.CreateDirectory(applicationFolderAbsolutePath);

                return Path.Combine(applicationFolderAbsolutePath, Const.SaveFileName);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Persist the current list of Twitter Tweets to disk.
        /// </summary>
        public void SaveToDisk()
        {
            try
            {
                                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(SaveFileAbsolutePath, settings))
                {


                    XamlWriter.Save(this, writer);
                    writer.Close();
                }

            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine(ex.Message);

                // Rethrow so we know about the error
                throw;
            }
        }

        /// <summary>
        /// Load the list of Tweets from disk
        /// </summary>
        public static Tweets LoadFromDisk()
        {
            if (!File.Exists(SaveFileAbsolutePath))
                return new Tweets();

            Tweets tweets;

            //try
            //{
                //XmlSerializer xml = new XmlSerializer(typeof(Tweets));
                //using (Stream stream = new FileStream(SaveFileAbsolutePath,
                //    FileMode.Open, FileAccess.Read, FileShare.Read))
                //{
                //    //Tweets tweets2 = (Tweets)xml.Deserialize(stream);

                //    stream.Close();
                //}

                //return new Tweets();

            XmlReader xmlReader = XmlReader.Create(SaveFileAbsolutePath);
            tweets = XamlReader.Load(xmlReader) as Tweets;
            xmlReader.Close();

            return tweets;
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //    return new Tweets();
            //}
        }

        #endregion
    }
}
