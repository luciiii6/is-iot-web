using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace IsIoTWeb.Models
{
    [BsonIgnoreExtraElements]
    public class Sink : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("sinkId")]
        public string SinkId { get; set; }

        [BsonElement("collectors")]
        public ICollection<string> Collectors { get; set; }

        [BsonElement("users")]
        public ICollection<string> Users { get; set; }
    }
}