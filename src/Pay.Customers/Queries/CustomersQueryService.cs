using System.Threading.Tasks;
using MongoDB.Driver;
using Pay.Verification.Projections;

namespace Pay.Customers.Queries
{
    public class CustomersQueryService
    {
        IMongoCollection<ReadModels.CustomerDetails> _database;
        public CustomersQueryService(
            IMongoDatabase database
        )
        {
            _database = database.GetCollection<ReadModels.CustomerDetails>(nameof(ReadModels.CustomerDetails));
        }

        public async Task<ReadModels.CustomerDetails> GetCustomerById(string customerId)
        {
            var query = await _database.FindAsync(d => d.Id == customerId);
            return await query.SingleOrDefaultAsync();
        }
    }
}