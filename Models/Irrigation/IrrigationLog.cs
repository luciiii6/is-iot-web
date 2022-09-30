using IsIoTWeb.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IsIoTWeb.Models
{
    [BsonIgnoreExtraElements]
    public class IrrigationLog : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("irrigationTime")]
        public double IrrigationTime { get; set; }

        [BsonElement("rainProbability")]
        public double RainProbability { get; set; }

        [BsonElement("completed")]
        public bool Completed { get; set; }

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }

        [BsonElement("sinkId")]
        public string SinkId { get; set; } = StaticVariables.SinkId;

        public string Date()
        {
            return Utils.Utils.TimestampToDatetime(Timestamp);
        }
    }
}