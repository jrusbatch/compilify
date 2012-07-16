using System.Linq;
using Compilify.Models;
using Compilify.Utilities;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Compilify.Data.Mongo
{
    public class MongoDbPostRepository : IPostRepository
    {
        private readonly MongoDatabase db;

        public MongoDbPostRepository(MongoDatabase mongoDatabase)
        {
            db = mongoDatabase;

            db.GetCollection("posts").EnsureIndex(
                IndexKeys.Descending("Slug", "Version"), 
                IndexOptions.SetUnique(true));

            db.GetCollection("sequences").EnsureIndex(
                IndexKeys.Descending("Name"), 
                IndexOptions.SetUnique(true));
        }

        public Post GetVersion(string slug, int version)
        {
            var query = Query.And(
                Query.EQ("Slug", slug),
                Query.EQ("Version", version));

            return db.GetCollection<Post>("posts").FindOne(query);
        }

        public int GetLatestVersion(string slug)
        {
            return db.GetCollection<Post>("posts")
                     .Find(Query.EQ("Slug", slug))
                     .SetSlaveOk(false)
                     .SetSortOrder(SortBy.Descending("Version"))
                     .SetLimit(1)
                     .SetFields("Slug", "Version")
                     .Select(x => x.Version)
                     .FirstOrDefault();
        }

        public Post Save(string slug, Post content)
        {
            var isNew = string.IsNullOrEmpty(slug);
            if (isNew)
            {
                // No slug was specified, so we need to get the next one
                var docs = db.GetCollection("sequences");

                docs.EnsureIndex(IndexKeys.Ascending("Name"), IndexOptions.SetUnique(true));

                var sequence = docs.FindAndModify(Query.EQ("Name", "slug"), null, Update.Inc("Current", 1), true, true)
                                   .GetModifiedDocumentAs<Incrementor>();

                slug = Base36Encoder.Encode(sequence.Current);
            }

            content.Slug = slug;

            content.Version = isNew ? 1 : GetLatestVersion(slug) + 1;

            db.GetCollection<Post>("posts").Save(content, SafeMode.True);

            return content;
        }
    }
}
