using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compilify.LanguageServices;
using Compilify.Models;

namespace Compilify.Web.Models
{
    public class PostViewModel
    {
        public PostViewModel()
            : this(null)
        {
        }

        public PostViewModel(Post post)
        {
            if (post != null)
            {
                Slug = post.Slug;
                Version = post.Version;

                Title = post.Title;
                Description = post.Description;
                AuthorId = post.AuthorId;

                Tags = string.Join(" ", post.Tags.OrderBy(x => x).ToArray());

                Content = post.Content;
                Classes = post.Classes;
            }

            Errors = new List<EditorError>();
        }

        public string Slug { get; set; }
        
        public int Version { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public Guid? AuthorId { get; set; }

        [AllowHtml]
        public string Content { get; set; }
        
        [AllowHtml]
        public string Classes { get; set; }

        public string Tags { get; set; }

        public IEnumerable<EditorError> Errors { get; set; } 

        public Post ToPost()
        {
            var post = new Post
                       {
                           Slug = Slug,
                           Version = Version,

                           Title = Title,
                           Description = Description,

                           Content = Content,
                           Classes = Classes
                       };

            if (!string.IsNullOrEmpty(Tags))
            {
                foreach (var tag in Tags.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    post.Tags.Add(tag);
                }
            }
            
            return post;
        }
    }
}
