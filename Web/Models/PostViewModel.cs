using System;
using System.Collections.Generic;
using Compilify.LanguageServices;
using Compilify.Models;

namespace Compilify.Web.Models
{
    public class PostViewModel
    {
        public PostViewModel()
        {
            References = new List<ReferenceViewModel>();
            Errors = new List<EditorError>();
            Project = new Project();
        }

        public PostViewModel(Post post)
            : this()
        {
            if (post != null)
            {
                Slug = post.Slug;
                Version = post.Version;
                AuthorId = post.AuthorId;
                ConsoleText = string.Empty;
            }
        }

        public Project Project { get; set; }

        public string Slug { get; set; }

        public int Version { get; set; }

        public Guid? AuthorId { get; set; }

        public string ConsoleText { get; set; }

        public IList<ReferenceViewModel> References { get; private set; }

        public IEnumerable<EditorError> Errors { get; set; }

        public Post ToPost()
        {
            var post = new Post
            {
                Slug = Slug,
                Version = Version,

                // Content = ConsoleText,

                // Content = Content,
                // Classes = Classes
            };

            return post;
        }
    }

    public class ReferenceViewModel
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public bool IsRemovable { get; set; }
    }
}
