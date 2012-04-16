using System.Collections.Generic;
using Compilify.Models;

namespace Compilify.Web.Models
{
    public class PostViewModel
    {
        public PostViewModel()
        {
            Post = new Post();
            Errors = new List<string>();
        }

        public Post Post { get; set; }

        public IEnumerable<string> Errors { get; set; } 
    }
}
