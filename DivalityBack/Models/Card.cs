using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models
{
    public class Card
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Pantheon { get; set; }
        public string Rarity { get; set; }
        public int Life { get; set; }
        public int Speed { get; set; }
        public int Power { get; set; }
        public int Armor { get; set; }
        public string OffensiveAbility { get; set; }
        public int OffensiveEfficiency { get; set; }
        public string DefensiveAbility { get; set; }
        public int DefensiveEfficiency { get; set; }
        public bool isLimited { get; set; }
        public int Distributed { get; set; }
        public int Available { get; set; }
        public string Description { get; set; }
    }
}
