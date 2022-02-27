﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace IsIoTWeb.Models
{
    [BsonIgnoreExtraElements]
    public class Reading : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("collectorId")]
        public int CollectorId { get; set; }

        [BsonElement("timestamp")]
        public double Timestamp { get; set; }

        [BsonElement("soilMoisture")]
        public List<double> SoilMoisture  { get; set; }

        [BsonElement("airTemp")]
        public double AirTemp { get; set; }

        [BsonElement("airHummidity")]
        public double AirHummidity { get; set; }

        [BsonElement("lightIntensity")]
        public double LightIntensity { get; set; }
    }
}