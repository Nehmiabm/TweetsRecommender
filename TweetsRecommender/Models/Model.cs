using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetsRecommender.Models
{
    public class user
    {
        public long _id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }

        public string description { get; set; }
        public DateTime createdDate { get; set; }
        public int friendsCount { get; set; }
        public int followersCount { get; set; }
        public string location { get; set; }
    }

    public class twittedPlace
    {
        public string _id { get; set; }
        public string fullName { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
    }

    public class TweetInfo
    {
        public long _id { get; set; }
        public string screenName { get; set; }
        public string text { get; set; }
        public DateTime createdDate { get; set; }
        public string inReplyToScreenName { get; set; }
        public int retweetCount { get; set; }
        public string language { get; set; }
        public int favoriteCount { get; set; }
        public user user { get; set; }
        public twittedPlace twittedPlace { get; set; }
    }

    public class Sale
    {
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public string ProductId { get; set; }
        public string StoreId { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
    }
}
