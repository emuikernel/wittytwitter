using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TwitterLib;

namespace WittyUnitTests
{
    [TestFixture]
    public class TweetTest
    {
        protected int count;

        [SetUp]
        public void Init()
        {
            count = 10;
        }

        [Test]
        public void Truncate()
        {
            TweetCollection tweets = new TweetCollection();

            for (int i = 0; i < 20; i++)
            {
                Tweet tweet = new Tweet();
                tweet.Id = i;
                tweets.Insert(0, tweet);
            }

            tweets.Truncate(count);

            Assert.AreEqual(count, tweets.Count);
            Assert.AreEqual(tweets[0].Id, 19);
        }
    }
}
