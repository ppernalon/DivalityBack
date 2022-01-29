using System;

namespace DivalityBack.Models
{
    public class Player
    {

        public Player(GodTeam godTeam, string username)
        {
            GodTeam = godTeam;
            Username = username;
        }

        public string Username { set; get; }
        public GodTeam GodTeam { set; get; }

        public Boolean isAlive()
        {
            return GodTeam.isAlive();
        }

        public Boolean isEqual(Player player)
        {
            return player.Username == Username;
        }
    }
}