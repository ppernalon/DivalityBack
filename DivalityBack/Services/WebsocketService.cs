using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace Divality.Services
{
    public class WebsocketService
    {
        private Dictionary<String, WebSocket> mapActivePlayersWebsocket = new Dictionary<String, WebSocket>();
        private readonly UsersCRUDService _usersCrudService;

        public WebsocketService(UsersCRUDService usersCrudService)
        {
            _usersCrudService = usersCrudService; 
        }
        
        public async Task HandleMessages(WebSocket websocket){
            try {
                using (var ms = new MemoryStream()) {
                    while (websocket.State == WebSocketState.Open) {
                        WebSocketReceiveResult result;
                        do {
                            var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                            result = await websocket.ReceiveAsync(messageBuffer, CancellationToken.None);
                            ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Text) {
                            JsonDocument msgJson = JsonDocument.Parse(ms.ToArray());
                            switch (@msgJson.RootElement.GetProperty("type").ToString().Trim())
                            {
                                case "connexion":
                                    await HandleConnexion(websocket, ms, result, msgJson); 
                                    break;
                                case "deconnexion":
                                    await HandleDeconnexion(websocket, ms, result, msgJson);
                                    break;
                                default:
                                    await websocket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;
                            }
                        }
                        ms.SetLength(0);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                    }
                }
            } catch (InvalidOperationException e) {
                Console.Write("ERREUR WS: " + e.Message);
            }
        }
        
        public async Task HandleConnexion(WebSocket webSocket, MemoryStream memoryStream, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            List<String> listUsernameFriendsUserConnected = new List<string>();
            
            String username = msgJson.RootElement.GetProperty("username").ToString();
            //On met à jour une liste globale des utilisateurs connectés
            User userConnected =
                _usersCrudService.GetByUsername(username);
            mapActivePlayersWebsocket.Add(username, webSocket);

            //On recupère la liste des usernames des amis connectés 
            List<String> listUsernameFriends =
                _usersCrudService.GetUsersById(userConnected.Friends).Select(s => s.Username).ToList();
            List<String> listUsernameUsersConnected = new List<string>(mapActivePlayersWebsocket.Keys);
            List<String> listUsernameFriendsConnected =
                listUsernameFriends.Intersect(listUsernameUsersConnected).ToList();
    
            
            byte[] bytesUsernamesFriends = listUsernameFriendsConnected
                .SelectMany(s => Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();
            
            //On envoie au nouveau user la liste de ses amis connectés
            await webSocket.SendAsync(bytesUsernamesFriends, result.MessageType, result.EndOfMessage,
                CancellationToken.None);

            //On envoie à tous ses amis la liste de leurs amis mise à jour
            foreach (String usernameFriendUserConnected in listUsernameFriendsConnected)
            {
                User user = _usersCrudService.GetByUsername(usernameFriendUserConnected);
                
                //On récupère la liste des amis connectés de l'User
                listUsernameFriends =
                    _usersCrudService.GetUsersById(user.Friends).Select(s => s.Username).ToList();
                listUsernameUsersConnected = new List<string>(mapActivePlayersWebsocket.Keys);
                listUsernameFriendsConnected =
                    listUsernameFriends.Intersect(listUsernameUsersConnected).ToList(); 
                
                
                bytesUsernamesFriends = listUsernameFriendsConnected
                    .SelectMany(s => Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();
                
                WebSocket websocketFriendUserConnected = mapActivePlayersWebsocket[usernameFriendUserConnected];
                await websocketFriendUserConnected.SendAsync(bytesUsernamesFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);

            }
        }
        
        private async Task HandleDeconnexion(WebSocket websocket, MemoryStream ms, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            //On met à jour une liste globale des utilisateurs connectés
            User userDisconnected =
                _usersCrudService.GetByUsername(username);
            mapActivePlayersWebsocket.Remove(username);
            
            //On initialise une liste de pseudos des joueurs ayant cet utilisateur dans leur liste d'ami
            List<String> listUsersHavingUserDisconnectedAsFriend = new List<String>();
            
            //On recherche le joueur dans toutes les listes d'ami
            foreach (User user in _usersCrudService.Get())
            {
                //Si la liste d'ami d'un joueur contient le joueur déconnecté
                if (user.Friends.Contains(userDisconnected.Id))
                {
                    //On ajoute ce joueur à la liste des joueurs qui ont le joueur déconnecté en ami 
                    listUsersHavingUserDisconnectedAsFriend.Add(user.Username);
                }
            }
        }
    }
}