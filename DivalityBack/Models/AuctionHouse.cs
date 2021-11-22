using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models
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
