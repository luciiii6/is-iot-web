using IsIoTWeb.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IsIoTWeb.Models
{
    public class ValveLog : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("valveId")]
        public int ValveId { get; set; }

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }

        [BsonElement("action")]
        public string Action { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("sinkId")]
        public string SinkId { get; set; } = StaticVariables.SinkId;

        public string Date()
        {
            return Utils.Utils.TimestampToDatetime(Timestamp);
        }
    }
}
