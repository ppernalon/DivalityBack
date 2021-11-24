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

        private sealed class CardEqualityComparer : IEqualityComparer<Card>
        {
            public bool Equals(Card x, Card y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id && x.Name == y.Name && x.Pantheon == y.Pantheon && x.Rarity == y.Rarity &&
                       x.Life == y.Life && x.Speed == y.Speed && x.Power == y.Power && x.Armor == y.Armor &&
                       x.OffensiveAbility == y.OffensiveAbility && x.OffensiveEfficiency == y.OffensiveEfficiency &&
                       x.DefensiveAbility == y.DefensiveAbility && x.DefensiveEfficiency == y.DefensiveEfficiency &&
                       x.isLimited == y.isLimited && x.Distributed == y.Distributed && x.Available == y.Available;
            }

            public int GetHashCode(Card obj)
            {
                var hashCode = new HashCode();
                hashCode.Add(obj.Id);
                hashCode.Add(obj.Name);
                hashCode.Add(obj.Pantheon);
                hashCode.Add(obj.Rarity);
                hashCode.Add(obj.Life);
                hashCode.Add(obj.Speed);
                hashCode.Add(obj.Power);
                hashCode.Add(obj.Armor);
                hashCode.Add(obj.OffensiveAbility);
                hashCode.Add(obj.OffensiveEfficiency);
                hashCode.Add(obj.DefensiveAbility);
                hashCode.Add(obj.DefensiveEfficiency);
                hashCode.Add(obj.isLimited);
                hashCode.Add(obj.Distributed);
                hashCode.Add(obj.Available);
                return hashCode.ToHashCode();
            }
        }

        public static IEqualityComparer<Card> CardComparer { get; } = new CardEqualityComparer();

      
    }
}
