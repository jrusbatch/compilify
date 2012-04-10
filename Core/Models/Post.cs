using System;

namespace Compilify.Models {

    public class Post {

        public string Id { get; set; }

        public string Slug { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// The ID of the user who created the post, or null if the post was 
        /// created by an anonymous user.</summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// The post content.</summary>
        public string Content { get; set; }

        /// <summary>
        /// The UTC date and time that the post was first persisted to the 
        /// data store.</summary>
        public DateTime? Created { get; set; }
    }

    public class Incrementor {

        public string Id { get; set; }

        public string Name { get; set; }

        public int Current { get; set; }

    }
}
