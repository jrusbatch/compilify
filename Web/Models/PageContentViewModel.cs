using System.Collections.Generic;

namespace Compilify.Web.Models
{
    public class PageContentViewModel
    {
        public PageContentViewModel()
        {
            Content = new PageContent();
            Errors = new List<string>();
        }

        public PageContent Content { get; set; }
        public IEnumerable<string> Errors { get; set; } 
    }
}