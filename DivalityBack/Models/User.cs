using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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
