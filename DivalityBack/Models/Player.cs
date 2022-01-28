using System;

namespace DivalityBack.Models
{
    public class Player
    {

        public Player(GodTeam godTeam, string username)
        {
            Life = 500; // base life
            GodTeam = godTeam;
            Username = username;
        }

        public string Username { set; get; }
        public GodTeam GodTeam { set; get; }
        public int Life { set; get; }

        public Boolean isAlive()
        {
            Boolean isPlayerHealthy = Life > 0;
            Boolean areGodsHealthy = GodTeam.isAlive();
            return isPlayerHealthy && areGodsHealthy;
        }

        public Boolean isEqual(Player player)
        {
            return player.Username == Username;
        }
        
        public void getStriked(int amountOfDamge)
        {
            Life -= amountOfDamge;
        }
    }
}