using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models
{
    public class User
    {
        public User()
        {
            Username = "";
            Password = "";
            Victory = 0;
            Defeat = 0;
            Disciples = 80;
            Collection = new List<string>();
            Friends = new List<string>();
            Teams = new List<Team>();
        }
        
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
    }
}
