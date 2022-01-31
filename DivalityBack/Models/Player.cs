using System;
using System.Net.WebSockets;

namespace DivalityBack.Models
{
    public class Player
    {

        public Player(GodTeam godTeam, string username, WebSocket webSocket)
        {
            GodTeam = godTeam;
            Username = username;
            PlayerWebSocket = webSocket;
        }

        public string Username { set; get; }
        public GodTeam GodTeam { set; get; }
        public WebSocket PlayerWebSocket { set; get; }

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