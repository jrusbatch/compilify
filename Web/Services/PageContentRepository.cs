using System.Globalization;
using BookSleeve;
using Compilify.Web.Models;

namespace Compilify.Web.Services
{
    public interface IPageContentRepository
    {
        PageContent Get(string slug, int version = 1);

        PageContent Save(string slug, PageContent content);
    }

    public class PageContentRepository : IPageContentRepository
    {
        public PageContentRepository(RedisConnection redisConnection)
        {
            redis = redisConnection;
        }

        private readonly RedisConnection redis;

        public PageContent Get(string slug, int version = 1)
        {
            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug.ToLowerInvariant(), version);
            var code = redis.Wait(redis.Hashes.GetString(0, key, "Code"));

            return new PageContent { Code = code };
        }

        public PageContent Save(string slug, PageContent content)
        {
            var version = (int) redis.Wait(redis.Strings.Increment(0, "sequence:content:" + slug));

            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug, version);

            if (redis.Wait(redis.Hashes.Set(0, key, "Code", content.Code)))
            {
                content.Slug = slug;
                content.Version = version;
            }

            return content;
        }
    }
}