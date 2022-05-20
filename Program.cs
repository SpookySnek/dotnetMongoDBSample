using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CypointTestCalle
{
    public class Accounts
    { 
        public string Id { get; set; }
        public int TotalBalance { get; set; }
        public List<int> Transactions { get; set; }
    }

    public class Program
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        private static IMongoCollection<Accounts> _collection;
        public void CRUDwithMongoDb()
        {


            Console.WriteLine("Please enter account name [empty to exit]:");
            string userSelection = Console.ReadLine();

            if (_collection.Find(S => S.Id == userSelection).Any())
            {
                Console.WriteLine
                    ("Please enter amount:");
                int amount = int.Parse(Console.ReadLine());
  
                var projection = _collection.Find(S => S.Id == userSelection)
                    .Project(x => new {x.TotalBalance}).First().ToString();
                    
                int orgAmountStripped = int.Parse(Regex.Replace(projection, "[^-.0-9]", ""));

                int newAmount = amount + orgAmountStripped;
                
                _collection.FindOneAndUpdate<Accounts>
                (Builders<Accounts>.Filter.Eq("Id", userSelection),
                    Builders<Accounts>.Update.Set("TotalBalance", newAmount).Push("Transactions", amount));
               
                Console.WriteLine($"Current balance: {newAmount}");
                CRUDwithMongoDb();
            }

            if (userSelection == String.Empty)
            {
                var total = _collection.AsQueryable().Sum(x => x.TotalBalance);
                Console.WriteLine($"Bank total balance: {total}");
                Console.WriteLine($"\nPlease press [ENTER] to exit");
                Console.ReadKey();
                Console.WriteLine("Have a good one!");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine
                    ("Please enter amount:");
                int userBalance = int.Parse(Console.ReadLine());
                List<int> firstTransaction = new List<int> {userBalance};
                Accounts account = new Accounts()
                {
                    Id = userSelection,
                    TotalBalance = userBalance,
                    Transactions = firstTransaction
                };
                _collection.InsertOne(account);
                Console.WriteLine($"Current balance: {userBalance}");
            }
            CRUDwithMongoDb();
        }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);
    
            var config = builder.Build();
            var urlKey = config["MongoDB:ConnectionURI"];
            var dbName = config["MongoDB:DatabaseName"];
            var colName = config["MongoDB:CollectionName"];
            
            _client = new MongoClient(urlKey);
            _database = _client.GetDatabase(dbName);
            _collection = _database.GetCollection<Accounts>(colName);
           
            Program p = new Program();
            p.CRUDwithMongoDb();
            
            Console.WriteLine("Press any key to terminate the program");
            Console.ReadKey();
        }
    }
}