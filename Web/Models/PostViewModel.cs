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
        {
            Documents = new List<DocumentModel>();
            Errors = new List<EditorError>();
        }

        public static PostViewModel Create(Post post)
        {
            if (post == null)
            {
                return null;
            }

            var model = new PostViewModel
                        {
                            Slug = post.Slug,
                            Version = post.Version,
                            AuthorId = post.AuthorId,
                            Documents = post.Documents
                               .Select(doc => new DocumentModel { Name = doc.Name, Text = doc.GetText() })
                               .ToArray()
                        };

            return model;
        }

        public string Slug { get; set; }
        
        public int Version { get; set; }

        public Guid? AuthorId { get; set; }

        public IEnumerable<DocumentModel> Documents { get; set; }

        public IEnumerable<EditorError> Errors { get; set; } 

        public Post ToPost()
        {
            var post = new Post
                       {
                           Slug = Slug,
                           Version = Version,
                       };

            foreach (var doc in Documents)
            {
                post.AddOrUpdateDocument(doc.Name, doc.Text);
            }
            
            return post;
        }
    }
}
