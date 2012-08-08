using MongoDB.Driver;

namespace Compilify.DataAccess.MongoDB
{
    public class MongoConnectionFactory : IMongoConnectionFactory
    {
        private readonly string connectionString;

        public MongoConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public MongoDatabase Create()
        {
            return MongoDatabase.Create(connectionString);
        }
    }
}