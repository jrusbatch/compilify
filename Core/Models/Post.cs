using System;
using System.Collections.Generic;

namespace Compilify.Models
{
    public class Post : ICodeProject
    {
        private readonly ISet<Document> documents;

        public Post()
        {
            documents = new HashSet<Document>();
        }

        public string Id { get; set; }
        
        public string Slug { get; set; }

        public int Version { get; set; }

        string ICodeProject.Name
        {
            get { return "Untitled"; }
        }

        TimeSpan ICodeProject.TimeoutPeriod
        {
            get { return TimeSpan.FromSeconds(5D); }
        }

        /// <summary>
        /// The ID of the user who created the post, or null if the post was created by an anonymous user.</summary>
        public Guid? AuthorId { get; set; }
        
        /// <summary>
        /// If <c>true</c>, the post will not be displayed in search results. However, it will still be accessible via 
        /// direct access.</summary>
        public bool? IsPrivate { get; set; }

        /// <summary>
        /// The page lanage.</summary>
        public string Language { get; set; }

        public IEnumerable<ICodeDocument> Documents
        {
            get { return documents; }
        }

        public void AddDocument(string name, string text)
        {
            documents.Add(new Document(name, text));
        }

        /// <summary>
        /// The UTC date and time that the post was first persisted to the data store.</summary>
        public DateTime? Created { get; set; }
    }
}
