using System;
using System.Collections.Generic;
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

        private sealed class AuctionHouseEqualityComparer : IEqualityComparer<AuctionHouse>
        {
            public bool Equals(AuctionHouse x, AuctionHouse y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id && x.CardId == y.CardId && x.OwnerId == y.OwnerId && x.Price == y.Price;
            }

            public int GetHashCode(AuctionHouse obj)
            {
                return HashCode.Combine(obj.Id, obj.CardId, obj.OwnerId, obj.Price);
            }
        }

        public static IEqualityComparer<AuctionHouse> AuctionHouseComparer { get; } = new AuctionHouseEqualityComparer();
    }
}
