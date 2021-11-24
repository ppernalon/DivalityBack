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

        private sealed class UserEqualityComparer : IEqualityComparer<User>
        {
            public bool Equals(User x, User y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id && x.Username == y.Username && x.Password == y.Password && x.Victory == y.Victory && x.Defeat == y.Defeat && x.Disciples == y.Disciples && x.Collection == y.Collection && Equals(x.Friends, y.Friends) && Equals(x.Teams, y.Teams);
            }

            public int GetHashCode(User obj)
            {
                var hashCode = new HashCode();
                hashCode.Add(obj.Id);
                hashCode.Add(obj.Username);
                hashCode.Add(obj.Password);
                hashCode.Add(obj.Victory);
                hashCode.Add(obj.Defeat);
                hashCode.Add(obj.Disciples);
                hashCode.Add(obj.Collection);
                hashCode.Add(obj.Friends);
                hashCode.Add(obj.Teams);
                return hashCode.ToHashCode();
            }
        }

        public static IEqualityComparer<User> UserComparer { get; } = new UserEqualityComparer();

        public string Username { get; set; }
        public string Password { get; set; }
        public int Victory { get; set; }
        public int Defeat { get; set; }
        public int Disciples { get; set; }
        public int Collection { get; set; }
        public string[] Friends{ get; set; }
        public Team[] Teams { get; set; }


    }
}
