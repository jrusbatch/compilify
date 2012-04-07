using System.Linq;
using BookSleeve;
using Compilify.Web.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Compilify.Web.Services
{
    public interface IPageContentRepository
    {
        PageContent GetVersion(string slug, int version);

        PageContent GetLatestVersion(string slug);

        void QueueForCompilation(string slug, int version);

        PageContent Save(string slug, PageContent content);
    }

    public class PageContentRepository : IPageContentRepository
    {
        public PageContentRepository(RedisConnection redisConnection, MongoDatabase mongoDatabase)
        {
            redis = redisConnection;
            documents = mongoDatabase.GetCollection<PageContent>("content");
            documents.EnsureIndex("Slug", "Version");
        }

        private readonly RedisConnection redis;
        private readonly MongoCollection<PageContent> documents;

        public PageContent GetVersion(string slug, int version)
        {
            //var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug.ToLowerInvariant(), version);
            //var code = redis.Wait(redis.Hashes.GetString(0, key, "Code"));

            return documents.FindOne(Query.And(
                                         Query.EQ("Slug", slug),
                                         Query.EQ("Version", version)
                                    ));
        }

        public PageContent GetLatestVersion(string slug)
        {
            return documents.Find(Query.EQ("Slug", slug))
                            .SetSortOrder(SortBy.Descending("Version"))
                            .FirstOrDefault();
        }

        public void QueueForCompilation(string slug, int version)
        {
            var content = GetVersion(slug, version);

            if (content != null)
            {
                redis.Publish("workers:execute", content.Code);
                // redis.Lists.AddLast(0, "queue:executer", content.Code);
            }
        }

        public PageContent Save(string slug, PageContent content)
        {
            //var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug, version);

            //if (redis.Wait(redis.Hashes.Set(0, key, "Code", content.Code)))
            //{
            //    content.Slug = slug;
            //    content.Version = version;
            //}

            var version = (int)redis.Wait(redis.Strings.Increment(0, "sequence:content:" + slug));

            if (string.IsNullOrEmpty(content.Slug))
            {
                content.Slug = slug;
            }
            
            content.Version = version;

            documents.Insert(content);
            
            return content;
        }
    }
}