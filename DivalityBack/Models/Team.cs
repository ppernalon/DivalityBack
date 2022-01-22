using System;
using System.Collections.Generic;

namespace DivalityBack.Models
{
    public class Team
    {
        public Team()
        {
            Name = "";
            Compo = new List<string>();
        }
        public string Name { get; set; }
        public List<String> Compo { get; set; }
    }
}
