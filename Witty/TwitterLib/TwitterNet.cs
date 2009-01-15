using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace TwitterLib
{
    /// <summary>
    /// .NET wrapper for interacting with the Twitter API
    /// </summary>
    public class TwitterNet : IServiceApi
    {
        #region Private Fields
        private string username;
        private SecureString password;
        private string publicTimelineUrl;
        private string friendsTimelineUrl;
        private string userTimelineUrl;
        private string repliesTimelineUrl;
        private string directMessagesUrl;
        private string updateUrl;
        private string friendsUrl;
        private string followersUrl;
        private string userShowUrl;
        private string sendMessageUrl;
        private string destroyUrl;
        private string destroyDirectMessageUrl;
        private string createFriendshipUrl;
        private string format;
        private IWebProxy webProxy = HttpWebRequest.DefaultWebProxy;  // Jason Follas: Added initialization
        private string twitterServerUrl;                              // Jason Follas

        private User currentLoggedInUser;

        // REMARK: might need to fix this for globalization
        private readonly string twitterCreatedAtDateFormat = "ddd MMM dd HH:mm:ss zzzz yyyy"; // Thu Apr 26 01:36:08 +0000 2007
        private readonly string twitterSinceDateFormat = "ddd MMM dd yyyy HH:mm:ss zzzz";

        private static int characterLimit;
        private string clientName;

        #endregion

        #region Public Properties

        public User CurrentlyLoggedInUser
        {
            get
            {
                if (null == currentLoggedInUser)
                {
                    currentLoggedInUser = Login();
                }
                return currentLoggedInUser;
            }
            set
            {
                currentLoggedInUser = value;
            }
        }

        /// <summary>
        /// Twitter username
        /// </summary>
        public string UserName
        {
            get { return (null == currentLoggedInUser ? currentLoggedInUser.Name: String.Empty) ; }
        }

        /// <summary>
        /// Twitter password
        /// </summary>
        public SecureString Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Web proxy to use when communicating with the Twitter service
        /// </summary>
        public IWebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }


        /// <summary>
        /// URL of the Twitter host.  Defaults to http://twitter.com/.  Why would
        /// you need to override this?  Think: Alternate endpoints, like other
        /// services with identical API's, or maybe a Twitter Proxy.
        /// </summary>
        public string TwitterServerUrl
        {
            get
            {
                if (String.IsNullOrEmpty(twitterServerUrl))
                    return "http://twitter.com/";
                else
                    return twitterServerUrl;
            }
            set
            {
                twitterServerUrl = value;

                if (!twitterServerUrl.EndsWith("/"))
                    twitterServerUrl += "/";
            }
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
                    return TwitterServerUrl + "statuses/public_timeline";
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
                    return TwitterServerUrl + "statuses/friends_timeline";
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
                    return TwitterServerUrl + "statuses/user_timeline";
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
                    return TwitterServerUrl + "statuses/replies";
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
                    return TwitterServerUrl + "direct_messages";
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
                    return TwitterServerUrl + "statuses/update";
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
                    return TwitterServerUrl + "statuses/friends";
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
                    return TwitterServerUrl + "statuses/followers";
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
                    return TwitterServerUrl + "users/show/";
                else
                    return userShowUrl;
            }
            set { userShowUrl = value; }
        }

        /// <summary>
        /// Url to sends a new direct message to the specified user from the authenticating user. Defaults to http://twitter.com/direct_messages/new
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string SendMessageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(sendMessageUrl))
                    return TwitterServerUrl + "direct_messages/new";
                else
                    return sendMessageUrl;
            }
            set { sendMessageUrl = value; }
        }

        /// <summary>
        /// Url to destroy a status from the authenticating user. Defaults to http://twitter.com/statuses/destroy
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DestroyUrl
        {
            get
            {
                if (string.IsNullOrEmpty(destroyUrl))
                    return TwitterServerUrl + "statuses/destroy/";
                else
                    return destroyUrl;
            }
            set { destroyUrl = value; }
        }

        /// <summary>
        /// Url to destroy a direct message from the authenticating user. Defaults to http://twitter.com/direct_messages/destroy/
        /// </summary>
        /// <remarks>
        /// This value should only be changed if Twitter API urls have been changed on http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </remarks>
        public string DestroyDirectMessageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(destroyDirectMessageUrl))
                    return TwitterServerUrl + "direct_messages/destroy/";
                else
                    return destroyDirectMessageUrl;
            }
            set { destroyDirectMessageUrl = value; }
        }



        public string CreateFriendshipUrl
        {
            get {
                if (string.IsNullOrEmpty(createFriendshipUrl))
                {
                    return TwitterServerUrl + "friendships/create/";
                }
                else
                {
                    return createFriendshipUrl;
                }
            }
            set { createFriendshipUrl = value; }
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

        /// <summary>
        /// The name of the current client. Defaults to "Witty"
        /// </summary>
        /// <remarks>
        /// This value can be changed if you're using this Library for your own twitter client
        /// </remarks>
        public string ClientName
        {
            get
            {
                if (string.IsNullOrEmpty(clientName))
                    return "witty";
                else
                    return clientName;
            }
            set { clientName = value; }
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
        public TwitterNet(string username, SecureString password)
        {
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Authenticated constructor with Proxy.
        /// </summary>
        /// <remarks>
        /// JMF: Note that this constructor should not be necessary.  The default proxy
        /// (HttpWebRequest.DefaultWebProxy) needs to be set to the user-configured
        /// proxy setting during app startup so that WPF controls that access the 
        /// internet directly (like Image controls) can do so.
        /// </remarks>
        public TwitterNet(string username, SecureString password, IWebProxy webProxy)
        {
            this.username = username;
            this.password = password;
            this.webProxy = webProxy;
        }

        #endregion

        #region Public Methods

        #region Timeline Methods

        /// <summary>
        /// Retrieves the public timeline
        /// </summary>
        public TweetCollection GetPublicTimeline()
        {
            return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the public timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetPublicTimeline(string since)
        {
            return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline
        /// </summary>
        public TweetCollection GetFriendsTimeline()
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends);
            else
                return RetrieveTimeline(Timeline.Public);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetFriendsTimeline(string since)
        {
            if (!string.IsNullOrEmpty(username))
                return RetrieveTimeline(Timeline.Friends, since);
            else
                return RetrieveTimeline(Timeline.Public, since);
        }

        /// <summary>
        /// Retrieves the friends timeline. Narrows the result to after the since date.
        /// </summary>
        public TweetCollection GetFriendsTimeline(string since, string userId)
        {
            return RetrieveTimeline(Timeline.Friends, since, userId);
        }

        public TweetCollection GetUserTimeline(string userId)
        {
            return RetrieveTimeline(Timeline.User, "", userId);
        }

        public TweetCollection GetReplies()
        {
            return RetrieveTimeline(Timeline.Replies);
        }

        #endregion

        /// <summary>
        /// Returns the authenticated user's friends who have most recently updated, each with current status inline.
        /// </summary>
        public UserCollection GetFriends()
        {
            return GetFriends(CurrentlyLoggedInUser.Id);
        }

        /// <summary>
        /// Returns the user's friends who have most recently updated, each with current status inline.
        /// </summary>
        public UserCollection GetFriends(int userId)
        {
            UserCollection users = new UserCollection();

            // Twitter expects http://twitter.com/statuses/friends/12345.xml
            string requestURL = FriendsUrl + "/" + userId + Format;

            int friendsCount = 0;

            // Since the API docs state "Returns up to 100 of the authenticating user's friends", we need
            // to use the page param and to fetch ALL of the users friends. We can find out how many pages
            // we need by dividing the # of friends by 100 and rounding any remainder up.
            // merging the responses from each request may be tricky.
            if (currentLoggedInUser != null && currentLoggedInUser.Id == userId)
            {
                friendsCount = CurrentlyLoggedInUser.FollowingCount;
            }
            else
            {
                // need to make an extra call to twitter
                User user = GetUser(userId);
                friendsCount = user.FollowingCount;
            }

            int numberOfPagesToFetch = (friendsCount / 100) + 1;

            string pageRequestUrl = requestURL;

            for (int count = 1; count <= numberOfPagesToFetch; count++)
            {
                pageRequestUrl = requestURL + "?page=" + count;

                // Create the web request
                HttpWebRequest request = WebRequest.Create(pageRequestUrl) as HttpWebRequest;

                // Add credendtials to request  
                request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

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
                            User user = CreateUser(node);
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

                        switch ((int)httpResponse.StatusCode)
                        {
                            case 304: // 304 Not modified = no new tweets so ignore error.
                                break;

                            case 400: // rate limit exceeded
                                throw new RateLimitException("Rate limit exceeded. Clients may not make more than 70 requests per hour. Please try again in a few minutes.");

                            case 401: // unauthorized
                                throw new SecurityException("Not Authorized.");

                            case 407: // proxy authentication required
                                throw new ProxyAuthenticationRequiredException("Proxy authentication required.");

                            case 502: //Bad Gateway, Twitter is freaking out.
                                throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                            default:
                                throw;
                        }
                    }
                    else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                    {
                        throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
                    }

                }
            }
            return users;
        }

        public User GetUser(int userId)
        {
            User user = new User();

            // Twitter expects http://twitter.com/users/show/12345.xml
            string requestURL = UserShowUrl  + userId + Format;

            // Create the web request
            HttpWebRequest request = WebRequest.Create(requestURL) as HttpWebRequest;

            // Add credendtials to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                // Get statuses with XPath  
                XmlNode userNode = doc.SelectSingleNode("user");

                if (userNode != null)
                {
                    user = CreateUser(userNode);
                }
            }

            return user;
        }

        /// <summary>
        /// Delete a tweet from a users timeline
        /// </summary>
        /// <param name="id">id of the Tweet to delete</param>
        public void DestroyTweet(double id)
        {
            string urlToCall = string.Format("{0}{1:g}{2}", DestroyUrl, id, Format);
            MakeDestroyRequestCall(urlToCall);
        }

        /// <summary>
        /// Destroy a direct message sent by a user
        /// </summary>
        /// <param name="id">id of the direct message to delete</param>
        public void DestroyDirectMessage(double id)
        {
            string urlToCall = string.Format("{0}{1:g}{2}", DestroyDirectMessageUrl, id, Format);
            MakeDestroyRequestCall(urlToCall);
        }

        /// <summary>
        /// Post new tweet to Twitter
        /// </summary>
        /// <returns>newly added tweet</returns>
        public Tweet AddTweet(string text)
        {
            bool isDirectMessage = (text.StartsWith("d ", StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrEmpty(text))
                return null;

            text = HttpUtility.UrlEncode(text);

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(UpdateUrl + Format) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;

            // Add authentication to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            request.Method = "POST";

            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "status=" + text;
            string sourceParam = "&source=" + ClientName;
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

                // Load the response data into a XmlDocument  
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                XmlNode node = doc.SelectSingleNode("status");

                tweet = new Tweet();
                tweet.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                //Defect 43 - Twitter incorrectly returns last tweet sent when you direct message someone.
                if (isDirectMessage)
                    tweet.Text = HttpUtility.UrlDecode(text);
                else
                    tweet.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);

                string source = HttpUtility.HtmlDecode(node.SelectSingleNode("source").InnerText);
                // Remove html from the source string
                if (!string.IsNullOrEmpty(source))
                    tweet.Source = Regex.Replace(source, @"<(.|\n)*?>", string.Empty);

                string dateString = node.SelectSingleNode("created_at").InnerText;
                if (!string.IsNullOrEmpty(dateString))
                {
                    tweet.DateCreated = DateTime.ParseExact(
                        dateString,
                        twitterCreatedAtDateFormat,
                        CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                }
                tweet.IsNew = true;


                XmlNode userNode = node.SelectSingleNode("user");
                User user = CreateUser(userNode); 
                tweet.User = user;
            }

            return tweet;
        }

        protected string GetPropertyFromXml(XmlNode twitterNode, string propertyName)
        {
            if (twitterNode != null)
            {
                XmlNode propertyNode = twitterNode.SelectSingleNode(propertyName);
                if (propertyNode != null)
                {
                    return propertyNode.InnerText;
                }
            }
            return String.Empty;
        }

        private User CreateUser(XmlNode userNode)
        {
            User user = new User();

            if (userNode != null)
            {
                int temp = -1;
                int.TryParse(GetPropertyFromXml(userNode, "id"), out temp);
                user.Id = temp;

                user.Name = GetPropertyFromXml(userNode, "name");
                user.ScreenName = GetPropertyFromXml(userNode, "screen_name");
                user.ImageUrl = GetPropertyFromXml(userNode, "profile_image_url");
                user.SiteUrl = GetPropertyFromXml(userNode, "url");
                user.Location = GetPropertyFromXml(userNode, "location");
                user.Description = GetPropertyFromXml(userNode, "description");
                user.BackgroundColor = GetPropertyFromXml(userNode, "profile_background_color");
                user.TextColor = GetPropertyFromXml(userNode, "profile_text_color");
                user.LinkColor = GetPropertyFromXml(userNode, "profile_link_color");
                user.SidebarBorderColor = GetPropertyFromXml(userNode, "profile_sidebar_border_color");
                user.SidebarFillColor = GetPropertyFromXml(userNode, "profile_sidebar_fill_color");

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "friends_count"), out temp);
                user.FollowingCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "favourites_count"), out temp);
                user.FavoritesCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "statuses_count"), out temp);
                user.StatusesCount = temp;

                temp = 0;
                int.TryParse(GetPropertyFromXml(userNode, "followers_count"), out temp);
                user.FollowersCount = temp;
            }

            return user;
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
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNode userNode = doc.SelectSingleNode("user");

                    if (userNode != null)
                    {
                        user = CreateUser(userNode);
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

                    switch ((int)httpResponse.StatusCode)
                    {
                        case 400: // rate limit exceeded
                            throw new RateLimitException("Rate limit exceeded. Clients may not make more than 70 requests per hour. Please try again in a few minutes.");

                        case 401: // unauthorized
                            return null;

                        case 407: // proxy authentication required
                            throw new ProxyAuthenticationRequiredException("Proxy authentication required.");

                        case 502 : //Bad Gateway, Twitter is freaking out.
                            throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                        default:
                            throw;
                    }
                }
                else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                {
                    throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
                }

                else
                    throw;
            }
            currentLoggedInUser = user;
            return user;
        }

        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("WittyPasswordSalt");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        /// <summary>
        /// Gets direct messages for the user
        /// </summary>
        /// <returns>Collection of direct messages</returns>
        public DirectMessageCollection RetrieveMessages()
        {
            DirectMessageCollection messages = new DirectMessageCollection();

            // Create the web request
            HttpWebRequest request = WebRequest.Create(DirectMessagesUrl + Format) as HttpWebRequest;

            // Add credendtials to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            try
            {
                // Get the Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    // Get statuses with XPath  
                    XmlNodeList nodes = doc.SelectNodes("/direct-messages/direct_message");

                    foreach (XmlNode node in nodes)
                    {
                        DirectMessage message = new DirectMessage();
                        message.Id = double.Parse(node.SelectSingleNode("id").InnerText);
                        message.Text = HttpUtility.HtmlDecode(node.SelectSingleNode("text").InnerText);

                        string dateString = node.SelectSingleNode("created_at").InnerText;
                        if (!string.IsNullOrEmpty(dateString))
                        {
                            message.DateCreated = DateTime.ParseExact(
                                dateString,
                                twitterCreatedAtDateFormat,
                                CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                        }

                        XmlNode senderNode = node.SelectSingleNode("sender");
                        User sender = CreateUser(senderNode);
                        message.Sender = sender;

                        XmlNode recipientNode = node.SelectSingleNode("recipient");
                        User recipient = CreateUser(recipientNode);
                        message.Recipient = recipient;

                        messages.Add(message);
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

                    switch ((int)httpResponse.StatusCode)
                    {
                        case 400: // rate limit exceeded
                            throw new RateLimitException("Rate limit exceeded. Clients may not make more than 70 requests per hour. Please try again in a few minutes.");

                        case 401: // unauthorized
                            throw new SecurityException("Not Authorized.");

                        case 502: //Bad Gateway, Twitter is freaking out.
                            throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                        default:
                            throw;
                    }
                }
                else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                {
                    throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home." );
                }

            }
            return messages;
        }

        public void SendMessage(string user, string text)
        {
            // Jason Follas: Make sure that the user isn't trying to DM themselves.
            if (String.Compare(user, CurrentlyLoggedInUser.ScreenName, true) == 0)
                return;

            if (string.IsNullOrEmpty(text))
                return;

            text = HttpUtility.UrlEncode(text);

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(SendMessageUrl + Format) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;

            // Add authentication to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            request.Method = "POST";

            // Set values for the request back
            request.ContentType = "application/x-www-form-urlencoded";
            string param = "text=" + text;
            string userParam = "&user=" + user;
            request.ContentLength = param.Length + userParam.Length;

            // Write the request paramater
            StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(param);
            stOut.Write(userParam);
            stOut.Close();

            try
            {
                // Perform the web request
                request.GetResponse();
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

                    switch ((int)httpResponse.StatusCode)
                    {
                        case 401: // unauthorized
                            throw new SecurityException("Not Authorized.");

                        case 502: //Bad Gateway, Twitter is freaking out.
                            throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                        default:
                            throw;
                    }
                }
                else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                {
                    throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
                }

            }

        }

        /// <summary>
        /// Follow the user specified by userId
        /// </summary>
        /// <param name="userId"></param>
        public void FollowUser(string userName)
        {
            // Jason Follas: Make sure that the user isn't trying to follow themselves.
            if (String.Compare(userName, CurrentlyLoggedInUser.ScreenName, true) == 0)
                return;

            string followUrl = CreateFriendshipUrl + userName + Format;
            MakeTwitterApiCall(followUrl, "POST");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline)
        {
            return RetrieveTimeline(timeline, string.Empty);
        }

        /// <summary>
        /// Retrieves the specified timeline from Twitter
        /// </summary>
        /// <returns>Collection of Tweets</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline, string since)
        {
            return RetrieveTimeline(timeline, since, string.Empty);
        }

        /// <summary>
        /// The Main function for interfacing with the Twitter API
        /// </summary>
        /// <returns>Collection of Tweets. Twitter limits the max to 20.</returns>
        private TweetCollection RetrieveTimeline(Timeline timeline, string since, string userId)
        {
            TweetCollection tweets = new TweetCollection();

            string timelineUrl;

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
                DateTime sinceDate;
                DateTime.TryParse(since, out sinceDate);

                // Go back a minute to compensate for latency.
                sinceDate = sinceDate.AddMinutes(-1);
                string sinceDateString = sinceDate.ToString(twitterSinceDateFormat);
                timelineUrl = timelineUrl + "?since=" + sinceDateString;
            }

            // Create the web request
            HttpWebRequest request = WebRequest.Create(timelineUrl) as HttpWebRequest;

            // Add configured web proxy
            request.Proxy = webProxy;

            // Begin JMF Edits: Authenticate for all timelines so you can get protected tweets
            //if (timeline == Timeline.Friends || timeline == Timeline.Replies)

            // Add credentials to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // End JMF Edits



            // moved this out of the try catch to use it later on in the XMLException
            // trying to fix a bug someone report
            XmlDocument doc = new XmlDocument();
            try
            {
                // Get the Web Response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Load the response data into a XmlDocument  
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
                                CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"), DateTimeStyles.AllowWhiteSpaces);
                        }

                        XmlNode userNode = node.SelectSingleNode("user");
                        User user = CreateUser(userNode);
                        tweet.User = user;
                        tweet.Timeline = timeline;
                        tweet.IsReply = IsReplyTweet(this.CurrentlyLoggedInUser.ScreenName, tweet);

                        tweets.Add(tweet);
                    }

                    tweets.SaveToDisk();
                }
            }
            catch (XmlException exXML)
            {
                // adding the XML document data to the exception so it will get logged
                // so we can debug the issue
                exXML.Data.Add("XMLDoc", doc);
                throw;
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

                    switch ((int)httpResponse.StatusCode)
                    {
                        case 304:  // 304 Not modified = no new tweets so ignore error.
                            break;

                        case 400: // rate limit exceeded
                            throw new RateLimitException("Rate limit exceeded. Clients may not make more than 70 requests per hour. Please try again in a few minutes.");

                        case 404: // Not Found = specified user does not exist
                            if (timeline == Timeline.User)
                                throw new UserNotFoundException(userId, "@" + userId + " does not exist (probably mispelled)");
                            else // what if a 404 happens to occur in another scenario?
                                throw;

                        case 407: // proxy authentication required
                            throw new ProxyAuthenticationRequiredException("Proxy authentication required.");

                        case 401: // unauthorized, but is this because the user's password
                                  // is wrong, or because user is trying to access protected
                                  // tweets...  Since we got this far, I'm going to assume
                                  // that the password is correct.
                            throw new SecurityException("Not Authorized.", webExcp);

                        case 502: //Bad Gateway, Twitter is freaking out.
                            throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                        default:
                            throw;
                    }
                }
                else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                {
                    throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
                }
            }
            return tweets;
        }

        private bool IsReplyTweet(string userId, Tweet tweet)
        {
            if (tweet.Timeline == Timeline.Replies)
                return true;

            if (tweet.Timeline == Timeline.Friends)
            {
                if (tweet.Text.Contains("@" + userId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Generic call to destroy a status ** still in progress **
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeDestroyRequestCall(string urlToCall)
        {
            MakeTwitterApiCall(urlToCall, "POST");
        }

        /// <summary>
        /// Default Twitter API call uses GET
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeTwitterApiCall(string urlToCall)
        {
            MakeTwitterApiCall(urlToCall, "GET");
        }

        /// <summary>
        /// Generic Twitter API call. Use this when you don't need/want to parse the return message
        ///  and just want to make a succeed/fail call to the API.
        /// </summary>
        /// <param name="urlToCall"></param>
        private void MakeTwitterApiCall(string urlToCall, string method)
        {
            //REMARK: We may want to refactor this to return the message returned by the API.
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(urlToCall) as HttpWebRequest;
            request.ServicePoint.Expect100Continue = false;

            // Add authentication to request  
            request.Credentials = new NetworkCredential(username, TwitterNet.ToInsecureString(password));

            // Add configured web proxy
            request.Proxy = webProxy;

            request.Method = method;

            try
            {
                // perform the destroy web request
                request.GetResponse();
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

                    switch ((int)httpResponse.StatusCode)
                    {
                        case 401: // unauthorized
                            throw new SecurityException("Not Authorized.", webExcp);

                        case 502: //Bad Gateway, Twitter is freaking out.
                            throw new BadGatewayException("Fail Whale!  There was a problem calling the Twitter API.  Please try again in a few minutes.");

                        default:
                            throw;
                    }
                }
                else if (status == WebExceptionStatus.ProxyNameResolutionFailure)
                {
                    throw new ProxyNotFoundException("The proxy server could not be found.  Check that it was entered correctly in the Options dialog.  You may need to disable your web proxy in the Options, if for instance you use a proxy server at work and are now at home.");
                }

            }
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
        Replies,
        DirectMessages
    }
}
