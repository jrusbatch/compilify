using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Models
{
    public class PageContentViewModel
    {
        public PageContent Content { get; set; }
        public IEnumerable<Diagnostic> Errors { get; set; } 
    }
}