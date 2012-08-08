using Compilify.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Compilify.DataAccess.MongoDB
{
    public static class MongoDbInitializer
    {
        public static void Initialize()
        {
            BsonClassMap.RegisterClassMap<Post>(x =>
            {
                x.AutoMap();

                x.SetIdMember(x.GetMemberMap(y => y.Id));
                x.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetRepresentation(BsonType.ObjectId);

                x.GetMemberMap(y => y.Slug).SetIsRequired(true);

                x.GetMemberMap(y => y.Version).SetDefaultValue(1);
            });

            BsonClassMap.RegisterClassMap<Incrementor>(x =>
            {
                x.AutoMap();

                x.SetIdMember(x.GetMemberMap(y => y.Id));
                x.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetRepresentation(BsonType.ObjectId);

                x.GetMemberMap(y => y.Name).SetIsRequired(true);

                x.GetMemberMap(y => y.Current).SetDefaultValue(0);
            });
        }
    }
}