using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ikvm.extensions;
using MongoDB.Bson;
using TweetSharp;
using OpenNLP.Tools;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.Tokenize;
using TweetsRecommender.Models;
using weka.core;

namespace TweetsRecommender
{
    class Program
    {

        #region Noun Constants

        private const string NN = "NN"; //Noun, singular or mass 
        private const string NNP = "NNP"; //Proper noun, singular
        private const string NNPS = "NNPS"; //Proper noun, plural
        private const string NNS = "NNS"; //Noun, plural

        #endregion

        #region Verb Constants

        private const string VB = "VB"; //Verb, base form
        private const string VBD = "VBD"; //Verb, past tense
        private const string VBG = "VBG"; //Verb, gerund/present participle
        private const string VBN = "VBN"; //Verb, past participle
        private const string VBP = "VBP"; //Verb, non-3rd ps. sing. present
        private const string VBZ = "VBZ"; //Verb, 3rd ps. sing. present

        #endregion

        private static StringBuilder tagBuilder = new StringBuilder();
        private static string modelPath = ConfigurationManager.AppSettings["ModelPath"];
        private static MongoConnector _mongoConnector;

        private static string consumerKey = "BVCKULB1I0tv8zIlxuk1jSRCg";
        private static string consumerSecret = "owNQNPAosiQm74r3eS01iLm8o9zBbtRpkUCU4ml1tCZ5nvEsiu";

        private static string accessToken = "358148844-jCqI1BarGkJTRcrN4Otp39E9qFHNMeGs8IUW3GDv";
        private static string accessTokenSecret = "auBPaVFwPpkB0JG2WlsfHoUJHcn4V3obyJher9lFXUWfa";
        static void Main(string[] args)
        {
            _mongoConnector = new MongoConnector();
           // ProcessQuery();
            //GenerateTweetsAndStore();
            GenerateTweetsAndTag();
            // AnalyzeTweetUsingPosTest("@mellie_me12009: The more you know...She was NOT crazy...");
            // ClassifyTest();
            // AnalyzeTweetUsingPos("When I typed in the first part of the hashtag, #blacklives, this was #Twitter's first and only suggestion");
        }

