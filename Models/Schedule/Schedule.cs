using IsIoTWeb.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IsIoTWeb.Models.Schedule
{
    public class Schedule : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }

        [BsonElement("duration")]
        public double Duration { get; set; }

        [BsonElement("sinkId")]
        public string SinkId { get; set; } = StaticVariables.SinkId;
    }
}
