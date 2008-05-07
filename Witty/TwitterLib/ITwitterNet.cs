using System;
namespace TwitterLib
{
    interface ITwitterNet
    {
        Tweet AddTweet(string text);
        string ClientName { get; set; }
        string CreateFriendshipUrl { get; set; }
        User CurrentlyLoggedInUser { get; set; }
        void DestroyDirectMessage(double id);
        string DestroyDirectMessageUrl { get; set; }
        void DestroyTweet(double id);
        string DestroyUrl { get; set; }
        string DirectMessagesUrl { get; set; }
        string FollowersUrl { get; set; }
        void FollowUser(string userName);
        string Format { get; set; }
        string FriendsTimelineUrl { get; set; }
        string FriendsUrl { get; set; }
        UserCollection GetFriends(int userId);
        UserCollection GetFriends();
        TweetCollection GetFriendsTimeline();
        TweetCollection GetFriendsTimeline(string since, string userId);
        TweetCollection GetFriendsTimeline(string since);
        TweetCollection GetPublicTimeline(string since);
        TweetCollection GetPublicTimeline();
        TweetCollection GetReplies();
        User GetUser(int userId);
        TweetCollection GetUserTimeline(string userId);
        User Login();
        System.Security.SecureString Password { get; set; }
        string PublicTimelineUrl { get; set; }
        string RepliesTimelineUrl { get; set; }
        DirectMessageCollection RetrieveMessages();
        void SendMessage(string user, string text);
        string SendMessageUrl { get; set; }
        string UpdateUrl { get; set; }
        string UserName { get; }
        string UserShowUrl { get; set; }
        string UserTimelineUrl { get; set; }
        System.Net.IWebProxy WebProxy { get; set; }
    }
}
