using System;
using System.Collections.Generic;

namespace Compilify.Models
{
    public class Post : ICodeProgram
    {
        public Post()
        {
            Tags = new HashSet<string>();
        }

        public string Id { get; set; }
        
        public string Title { get; set; }

        public string Slug { get; set; }

        public int Version { get; set; }

        public string Description { get; set; }

        string ICodeProgram.Name
        {
            get { return Title; }
        }

        TimeSpan ICodeProgram.TimeoutPeriod
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

        /// <summary>
        /// The post content.</summary>
        public string Content { get; set; }

        /// <summary>
        /// Supporting classes</summary>
        public string Classes { get; set; }

        public HashSet<string> Tags { get; protected set; }

        /// <summary>
        /// The UTC date and time that the post was first persisted to the data store.</summary>
        public DateTime? Created { get; set; }
    }
}
