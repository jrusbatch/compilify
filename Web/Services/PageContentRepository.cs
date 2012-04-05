using System.Globalization;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Web.Models;

namespace Compilify.Web.Services
{
    public interface IPageContentRepository
    {
        Task<PageContent> Get(string slug, int version = 1);

        Task<PageContent> Save(string slug, PageContent content);
    }

    public class PageContentRepository : IPageContentRepository
    {
        public PageContentRepository(RedisConnection redisConnection)
        {
            db = redisConnection;
        }

        private readonly RedisConnection db;

        public async Task<PageContent> Get(string slug, int version = 1)
        {
            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug.ToLowerInvariant(), version);
            var code = await db.Hashes.GetString(0, key, "Code");

            return new PageContent { Code = code };
        }

        public async Task<PageContent> Save(string slug, PageContent content)
        {
            var version = (int)await db.Strings.Increment(0, "sequence:content:" + slug);

            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug, version);

            if (await db.Hashes.Set(0, key, "Code", content.Code))
            {
                content.Slug = slug;
                content.Version = version;
            }

            return content;
        }
    }
}