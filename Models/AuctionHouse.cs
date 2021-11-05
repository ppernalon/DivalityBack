using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Divality.Models
{
    public class AuctionHouse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CardId { get; set; }
        public string OwnerId { get; set; }
        public int Price { get; set; }

    }
}
