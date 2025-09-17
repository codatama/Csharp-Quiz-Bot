using MongoDB.Driver;
using TelegramTestBot.Domain.Interfaces;
using TelegramTestBot.Domain.Models;

namespace TelegramTestBot.Infrastructure.Mongo
{
    public class MongoTestRepository : ITestRepository
    {
        private readonly IMongoCollection<TestDocument> _collection;

        public MongoTestRepository(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            _collection = database.GetCollection<TestDocument>("Tests");
        }

        public async Task SaveAsync(TestDocument test) =>
            await _collection.InsertOneAsync(test);

        public async Task<TestDocument?> GetByIdAsync(string id) =>
            await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task<List<TestDocument>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<List<TestDocument>> GetByUserIdAsync(long userId)
        {
            var filter = Builders<TestDocument>.Filter.Eq(t => t.OwnerUserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}   