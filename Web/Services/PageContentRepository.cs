using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Web.Models;

namespace Compilify.Web.Services
{
    public interface IPageContentRepository
    {
        PageContent Get(string slug, int version = 1);

        PageContent Save(PageContent content);
    }

    public class PageContentRepository : IPageContentRepository
    {
        public PageContentRepository(RedisConnection redisConnection)
        {
            db = redisConnection;
        }

        private readonly RedisConnection db;

        public PageContent Get(string slug, int version = 1)
        {
            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug.ToLowerInvariant(), version);
            var code = db.Wait(db.Hashes.GetString(0, key, "Code"));

            return new PageContent
                   {
                       Code = code
                   };
        }

        public PageContent Save(PageContent content)
        {


            var id = (int)db.Wait(db.Strings.Increment(0, "sequence:url"));
            var slug = Base32Encoder.Encode(id);

            var version = (int)db.Wait(db.Strings.Increment(0, "sequence:content:" + slug.ToLowerInvariant()));

            var key = string.Format(CultureInfo.InvariantCulture, "content:{0}:{1}", slug.ToLowerInvariant(), version);

            db.Wait(db.Hashes.Set(0, key, "Code", content.Code));

            content.Slug = slug;
            content.Version = version;

            return content;
        }
    }
}