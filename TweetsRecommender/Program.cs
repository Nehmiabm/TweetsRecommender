using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace TweetsRecommender
{
    class Program
    {
        static  void Main(string[] args)
        {

            // Pass your credentials to the service
            string consumerKey = "BVCKULB1I0tv8zIlxuk1jSRCg";
            string consumerSecret = "owNQNPAosiQm74r3eS01iLm8o9zBbtRpkUCU4ml1tCZ5nvEsiu";

            string accessToken = "358148844-jCqI1BarGkJTRcrN4Otp39E9qFHNMeGs8IUW3GDv";
            string accessTokenSecret = "auBPaVFwPpkB0JG2WlsfHoUJHcn4V3obyJher9lFXUWfa";
            TwitterService service = new TwitterService(consumerKey, consumerSecret);

          
            service.AuthenticateWith(accessToken, accessTokenSecret);

            var result =
                service.Search(new SearchOptions()
                {
                    Count = 1000,
                    Geocode =
                        new TwitterGeoLocationSearch()
                        {
                            Radius = 3,
                            Coordinates =
                                new TwitterGeoLocation.GeoCoordinates() {Latitude = 41.00688, Longitude = -91.967137}
                        }
                });
            foreach (TwitterStatus status in result.Statuses)
            {
            
                Console.WriteLine("==========================================================================");
                Console.WriteLine("Author Screen Name:"+status.Author.ScreenName);
                Console.WriteLine("Created Date:"+status.CreatedDate.ToLongDateString());
                Console.WriteLine("Text:"+status.Text);
            }
            Console.ReadKey();
          
        }
    }
}
