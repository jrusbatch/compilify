using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Compilify.Web.Models
{
    [Serializable]
    public class PageContent
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRequired]
        public string Slug { get; set; }

        [BsonRequired]
        public int Version { get; set; }

        [BsonRequired]
        public string Code { get; set; }
    }
}