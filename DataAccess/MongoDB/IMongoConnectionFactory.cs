using MongoDB.Driver;

namespace Compilify.DataAccess.MongoDB
{
    public interface IMongoConnectionFactory
    {
        MongoDatabase Create();
    }
}