        private static void GenerateTweetsAndStore()
        {
            TwitterService service = new TwitterService(consumerKey, consumerSecret);
            StringBuilder sb = new StringBuilder();

            service.AuthenticateWith(accessToken, accessTokenSecret);

            var result =
                service.Search(new SearchOptions()
                {
                    Count = 50, //Number of tweets
                                //Q = "donald" 47.6062° N, 122.3321° W
                    Geocode =
                        new TwitterGeoLocationSearch()
                        {
                            Radius = 1,
                            Coordinates =
                                new TwitterGeoLocation.GeoCoordinates()
                                { Latitude = 47.6062, Longitude = -122.3321 } //Search by geo location for Fairfield, IA
                        }
                });

            if (result.Statuses.Any())
                _mongoConnector.DeleteAllTweetsAsync(); //Delete all tweet documents


            foreach (TwitterStatus status in result.Statuses)
            {
                if (status.Language.Equals("en"))
                {
                    var document = new BsonDocument();
                    TweetInfo tweetInfo = new TweetInfo();
                    var place = status.Place;
                    string country = place == null ? "" : place.Country;
                    string address = place == null ? "" : place.FullName;
                    Console.WriteLine("==========================================================================");
                    sb.AppendLine("==========================================================================");
                    tweetInfo.screenName = status.Author.ScreenName;
                    tweetInfo.createdDate = status.CreatedDate;
                    tweetInfo.text = status.Text;
                    tweetInfo._id = status.Id;
                    tweetInfo.favoriteCount = status.FavoriteCount;
                    tweetInfo.inReplyToScreenName = status.InReplyToScreenName;
                    tweetInfo.retweetCount = status.RetweetCount;
                    tweetInfo.language = status.Language;
                    user user = new user
                    {
                        createdDate = status.User.CreatedDate,
                        description = status.User.Description,
                        email = status.User.Email,
                        followersCount = status.User.FollowersCount,
                        friendsCount = status.User.FriendsCount,
                        location = status.User.Location,
                        userName = status.User.Name,
                        _id = status.User.Id
                    };
                    twittedPlace tweetPlace = null;
                    if (place != null)
                    {
                        tweetPlace = new twittedPlace
                        {
                          _id = place.Id,
                          fullName = place.FullName,
                          country = place.Country,
                          countryCode = place.CountryCode
                        };
                    }
                    tweetInfo.user = user;
                    tweetInfo.twittedPlace = tweetPlace;
                    string json = tweetInfo.ToJson();
                    document.AddRange(BsonDocument.Parse(json));
                    _mongoConnector.InsertDocumentAsync(document);
                    Console.WriteLine("Tweet with id: "+tweetInfo._id+" successfully stored!");
                    
                }
            }
            if (!System.IO.Directory.Exists(@"C\bigdata"))
                System.IO.Directory.CreateDirectory(@"C:\bigdata");
            System.IO.File.WriteAllText(@"C:\bigdata\bigdata.txt", sb.ToString());
            System.IO.File.WriteAllText(@"C:\bigdata\tagslist.txt", tagBuilder.toString(),Encoding.UTF8);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void GenerateTweetsAndTag()
        {
          
            TwitterService service = new TwitterService(consumerKey, consumerSecret);
            StringBuilder sb = new StringBuilder();

            service.AuthenticateWith(accessToken, accessTokenSecret);

            var result =
                service.Search(new SearchOptions()
                {
                    //Count = 99, //Number of tweets
                    //Q = "#trump" 
                     
                    Geocode =
                        new TwitterGeoLocationSearch()
                        {
                            Radius = 50,
                            Coordinates =
                                new TwitterGeoLocation.GeoCoordinates()
                                { Latitude = 38.5816, Longitude = -121.4944 } //Search by geo location for Fairfield, IA
                        }
                });

            
            foreach (TwitterStatus status in result.Statuses)
            {
                if (status.Language.Equals("en"))
                {
                    AnalyzeTweetUsingPos(status.Text);
                    Console.WriteLine("Tweet Id:"+status.Id+" tagged successfully");
                }
            }
            if (!System.IO.Directory.Exists(@"C\bigdata"))
                System.IO.Directory.CreateDirectory(@"C:\bigdata");
            System.IO.File.WriteAllText(@"C:\bigdata\tagslist.txt", tagBuilder.toString(), Encoding.UTF8);
            Console.WriteLine("Press any key to exit...");
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

                int trainSize = insts.numInstances()*percentSplit/100;
                int testSize = insts.numInstances() - trainSize;

                Console.WriteLine("Scheme: " + cl.getClass());
                Console.WriteLine("Relation: iris ");
                Console.WriteLine("Instances: " + insts.numInstances());
                Console.WriteLine("Attributes: " + insts.numAttributes());
                Console.WriteLine("Train data instance: " + trainSize);
                Console.WriteLine("Test data instance: " + testSize);
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
                                  (double) ((double) numCorrect/(double) testSize*100.0) + "%)");
                Console.ReadKey();
            }
            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }
        }

        private static void AnalyzeTweetUsingPos(string tweet, TagType tagType = TagType.Noun)
        {
            try
            {

                //Tokenize tweet

                EnglishMaximumEntropyTokenizer tokenizer =
                    new EnglishMaximumEntropyTokenizer(modelPath + "EnglishTok.nbin");
                string[] tokens = tokenizer.Tokenize(tweet);
               var filteredTokens= FilterNoises(tokens);
                //Tag tokens
                EnglishMaximumEntropyPosTagger posTagger =
                    new EnglishMaximumEntropyPosTagger(modelPath + "EnglishPOS.nbin");
                string[] tags = posTagger.Tag(filteredTokens);
                //Console.WriteLine(tokens.Length + " tokens found.");
                //Console.WriteLine("Building tags. Please wait this might take a while...");

                int i = 0;
                // int countTagged = 0;
                foreach (string tag in tags)
                {
                    if (tagType == TagType.Noun) //tag noun
                    {
                        if (tag.Equals(NN) || tag.Equals(NNS)) // || tag.Equals(NNP) || tag.Equals(NNPS)
                        {
                            tagBuilder.AppendLine(filteredTokens[i]);
                            // countTagged++;
                        }
                    }
                    else //tag verb
                    {
                        if (tag.Equals(VB) || tag.Equals(VBD) || tag.Equals(VBG) || tag.Equals(VBN)
                            || tag.Equals(VBP) || tag.Equals(VBZ))
                        {
                            tagBuilder.AppendLine(filteredTokens[i]);
                            // countTagged++;
                        }
                    }
                    i++;

                }
                //Console.WriteLine("Building tags done.");
                //Console.WriteLine("Writing tags to file..");
                //write tags to file
                //if (!System.IO.Directory.Exists(@"C:\bigdata"))
                //    System.IO.Directory.CreateDirectory(@"C\bigdata");
                //System.IO.File.WriteAllText(@"C:\bigdata\tagslist.txt", sb.toString());
                //Console.WriteLine(countTagged + " Tags successfully Written to file!");
                //Console.Read();
            }


            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.Read();
            }

        }

        private static string[] FilterNoises(string[] tokens)
        {
            List<string> filtered=new List<string>();
            foreach (String tag in tokens)
            {
                if (!tag.Contains("http") && !tag.Contains("@") && !tag.Contains("#"))
                    filtered.Add(tag);
            }
            return filtered.ToArray();
        }

        private static void ProcessQuery()
        {
            if (!System.IO.File.Exists(@"C:\bigdata\Sale.csv"))
            {
                Console.WriteLine("No file found to process query!");
                Console.ReadKey();
            }
            else
            {
                List<Sale> saleList = new List<Sale>();
                string line;
                int lineCount = 0;
                string path = @"C:\bigdata\Sale.csv";
                System.IO.StreamReader file =
                    new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    //System.Console.WriteLine(line);
                    if (lineCount > 0)
                    {
                        string[] cols = line.split(",");
                        var sale = new Sale
                        {
                            OrderId = int.Parse(cols[0]),
                            Date = DateTime.Parse(cols[1]),
                            CustomerId = int.Parse(cols[2]),
                            ProductId = cols[3],
                            StoreId = cols[4],
                            Quantity = double.Parse(cols[5]),
                            Amount = double.Parse(cols[6])
                        };
                        saleList.Add(sale);
                    }
                    lineCount++;
                }

                //perform group by query on storeid and produtid for sum of quantity
                var result = saleList
                    .GroupBy(sl => new
                    {
                        sl.StoreId,
                        sl.ProductId,
                    })
                    .Select(g =>
                        new
                        {
                            sId = g.Key.StoreId,
                            pId = g.Key.ProductId,
                            qty = g.Sum(x => Math.Round(Convert.ToDecimal(x.Quantity), 2)),
                        }
                    );
                   
            }
        }


        private static void AnalyzeTweetUsingPosTest(string tweet, TagType tagType = TagType.Noun)
        {
            try
            {

                //Tokenize tweet

                EnglishMaximumEntropyTokenizer tokenizer =
                    new EnglishMaximumEntropyTokenizer(modelPath + "EnglishTok.nbin");
                string[] tokens = tokenizer.Tokenize(tweet);

                //Tag tokens
                EnglishMaximumEntropyPosTagger posTagger =
                    new EnglishMaximumEntropyPosTagger(modelPath + "EnglishPOS.nbin");
                string[] tags = posTagger.Tag(tokens);
                Console.WriteLine(tokens.Length + " tokens found.");
                Console.WriteLine("Building tags. Please wait this might take a while...");

                int i = 0;
                int countTagged = 0;
                foreach (string tag in tags)
                {
                    if (tagType == TagType.Noun) //tag noun
                    {
                        if (tag.Equals(NN) || tag.Equals(NNP) || tag.Equals(NNPS) || tag.Equals(NNS))
                        {
                            tagBuilder.AppendLine(tokens[i]);
                            countTagged++;
                        }
                    }
                    else //tag verb
                    {
                        if (tag.Equals(VB) || tag.Equals(VBD) || tag.Equals(VBG) || tag.Equals(VBN)
                            || tag.Equals(VBP) || tag.Equals(VBZ))
                        {
                            tagBuilder.AppendLine(tokens[i]);
                            countTagged++;
                        }
                    }
                    i++;

                }
                Console.WriteLine("Building tags done.");
                Console.WriteLine("Writing tags to file..");
                //write tags to file
                if (!System.IO.Directory.Exists(@"C:\bigdata"))
                    System.IO.Directory.CreateDirectory(@"C\bigdata");
                System.IO.File.WriteAllText(@"C:\bigdata\tagslist.txt", tagBuilder.toString());
                Console.WriteLine(countTagged + " Tags successfully Written to file!");
                Console.Read();
            }


            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.Read();
            }

        }


        enum TagType
        {
            Noun,
            Verb
        }
    }

}