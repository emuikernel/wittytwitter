using NUnit.Framework;
using System;
using TwitterLib;

namespace WittyUnitTests
{
    [TestFixture]
    public class TwitterNetTest
    {
        protected string username;
        protected string password;
        protected int userId;
        protected int friendsCount;

        protected int alanUserId;
        protected int alanFriendsCount;

        [SetUp]
        public void Init()
        {
            username = "WittyTest";
            password = "WittyTest";
            userId = 14203624; // WittyTest's Twitter id

            alanUserId = 758185;
            alanFriendsCount = 217;

            friendsCount = 1;
        }
        
        [Test]
        public void Login()
        {
            TwitterNet twitter = new TwitterNet(username, password);
            User user = twitter.Login();
            Assert.AreEqual(userId, user.Id);
        }

        [Test]
        public void GetFriends()
        {
            TwitterNet twitter = new TwitterNet(username, password);
            UserCollection uc = twitter.GetFriends();
            Assert.AreEqual(friendsCount, uc.Count);
        }

        [Test]
        public void GetFriendsUnauthenticated()
        {
            TwitterNet twitter = new TwitterNet();
            UserCollection uc = twitter.GetFriends(userId);
            Assert.AreEqual(friendsCount, uc.Count);
        }

        /// <summary>
        /// For testing GetFriends above 100.
        /// </summary>
        [Test]
        public void GetAlanFriendsUnauthenticated()
        {
            TwitterNet twitter = new TwitterNet();
            UserCollection uc = twitter.GetFriends(alanUserId);
            Assert.AreEqual(alanFriendsCount, uc.Count); 
        }

        [Test]
        public void GetUser()
        {
            TwitterNet twitter = new TwitterNet(username,password);
            User user = twitter.GetUser(userId);
            Assert.AreEqual(username, user.ScreenName);
        }
    }
}
