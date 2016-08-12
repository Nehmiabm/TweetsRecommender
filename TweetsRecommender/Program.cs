using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;
using TweetSharp;

namespace TweetsRecommender
{
    class Program
    {
        static  void Main(string[] args)
        {
           
          GenerateTweets();
         // ClassifyTest();
        }

        private static void GenerateTweets()
        {
            string consumerKey = "BVCKULB1I0tv8zIlxuk1jSRCg";
            string consumerSecret = "owNQNPAosiQm74r3eS01iLm8o9zBbtRpkUCU4ml1tCZ5nvEsiu";

            string accessToken = "358148844-jCqI1BarGkJTRcrN4Otp39E9qFHNMeGs8IUW3GDv";
            string accessTokenSecret = "auBPaVFwPpkB0JG2WlsfHoUJHcn4V3obyJher9lFXUWfa";
            TwitterService service = new TwitterService(consumerKey, consumerSecret);
            StringBuilder sb = new StringBuilder();

            service.AuthenticateWith(accessToken, accessTokenSecret);

            var result =
                service.Search(new SearchOptions()
                {
                    Count = 100, //Number of tweets
                    Q = "#BlackLives"
                    //Geocode =
                    //    new TwitterGeoLocationSearch()
                    //    {
                    //        Radius = 1,
                    //        Coordinates =
                    //            new TwitterGeoLocation.GeoCoordinates()
                    //            {Latitude = 41.00688, Longitude = -91.967137} //Search by geo location for Fairfield, IA
                    //    }
                });
            foreach (TwitterStatus status in result.Statuses)
            {
                var place = status.Place;
                string country = place == null ? "" : place.Country;
                string address = place == null ? "" : place.FullName;
                Console.WriteLine("==========================================================================");
                sb.AppendLine("==========================================================================");
                Console.WriteLine("Author Screen Name:" + status.Author.ScreenName);
                sb.AppendLine("Author Screen Name:" + status.Author.ScreenName);
                Console.WriteLine("Created Date:" + status.CreatedDate.ToLongDateString());
                sb.AppendLine("Created Date:" + status.CreatedDate.ToLongDateString());
                Console.WriteLine("Text:" + status.Text);
                sb.AppendLine("Text:" + status.Text);
                Console.WriteLine("Id:" + status.Id);
                sb.AppendLine("Id:" + status.Id);
                //Console.WriteLine("Favorite Count:" + status.FavoriteCount);
                //sb.AppendLine("Favorite Count:" + status.FavoriteCount);
                Console.WriteLine("ID String:" + status.IdStr);
                sb.AppendLine("ID String:" + status.IdStr);
                Console.WriteLine("In Reply To Screen Name:" + status.InReplyToScreenName);
                sb.AppendLine("In Reply To Screen Name:" + status.InReplyToScreenName);
                //Console.WriteLine("Is Retweeted:" + status.IsRetweeted);
                //sb.AppendLine("Is Retweeted:" + status.IsRetweeted);
                Console.WriteLine("Is Favorited:" + status.IsFavorited);
                sb.AppendLine("Is Favorited:" + status.IsFavorited);
                Console.WriteLine("Language:" + status.Language);
                sb.AppendLine("Language:" + status.Language);
                Console.WriteLine("Retweet Count:" + status.RetweetCount);
                sb.AppendLine("Retweet Count:" + status.RetweetCount);
                Console.WriteLine("Raw Source:" + status.RawSource);
                sb.AppendLine("Raw Source:" + status.RawSource);
                Console.WriteLine("Source:" + status.Source);
                sb.AppendLine("Source:" + status.Source);
                Console.WriteLine("Country:" + country + ", Address: " + address);
                sb.AppendLine("Country:" + country + ", Address: " + address);
                Console.WriteLine("User Name:" + status.User.Name);
                sb.AppendLine("User Name:" + status.User.Name);
                //Console.WriteLine("User Email:" + status.User.Email);
                //sb.AppendLine("User Email:" + status.User.Email);
                Console.WriteLine("User Description:" + status.User.Description);
                sb.AppendLine("User Description:" + status.User.Description);
                Console.WriteLine("User Followers Count:" + status.User.FollowersCount);
                sb.AppendLine("User Followers Count:" + status.User.FollowersCount);
                Console.WriteLine("User Created Date:" + status.User.CreatedDate.ToLongDateString());
                sb.AppendLine("User Created Date:" + status.User.CreatedDate.ToLongDateString());
                Console.WriteLine("User Friends Count:" + status.User.FriendsCount);
                sb.AppendLine("User Friends Count:" + status.User.FriendsCount);
                Console.WriteLine("User Id:" + status.User.Id);
                sb.AppendLine("User Id:" + status.User.Id);
                Console.WriteLine("User Location:" + status.User.Location);
                sb.AppendLine("User Location:" + status.User.Location);
            }
            if (!System.IO.Directory.Exists(@"C\bigdata"))
                System.IO.Directory.CreateDirectory(@"C:\bigdata");
            System.IO.File.WriteAllText(@"C:\bigdata\bigdata.txt", sb.ToString());
            Console.ReadKey();
        }

     
        public static void ClassifyTest()
        {
            const int percentSplit = 66;
            try
            {
                weka.core.Instances insts = new weka.core.Instances(new java.io.FileReader("iris.arff"));
                insts.setClassIndex(insts.numAttributes() - 1);

                weka.classifiers.Classifier cl = new weka.classifiers.trees.J48();
                Console.WriteLine("Performing " + percentSplit + "% split evaluation.");
               
                //randomize the order of the instances in the dataset.
                weka.filters.Filter myRandom = new weka.filters.unsupervised.instance.Randomize();
                myRandom.setInputFormat(insts);
                insts = weka.filters.Filter.useFilter(insts, myRandom);

                int trainSize = insts.numInstances() * percentSplit / 100;
                int testSize = insts.numInstances() - trainSize;

                Console.WriteLine("Scheme: "+cl.getClass());
                Console.WriteLine("Relation: iris ");
                Console.WriteLine("Instances: " + insts.numInstances());
                Console.WriteLine("Attributes: " + insts.numAttributes());
                Console.WriteLine("Train data instance: "+trainSize);
                Console.WriteLine("Test data instance: "+testSize);
                weka.core.Instances train = new weka.core.Instances(insts, 0, trainSize);

                cl.buildClassifier(train);
                int numCorrect = 0;
                for (int i = trainSize; i < insts.numInstances(); i++)
                {
                    weka.core.Instance currentInst = insts.instance(i);
                    double predictedClass = cl.classifyInstance(currentInst);
                    if (predictedClass == insts.instance(i).classValue())
                        numCorrect++;
                }
                Console.WriteLine(numCorrect + " out of " + testSize + " correct (" +
                           (double)((double)numCorrect / (double)testSize * 100.0) + "%)");
                Console.ReadKey();
            }
            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }
        }

    }
}

