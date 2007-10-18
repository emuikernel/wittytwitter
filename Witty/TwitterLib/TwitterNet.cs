using System;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.IO;
using System.Globalization;
using System.Web;
using System.Text.RegularExpressions;

namespace TwitterLib
{
    /// <summary>
    /// .NET wrapper for Twitter API calls
    /// </summary>
    public class TwitterNet
    {
        #region Private Fields

        private string username;
        private string password;
        private string publicTimelineUrl;
        private string friendsTimelineUrl;
        private string userTimelineUrl;
        private string repliesTimelineUrl;
        private string directMessagesUrl; 
        private string updateUrl;
        private string friendsUrl;
        private string followersUrl;
        private string userShowUrl; 
        private string format;

        // TODO: might need to fix this for globalization
        private string twitterCreatedAtDateFormat = "ddd MMM dd HH:mm:ss zzzz yyyy"; // Thu Apr 26 01:36:08 +0000 2007
        private string twitterSinceDateFormat = "ddd MMM dd yyyy HH:mm:ss zzzz";

        private static int characterLimit;

        #endregion

        #region Public Properties

        /// <summary>
        /// Twitter username
        /// </summary>
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }

        /// <summary>
        /// Twitter password
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        ///  Url to the Twitter Public Timeline. Defaults to http://twitter.com/statuses/public_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string PublicTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(publicTimelineUrl))
                    return "http://twitter.com/statuses/public_timeline";
                else
                    return publicTimelineUrl;
            }
            set { publicTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the Twitter Friends Timeline. Defaults to http://twitter.com/statuses/friends_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FriendsTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(friendsTimelineUrl))
                    return "http://twitter.com/statuses/friends_timeline";
                else
                    return friendsTimelineUrl;
            }
            set { friendsTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the user's timeline. Defaults to http://twitter.com/statuses/user_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UserTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(userTimelineUrl))
                    return "http://twitter.com/statuses/user_timeline";
                else
                    return userTimelineUrl;
            }
            set { userTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the 20 most recent replies (status updates prefixed with @username posted by users who are friends with the user being replied to) to the authenticating user. 
        /// Defaults to http://twitter.com/statuses/user_timeline
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string RepliesTimelineUrl
        {
            get
            {
                if (string.IsNullOrEmpty(repliesTimelineUrl))
                    return "http://twitter.com/statuses/replies";
                else
                    return repliesTimelineUrl;
            }
            set { repliesTimelineUrl = value; }
        }

        /// <summary>
        /// Url to the list of the 20 most recent direct messages sent to the authenticating user.  Defaults to http://twitter.com/direct_messages
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DirectMessagesUrl
        {
            get
            {
                if (string.IsNullOrEmpty(directMessagesUrl))
                    return "http://twitter.com/direct_messages";
                else
                    return directMessagesUrl;
            }
            set { directMessagesUrl = value; }
        }

        /// <summary>
        /// Url to the Twitter HTTP Post. Defaults to http://twitter.com/statuses/update
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UpdateUrl
        {
            get
            {
                if (string.IsNullOrEmpty(updateUrl))
                    return "http://twitter.com/statuses/update";
                else
                    return updateUrl;
            }
            set { updateUrl = value; }
        }

        /// <summary>
        /// Url to the user's friends. Defaults to http://twitter.com/statuses/friends
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FriendsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(friendsUrl))
                    return "http://twitter.com/statuses/friends";
                else
                    return friendsUrl;
            }
            set { friendsUrl = value; }
        }

        /// <summary>
        /// Url to the user's followers. Defaults to http://twitter.com/statuses/followers
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string FollowersUrl
        {
            get
            {
                if (string.IsNullOrEmpty(followersUrl))
                    return "http://twitter.com/statuses/followers";
                else
                    return followersUrl;
            }
            set { followersUrl = value; }
        }

        /// <summary>
        /// Returns extended information of a given user, specified by ID or screen name as per the required id parameter below.  
        /// This information includes design settings, so third party developers can theme their widgets according to a given user's preferences. 
        /// Defaults to http://twitter.com/users/show/
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string UserShowUrl
        {
            get
            {
                if (string.IsNullOrEmpty(userShowUrl))
                    return "http://twitter.com/users/show/";
                else
                    return userShowUrl;
            }
            set { userShowUrl = value; }
        }	

        /// <summary>
        /// The format of the results from the twitter API. Ex: .xml, .json, .rss, .atom. Defaults to ".xml"
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string Format
        {
            get
            {
                if (string.IsNullOrEmpty(format))
                    return ".xml";
                else
                    return format;
            }
            set { format = value; }
        }

        /// <summary>
        /// The number of characters available for the Tweet text. Defaults to 140.
        /// </summary>
        /// <remarks>
        /// This value should only be changed if the character limit on Twitter.com has been changed.
        /// </remarks>
        public static int CharacterLimit
        {
            get
            {
                if (characterLimit == 0)
                    return 140;
                else
                    return characterLimit;
            }
            set { characterLimit = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Unauthenticated constructor
        /// </summary>
        public TwitterNet()
        {
        }

        /// <summary>
        /// Authenticated constructor
        /// </summary>
        public TwitterNet(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the public timeline
        /// </summary>
        public Tweets GetPublicTimeline()
        {
            return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the public timeline. Narrows the result to after the since date.
        /// </summary>
        public Tweets GetPublicTimeline(string since)
        {
            return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline
        /// </summary>
        public Tweets GetFriendsTimeline()
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends);
            else
                return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public Tweets GetFriendsTimeline(string since)
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends, since);
            else
                return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public Tweets GetFriendsTimeline(string since, string userId)
        {
            return RetrieveTimeline(Timeline.Friends, string.Empty, userId);
        }

        public Tweets GetUserTimeline(string userId)
        {
            return RetrieveTimeline(Timeline.User, "", userId);
        }

        /// <summary>
        /// Post new tweet to Twitter
        /// </summary>
        /// <returns>newly added tweet</returns>
        public Tweet AddTweet(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = HttpUtility.UrlEncode(text);

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(UpdateUrl + Format) as HttpWebRequest;

            // Add authentication to request  
            request.Credentials = new NetworkCredential(username, password);

            request.Method = "POST";

            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "status=" + text;
            string sourceParam = "&source=Witty";
            request.ContentLength = param.Length + sourceParam.Length;

            // Write the request paramater
            StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(param);
            stOut.Write(sourceParam);
            stOut.Close();

            Tweet tweet;

            // Do the request to get the response
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Create a new XmlDocument  
                XmlDocument doc = new XmlDocument();

                // Load data  
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("status");

                tweet = new Tweet();

                tweet.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);
                string source = HttpUtility.HtmlDecode(node.SelectSingleNode("source").InnerText);
                if (!string.IsNullOrEmpty(source))
                    tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                string dateString = node.SelectSingleNode("created_at").InnerText;
                if (!string.IsNullOrEmpty(dateString))
                {
                    tweet.DateCreated = DateTime.ParseExact(
                        dateString,
                        twitterCreatedAtDateFormat,
                        CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
                }
                tweet.IsNew = true;

                User user = new User();
                XmlNode userNode = node.SelectSingleNode("user");
                user.Name = userNode.SelectSingleNode("name").InnerText;
                user.ScreenName = userNode.SelectSingleNode("screen_name").InnerText;
                user.ImageUrl = userNode.SelectSingleNode("profile_image_url").InnerText;
                user.SiteUrl = userNode.SelectSingleNode("url").InnerText;
                user.Location = userNode.SelectSingleNode("location").InnerText;
                user.Description = userNode.SelectSingleNode("description").InnerText;
                tweet.User = user;
            }

            return tweet;
        }

        /// <summary>
        /// Authenticating with the provided credentials and retrieve the user's settings
        /// </summary>
        /// <returns></returns>
        public User Login()
        {
            string timelineUrl = UserShowUrl + username + Format;

            User user = new User();

            // Create the web request
            HttpWebRequest request = WebRequest.Create(timelineUrl) as HttpWebRequest;

            // Add credendtials to request  
            request.Credentials = new NetworkCredential(username, password);

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Create a new XmlDocument  
                    XmlDocument doc = new XmlDocument();

                    // Load data  
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNode userNode = doc.SelectSingleNode("user");

                    if (userNode != null)
                    {
                        user.Name = userNode.SelectSingleNode("name").InnerText;
                        user.ScreenName = userNode.SelectSingleNode("screen_name").InnerText;
                        user.ImageUrl = userNode.SelectSingleNode("profile_image_url").InnerText;
                        user.SiteUrl = userNode.SelectSingleNode("url").InnerText;
                        user.Location = userNode.SelectSingleNode("location").InnerText;
                        user.Description = userNode.SelectSingleNode("description").InnerText;
                        user.BackgroundColor = userNode.SelectSingleNode("profile_background_color").InnerText;
                        user.TextColor = userNode.SelectSingleNode("profile_text_color").InnerText;
                        user.LinkColor = userNode.SelectSingleNode("profile_link_color").InnerText;
                        user.SidebarBorderColor = userNode.SelectSingleNode("profile_sidebar_border_color").InnerText;
                        user.SidebarFillColor = userNode.SelectSingleNode("profile_sidebar_fill_color").InnerText;
                    }
                }
            }
            catch (WebException webExcp)
            {
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                // there has been a protocol error and a WebResponse 
                // should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;

                    // Check for 401 Unauthorized. Rethrow every other WebException.
                    if ((int)httpResponse.StatusCode == 401)
                        return null;
                    else
                        throw webExcp;
                }
                else
                    throw webExcp;
            }

            return user;
        }

        public Users GetFriends()
        {
            Users users = new Users();

            // Create the web request
            HttpWebRequest request = WebRequest.Create(FriendsUrl + Format) as HttpWebRequest;

            // Add credendtials to request  
            request.Credentials = new NetworkCredential(username, password);

            try
            {
                // Get the Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Create a new XmlDocument  
                    XmlDocument doc = new XmlDocument();

                    // Load data  
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/users/user");

                    foreach (XmlNode node in nodes)
                    {
                        User user = new User();
                        user.Id = int.Parse(node.SelectSingleNode("id").InnerText);
                        user.Name = node.SelectSingleNode("name").InnerText;
                        user.ScreenName = node.SelectSingleNode("screen_name").InnerText;
                        user.ImageUrl = node.SelectSingleNode("profile_image_url").InnerText;
                        user.SiteUrl = node.SelectSingleNode("url").InnerText;
                        user.Location = node.SelectSingleNode("location").InnerText;
                        user.Description = node.SelectSingleNode("description").InnerText;

                        Tweet tweet = new Tweet();
                        XmlNode statusNode = node.SelectSingleNode("status");

                        //string dateString = statusNode.SelectSingleNode("created_at").InnerText;
                        //if (!string.IsNullOrEmpty(dateString))
                        //{
                        //    tweet.DateCreated = DateTime.ParseExact(
                        //        dateString,
                        //        twitterCreatedAtDateFormat,
                        //        CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
                        //}
                        //tweet.Id = double.Parse(statusNode.SelectSingleNode("id").InnerText);
                        //tweet.Text = HttpUtility.HtmlDecode(statusNode.SelectSingleNode("text").InnerText);
                        //tweet.Source = statusNode.SelectSingleNode("source").InnerText;
                        //user.Tweet = tweet;

                        users.Add(user);
                    }

                }
            }
            catch (WebException webExcp)
            {
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                //   there has been a protocol error and a WebResponse 
                //   should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;

                    // 304 Not modified = no new tweets so ignore error.  Rethrow every other WebException.
                    if ((int)httpResponse.StatusCode != 304)
                        throw webExcp;
                }
            }
            return users;
        }

        public Tweets GetReplies()
        {
            return RetrieveTimeline(Timeline.Replies);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private Tweets RetrieveTimeline(Timeline timeline)
        {
            return RetrieveTimeline(timeline, string.Empty);
        }

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private Tweets RetrieveTimeline(Timeline timeline, string since)
        {
            return RetrieveTimeline(timeline, since, string.Empty);
        }

        /// <summary>
        /// The Main function for interfacing with the Twitter API
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="since"></param>
        /// <param name="userId"></param>
        /// <returns>Collection of Tweets. Twitter limits the max to 20.</returns>
        private Tweets RetrieveTimeline(Timeline timeline, string since, string userId)
        {
            Tweets tweets = new Tweets();

            string timelineUrl = string.Empty;

            switch (timeline)
            {
                case Timeline.Public:
                    timelineUrl = PublicTimelineUrl;
                    break;
                case Timeline.Friends:
                    timelineUrl = FriendsTimelineUrl;
                    break;
                case Timeline.User:
                    timelineUrl = UserTimelineUrl;
                    break;
                case Timeline.Replies:
                    timelineUrl = RepliesTimelineUrl;
                    break;
                default:
                    timelineUrl = PublicTimelineUrl;
                    break;
            }

            if (!string.IsNullOrEmpty(userId))
                timelineUrl += "/" + userId + Format;
            else
                timelineUrl += Format;

            if (!string.IsNullOrEmpty(since))
            {
                DateTime sinceDate = new DateTime();
                DateTime.TryParse(since, out sinceDate);

                // Go back a minute to compensate for latency.
                sinceDate = sinceDate.AddMinutes(-1);

                string sinceDateString = sinceDate.ToString(twitterSinceDateFormat);

                timelineUrl = timelineUrl + "?since=" + sinceDateString;
            }

            // Create the web request
            HttpWebRequest request = WebRequest.Create(timelineUrl) as HttpWebRequest;

            // Friends and Replies timeline requests need to be authenticated
            if (timeline == Timeline.Friends || timeline == Timeline.Replies)
            {
                // Add credendtials to request  
                request.Credentials = new NetworkCredential(username, password);
            }

            try
            {
                // Get the Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Create a new XmlDocument  
                    XmlDocument doc = new XmlDocument();

                    // Load data  
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/statuses/status");

                    foreach (XmlNode node in nodes)
                    {
                        Tweet tweet = new Tweet();
                        tweet.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                        tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);
                        string source = HttpUtility.HtmlDecode(node.SelectSingleNode("source").InnerText);
                        if (!string.IsNullOrEmpty(source))
                            tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                        string dateString = node.SelectSingleNode("created_at").InnerText;
                        if (!string.IsNullOrEmpty(dateString))
                        {
                            tweet.DateCreated = DateTime.ParseExact(
                                dateString,
                                twitterCreatedAtDateFormat,
                                CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
                        }

                        User user = new User();
                        XmlNode userNode = node.SelectSingleNode("user");
                        user.Name = userNode.SelectSingleNode("name").InnerText;
                        user.ScreenName = userNode.SelectSingleNode("screen_name").InnerText;
                        user.ImageUrl = userNode.SelectSingleNode("profile_image_url").InnerText;
                        user.SiteUrl = userNode.SelectSingleNode("url").InnerText;
                        user.Location = userNode.SelectSingleNode("location").InnerText;
                        user.Description = userNode.SelectSingleNode("description").InnerText;
                        tweet.User = user;

                        tweets.Add(tweet);
                    }

                    tweets.SaveToDisk();
                }
            }
            catch (WebException webExcp)
            {
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                //   there has been a protocol error and a WebResponse 
                //   should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;

                    // 304 Not modified = no new tweets so ignore error.  Rethrow every other WebException.
                    if ((int)httpResponse.StatusCode != 304)
                        throw webExcp;
                }
            }
            return tweets;
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of available Twitter timelines
    /// </summary>
    public enum Timeline
    {
        Public,
        Friends,
        User,
        Replies
    }
}