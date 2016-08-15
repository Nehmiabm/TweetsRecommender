using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TweetsRecommender
{
    public class MongoConnector
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        public MongoConnector()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("TweetDB");

        }

        public async void InsertDocumentAsync(BsonDocument document)
        {
            var collection = _database.GetCollection<BsonDocument>("tweets");
            await collection.InsertOneAsync(document);
        }

        public async void DeleteAllTweetsAsync()
        {
            var collection = _database.GetCollection<BsonDocument>("tweets");
            await collection.DeleteManyAsync(new BsonDocument());
        }
       
    }
}
