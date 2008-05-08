using System;
namespace TwitterLib
{
    public interface ITwitterNet
    {
        Tweet AddTweet(string text);

        string ClientName { get; set; }
        string CreateFriendshipUrl { get; set; }
        string SendMessageUrl { get; set; }
        string UpdateUrl { get; set; }
        string UserName { get; }
        string UserShowUrl { get; set; }
        string UserTimelineUrl { get; set; }
        string DestroyDirectMessageUrl { get; set; }
        string DestroyUrl { get; set; }
        string DirectMessagesUrl { get; set; }
        string FollowersUrl { get; set; }
        string FriendsTimelineUrl { get; set; }
        string FriendsUrl { get; set; }
        string PublicTimelineUrl { get; set; }
        string RepliesTimelineUrl { get; set; }
        string Format { get; set; }

        void DestroyDirectMessage(double id);
        void DestroyTweet(double id);
        void FollowUser(string userName);
        void SendMessage(string user, string text);

        UserCollection GetFriends(int userId);
        UserCollection GetFriends();
        User CurrentlyLoggedInUser { get; set; }
        User GetUser(int userId);
        User Login();

        TweetCollection GetFriendsTimeline();
        TweetCollection GetFriendsTimeline(string since, string userId);
        TweetCollection GetFriendsTimeline(string since);
        TweetCollection GetPublicTimeline(string since);
        TweetCollection GetPublicTimeline();
        TweetCollection GetReplies();
        TweetCollection GetUserTimeline(string userId);
        DirectMessageCollection RetrieveMessages();

        System.Security.SecureString Password { get; set; }
        System.Net.IWebProxy WebProxy { get; set; }
    }
}
