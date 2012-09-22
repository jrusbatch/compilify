using System.Web.Mvc;

namespace Compilify.Web.Models
{
    public class DocumentModel
    {
        public string Name { get; set; }

        [AllowHtml]
        public string Text { get; set; }
    }
}