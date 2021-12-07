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
        private List<User> listActivePlayers = new List<User>();
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
            String username = msgJson.RootElement.GetProperty("username").ToString();
            //On met à jour une liste globale des utilisateurs connectés
            User userConnected =
                _usersCrudService.GetByUsername(username);
            listActivePlayers.Add(userConnected);

            List<User> listFriendsUser = new List<User>();
            listFriendsUser = _usersCrudService.GetUsersById(userConnected.Friends);
            List<String> listUsernameFriendsUser = listFriendsUser.Select(s => s.Username).ToList();
            
            //On renvoie à l'utilisateur la liste de ses amis connectés
            
            
            byte[] dataAsBytes = listUsernameFriendsUser.SelectMany(s =>
                Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();
            
            await webSocket.SendAsync(dataAsBytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}