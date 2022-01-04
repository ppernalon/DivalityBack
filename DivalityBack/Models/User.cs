using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public int Victory { get; set; }
        public int Defeat { get; set; }
        public int Disciples { get; set; }
        public List<String> Collection { get; set; }
        public List<String> Friends{ get; set; }
        public List<Team> Teams { get; set; }

        protected bool Equals(User other)
        {
            return Id == other.Id && Username == other.Username && Password == other.Password && Victory == other.Victory && Defeat == other.Defeat && Disciples == other.Disciples && Equals(Collection, other.Collection) && Equals(Friends, other.Friends) && Equals(Teams, other.Teams);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(Username);
            hashCode.Add(Password);
            hashCode.Add(Victory);
            hashCode.Add(Defeat);
            hashCode.Add(Disciples);
            hashCode.Add(Collection);
            hashCode.Add(Friends);
            hashCode.Add(Teams);
            return hashCode.ToHashCode();
        }
    }
}
