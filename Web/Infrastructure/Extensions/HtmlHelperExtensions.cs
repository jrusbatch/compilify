using System.Web.Mvc;

namespace Compilify.Web.Infrastructure.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString Analytics(this HtmlHelper helper)
        {
#if !DEBUG
            var accountId = System.Configuration.ConfigurationManager.AppSettings["GoogleAnalyticsAccount"];
            
            if (!string.IsNullOrWhiteSpace(accountId))
            {
                return MvcHtmlString.Create(
                    @"<script>
                          var _gaq = _gaq || [];
                          _gaq.push(['_setAccount', '" + accountId + @"']);
                          _gaq.push(['_trackPageview']);

                          (function () {
                             var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                             ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
                             var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
                          })();
                      </script>");
            }
#endif

            return MvcHtmlString.Empty;
        }
    }
}
