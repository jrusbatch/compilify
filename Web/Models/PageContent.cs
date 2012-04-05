using System;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Models
{
    [Serializable]
    public class PageContent
    {
        public string Slug { get; set; }
        public int Version { get; set; }
        public string Code { get; set; }

        public IEnumerable<Diagnostic> Errors { get; set; }
    }
}