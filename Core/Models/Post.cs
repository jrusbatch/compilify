using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilify.Models
{
    public class Post : ICodeProject
    {
        private ICollection<Document> documents;
        private ICollection<string> tags; 

        public Post()
        {
            tags = new HashSet<string>();
            documents = new HashSet<Document>
                        {
                            new Document("Content", string.Empty),
                            new Document("Classes", string.Empty)
                        };
        }

        public string Id { get; set; }
        
        public string Slug { get; set; }

        public int Version { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }

        public IEnumerable<string> Tags
        {
            get { return tags; }
            set { tags = new HashSet<string>(value ?? Enumerable.Empty<string>()); }
        }
        
        public string Content
        {
            get
            {
                var doc = GetOrCreateDocument("Content");
                return doc != null ? doc.Text : string.Empty;
            }
            set
            {
                var doc = GetOrCreateDocument("Content");
                doc.Text = value;
            }
        }
        
        public string Classes
        {
            get
            {
                var doc = GetOrCreateDocument("Classes");
                return doc != null ? doc.Text : string.Empty;
            }
            set
            {
                var doc = GetOrCreateDocument("Classes");
                doc.Text = value;
            }
        }
        
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

        public IEnumerable<Document> Documents
        {
            get { return documents; }
            set { documents = new HashSet<Document>(value ?? Enumerable.Empty<Document>()); }
        }

        public void AddOrUpdateDocument(string name, string text)
        {
            var doc = documents.FirstOrDefault(x => x.Name == name);

            if (doc != null)
            {
                doc.Text = text;
            }
            else
            {
                documents.Add(new Document(name, text));
            }
        }

        /// <summary>
        /// The UTC date and time that the post was first persisted to the data store.</summary>
        public DateTime? Created { get; set; }

        private Document GetOrCreateDocument(string name)
        {
            var document = documents.FirstOrDefault(x => x.Name == name);

            if (document == null)
            {
                documents.Add(document = new Document(name, string.Empty));
            }

            return document;
        }
    }
}
