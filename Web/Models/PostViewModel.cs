using System.Collections.Generic;
using Compilify.Models;

namespace Compilify.Web.Models
{
    public class PostViewModel
    {
        public PostViewModel()
        {
            Post = new Post();
            Errors = new List<EditorError>();
        }

        public bool CurrentUserIsAuthor { get; set; }

        public bool HasSlug
        {
            get { return Post != null && Post.Slug != null; }
        }

        public Post Post { get; set; }

        public IEnumerable<EditorError> Errors { get; set; } 
    }
}
