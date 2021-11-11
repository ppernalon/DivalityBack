using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Divality.Models
{
    public class Team
    {
        public string Name { get; set; }
        public string[] Compo { get; set; }
    }
}
