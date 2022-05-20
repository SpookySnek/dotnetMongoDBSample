using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CypointTestCalle;

public class Accounts
{
    public string Id { get; set; }
    public int TotalBalance { get; set; }
    public List<int> Transactions { get; set; }
}

public class Program
{
    private static IMongoClient _client;
    private static IMongoDatabase _database;
    private static IMongoCollection<Accounts> _collection;

    private void CRUDwithMongoDb()
    {
        Console.WriteLine("Please enter account name(cAsE) [empty to exit]:");
        var userSelection = Console.ReadLine();

        if (_collection.Find(s => s.Id == userSelection).Any())
        {
            Console.WriteLine
                ("Please enter amount:");
            var input = Console.ReadLine();

            if (Regex.IsMatch(input, "[^-.0-9]"))
            {
                Console.WriteLine("Invalid amount, please try again");
                CRUDwithMongoDb();
            }

            var amount = int.Parse(input);

            var projection = _collection.Find(s => s.Id == userSelection)
                .Project(x => new { x.TotalBalance }).First();

            var orgAmountStripped = int.Parse(Regex.Replace(projection.ToString(), "[^-.0-9]", ""));

            var newAmount = amount + orgAmountStripped;

            _collection.FindOneAndUpdate<Accounts>
            (Builders<Accounts>.Filter.Eq("Id", userSelection),
                Builders<Accounts>.Update.Set("TotalBalance", newAmount).Push("Transactions", amount));

            Console.WriteLine($"Current balance: {newAmount}");
            CRUDwithMongoDb();
        }

        if (userSelection == string.Empty)
        {
            var total = _collection.AsQueryable().Sum(x => x.TotalBalance);
            Console.WriteLine($"Bank total balance: {total}");
            Console.WriteLine("\nPlease press [ENTER] to exit");
            Console.ReadKey();
            Console.WriteLine("Have a good one!");
            Environment.Exit(0);
        }

        else
        {
            Console.WriteLine("Please enter amount:");
            var input = Console.ReadLine();
            if (Regex.IsMatch(input, "[^-.0-9]"))
            {
                Console.WriteLine("Invalid amount, please try again");
                CRUDwithMongoDb();
            }

            var userBalance = int.Parse(input);
            var firstTransaction = new List<int> { userBalance };
            var account = new Accounts
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
            .AddJsonFile("appsettings.json", true, true);

        var config = builder.Build();
        var urlKey = config["MongoDB:ConnectionURI"];
        var dbName = config["MongoDB:DatabaseName"];
        var colName = config["MongoDB:CollectionName"];

        _client = new MongoClient(urlKey);
        _database = _client.GetDatabase(dbName);
        _collection = _database.GetCollection<Accounts>(colName);

        var p = new Program();
        p.CRUDwithMongoDb();
    }
}