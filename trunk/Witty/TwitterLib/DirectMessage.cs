using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TwitterLib
{
    public class DirectMessage : INotifyPropertyChanged, IEquatable<DirectMessage>
    {
        private double id;

        public double Id
        {
            get { return id; }
            set { id = value; }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private DateTime dateCreated;

        public DateTime DateCreated
        {
            get { return dateCreated; }
            set
            {
                dateCreated = value;
                UpdateRelativeTime();
            }
        }

        private User sender;

        public User Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        private User recipient;

        public User Recipient
        {
            get { return recipient; }
            set { recipient = value; }
        }

        private bool isNew;

        public bool IsNew
        {
            get { return isNew; }
            set { isNew = value; }
        }

        private string relativeTime;

        /// <summary>
        /// How long ago the Direct Message was added based on DatedCreated and DateTime.Now
        /// </summary>
        public string RelativeTime
        {
            get
            {
                return relativeTime;
            }
            set
            {
                relativeTime = value;
                OnPropertyChanged("Relativetime");
            }
        }

        public void UpdateRelativeTime()
        {
            DateTime StatusCreatedDate = (DateTime)dateCreated;

            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - StatusCreatedDate.Ticks);
            double delta = ts.TotalSeconds;

            string relativeTime = string.Empty;

            if (delta == 1)
            {
                relativeTime = "a second ago";
            }
            else if (delta < 60)
            {
                relativeTime = ts.Seconds + " seconds ago";
            }
            else if (delta < 120)
            {
                relativeTime = "about a minute ago";
            }
            else if (delta < (45 * 60))
            {
                relativeTime = ts.Minutes + " minutes ago";
            }
            else if (delta < (90 * 60))
            {
                relativeTime = "about an hour ago";
            }
            else if (delta < (24 * 60 * 60))
            {
                relativeTime = "about " + ts.Hours + " hours ago";
            }
            else if (delta < (48 * 60 * 60))
            {
                relativeTime = "1 day ago";
            }
            else
            {
                relativeTime = ts.Days + " days ago";
            }

            RelativeTime = relativeTime;
        }

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

        #region IEquatable<DirectMessage> Members

        public bool Equals(DirectMessage other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            
            return (this.Id == other.Id);
        }

        #endregion
    }

    public class DirectMessageCollection : ObservableCollection<DirectMessage>
    {
    }
}
