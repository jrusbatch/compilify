using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Models
{
    public class PageContentViewModel
    {
        public PageContentViewModel()
        {
            Content = new PageContent();
            Errors = new List<Diagnostic>();
        }

        public PageContent Content { get; set; }
        public IEnumerable<Diagnostic> Errors { get; set; } 
    }
}