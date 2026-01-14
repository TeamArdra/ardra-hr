using Microsoft.Extensions.Options;
using MongoDB.Driver;
using backend.Models;

namespace backend.Services;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Review> Reviews { get; }

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        
        Users = _database.GetCollection<User>("users");
        Reviews = _database.GetCollection<Review>("reviews");
    }

    public async Task InitializeIndexesAsync()
    {
        // Create unique index on registration number
        var userIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.RegNumber);
        var userIndexOptions = new CreateIndexOptions { Unique = true };
        var userIndexModel = new CreateIndexModel<User>(userIndexKeys, userIndexOptions);
        await Users.Indexes.CreateOneAsync(userIndexModel);

        // Create compound index for reviews querying
        var reviewIndexKeys = Builders<Review>.IndexKeys
            .Ascending(r => r.SubjectRegNumber)
            .Ascending(r => r.MonthYear);
        var reviewIndexModel = new CreateIndexModel<Review>(reviewIndexKeys);
        await Reviews.Indexes.CreateOneAsync(reviewIndexModel);

        Console.WriteLine("MongoDB indexes initialized");
    }
}