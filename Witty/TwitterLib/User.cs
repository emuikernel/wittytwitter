using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TwitterLib
{
    /// <summary>
    /// A Twitter User
    /// </summary>
    [Serializable]
    public class User
    {
        private int id;
        private string name;
        private string screenName;
        private string imageUrl;
        private string siteUrl;
        private string location;
        private string description;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ScreenName
        {
            get { return screenName; }
            set { screenName = value; }
        }

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }

        public string SiteUrl
        {
            get { return siteUrl; }
            set { siteUrl = value; }
        }

        public string TwitterUrl
        {
            get
            {
                return "http://twitter.com/" + screenName;
            }
        }

        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// The user name along with the screenname. This makes binding a little easier.
        /// </summary>
        public string FullName
        {
            get { return Name + " (" + screenName + ")"; }
        }

        private string backgroundColor;

        public string BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private string textColor;

        public string TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }
        private string linkColor;

        public string LinkColor
        {
            get { return linkColor; }
            set { linkColor = value; }
        }

        private string sidebarFillColor;

        public string SidebarFillColor
        {
            get { return sidebarFillColor; }
            set { sidebarFillColor = value; }
        }

        private string sidebarBorderColor;

        public string SidebarBorderColor
        {
            get { return sidebarBorderColor; }
            set { sidebarBorderColor = value; }
        }

        private Tweet tweet;

        public Tweet Tweet
        {
            get { return tweet; }
            set { tweet = value; }
        }

        private int followingCount;

        public int FollowingCount
        {
            get { return followingCount; }
            set { followingCount = value; }
        }

        private int followersCount;

        public int FollowersCount
        {
            get { return followersCount; }
            set { followersCount = value; }
        }

        private int statusesCount;

        public int StatusesCount
        {
            get { return statusesCount; }
            set { statusesCount = value; }
        }

        private int favoritesCount;

        public int FavoritesCount
        {
            get { return favoritesCount; }
            set { favoritesCount = value; }
        }
    }

    [Serializable]
    public class UserCollection : ObservableCollection<User>
    {
    }
}
