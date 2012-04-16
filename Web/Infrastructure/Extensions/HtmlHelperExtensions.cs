using System.Configuration;
using System.Web.Mvc;

namespace Compilify.Web.Infrastructure.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Inserts the Google Analytics script if a valid account token is specified in the web.config and 
        /// debugging is not enabled for the current HttpContext.</summary>
        public static MvcHtmlString Analytics(this HtmlHelper helper)
        {
            var accountId = ConfigurationManager.AppSettings["Compilify.GoogleAnalyticsAccount"];
            
            if (helper.ViewContext.HttpContext.IsDebuggingEnabled || string.IsNullOrWhiteSpace(accountId))
            {
                return MvcHtmlString.Empty;
            }

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
    }
}
