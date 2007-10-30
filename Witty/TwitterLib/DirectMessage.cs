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
            set { dateCreated = value; }
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
            return (this.Id == other.Id);
        }

        #endregion
    }

    public class DirectMessages : ObservableCollection<DirectMessage>
    {
    }
}
