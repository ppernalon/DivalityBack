using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DivalityBack.Services
{
    [ExcludeFromCodeCoverage]
    public class WebsocketService
    {
        private readonly UsersService _usersService;
        private readonly CardsService _cardsService;
        private readonly AuctionHouseService _auctionHouseService;

        public WebsocketService(UsersService usersService, CardsService cardsService,
            AuctionHouseService auctionHouseService)
        {
            _usersService = usersService;
            _cardsService = cardsService;
            _auctionHouseService = auctionHouseService;
        }

        public async Task HandleMessages(WebSocket websocket)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    while (websocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                            result = await websocket.ReceiveAsync(messageBuffer, CancellationToken.None);
                            ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                        } while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            HandleDeconnection(websocket);
                            await websocket.CloseOutputAsync(
                                WebSocketCloseStatus.NormalClosure,
                                string.Empty,
                                CancellationToken.None
                            );
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            JsonDocument msgJson = JsonDocument.Parse(ms.ToArray());
                            try
                            {
                                switch (@msgJson.RootElement.GetProperty("type").ToString().Trim())
                                {
                                    case "connection":
                                        await HandleConnection(websocket, result, msgJson);
                                        break;
                                    case "pray":
                                        await HandleGenerateCard(websocket, result, msgJson);
                                        break;
                                    case "collection":
                                        await HandleCollection(websocket, result, msgJson);
                                        break;
                                    case "auctionHouse":
                                        await HandleAuctionHouse(websocket, result, msgJson);
                                        break;
                                    case "sellAuctionHouse":
                                        await HandleSellAuctionHouse(websocket, result, msgJson);
                                        break;
                                    case "buyAuctionHouse":
                                        await HandleBuyAuctionHouse(websocket, result, msgJson);
                                        break;
                                    case "teams":
                                        await HandleTeams(websocket, result, msgJson);
                                        break;
                                    case "modificationTeam":
                                        await HandleModificationTeam(websocket, result, msgJson);
                                        break;
                                    case "sendFriendRequest":
                                        await HandleSendFriendRequest(websocket, result, msgJson);
                                        break;
                                    case "acceptFriendRequest":
                                        await HandleAcceptFriendRequest(websocket, result, msgJson);
                                        break;
                                    case "refuseFriendRequest":
                                        await HandleRefuseFriendRequest(websocket, result, msgJson);
                                        break;
                                    case "deleteFriend":
                                        await HandleDeleteFriend(websocket, result, msgJson);
                                        break;
                                    case "waitForDuel":
                                        HandleWaitForDuel(websocket, result, msgJson);
                                        break;
                                    case "cancelWaitForDuel":
                                        HandleCancelWaitForDuel(websocket, result, msgJson);
                                        break;
                                    case "disciples":
                                        await HandleDisciples(websocket, result, msgJson);
                                        break;
                                    case "pickTeamForDuel":
                                        await PickTeamForDuel(result, msgJson);
                                        break;
                                    case "auctionsByUsername":
                                        await HandleAuctionsByUsername(websocket, result, msgJson);
                                        break;
                                    case "cancelAuction":
                                        await HandleCancelAuction(websocket, result, msgJson);
                                        break;
                                    case "challengeFriend":
                                        await HandleChallengeFriend(websocket, result, msgJson);
                                        break;
                                    case "cancelChallenge":
                                        await HandleCancelChallenge(websocket, result, msgJson);
                                        break;
                                    case "refuseChallenge":
                                        await HandleRefuseChallenge(websocket, result, msgJson);
                                        break;
                                    case "acceptChallenge":
                                        await HandleAcceptChallenge(websocket, result, msgJson);
                                        break;
                                    case "ranking":
                                        await HandleRanking(websocket, result, msgJson);
                                        break; 
                                    case "infoWinRate":
                                        await HandleInfoWinRate(websocket, result, msgJson);
                                        break; 
                                    default:
                                        await websocket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true,
                                            CancellationToken.None);
                                        break;
                                }
                            }
                            catch (KeyNotFoundException e)
                            {
                                Console.WriteLine("ERREUR WS: Message incorrect");
                                Console.WriteLine("Message reçu : " + JsonSerializer.Serialize(msgJson));
                            }
                        }

                        ms.SetLength(0);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                    }

                    if (websocket.State == WebSocketState.CloseReceived)
                    {
                        HandleDeconnection(websocket);
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write("ERREUR WS: " + e.Message);
            }
        }

        private async Task HandleInfoWinRate(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            await _usersService.GetInfoWinRate(websocket, result, username); 
        }

        private async Task HandleRanking(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            await _usersService.GetRanking(websocket, result); 
        }

        private async Task HandleAcceptChallenge(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String usernameChallenged = msgJson.RootElement.GetProperty("usernameChallenged").ToString();
            await _usersService.AcceptChallenge(websocket, result, username, usernameChallenged);
        }

        private async Task HandleRefuseChallenge(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String usernameChallenged = msgJson.RootElement.GetProperty("usernameChallenged").ToString();
            await _usersService.RefuseChallenge(websocket, result, username, usernameChallenged);
        }

        private async Task HandleCancelChallenge(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String usernameChallenged = msgJson.RootElement.GetProperty("usernameChallenged").ToString();
            await _usersService.CancelChallenge(websocket, result, username, usernameChallenged);
        }

        private async Task HandleChallengeFriend(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String usernameChallenged = msgJson.RootElement.GetProperty("usernameChallenged").ToString();
            await _usersService.ChallengeFriend(websocket, result, username, usernameChallenged);
        }

        private async Task HandleCancelAuction(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String cardName = msgJson.RootElement.GetProperty("cardName").ToString();
            String price = msgJson.RootElement.GetProperty("price").ToString();
            String quantity = msgJson.RootElement.GetProperty("quantity").ToString();

            await _auctionHouseService.CancelAuction(websocket, result, username, cardName, price, quantity);
        }

        private async Task HandleAuctionsByUsername(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            await _auctionHouseService.GetAuctionsByUsername(websocket, result, username);
        }


        private async Task HandleDisciples(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            await _usersService.GetDisciples(websocket, result, username);
        }

        private void HandleCancelWaitForDuel(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            _usersService.CancelWaitForDuel(username);
        }

        private void HandleWaitForDuel(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            _usersService.WaitForDuel(websocket, result, username);
        }

        private async Task HandleDeleteFriend(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String usernameFriendToDelete = msgJson.RootElement.GetProperty("usernameFriendToDelete").ToString();
            await _usersService.DeleteFriend(websocket, result, username, usernameFriendToDelete);
        }

        private async Task HandleRefuseFriendRequest(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String usernameSender = msgJson.RootElement.GetProperty("usernameSender").ToString();
            String usernameReceiver = msgJson.RootElement.GetProperty("usernameReceiver").ToString();
            await _usersService.RefuseFriendRequest(websocket, result, usernameSender, usernameReceiver);
        }

        private async Task HandleAcceptFriendRequest(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String usernameSender = msgJson.RootElement.GetProperty("usernameSender").ToString();
            String usernameReceiver = msgJson.RootElement.GetProperty("usernameReceiver").ToString();
            await _usersService.AcceptFriendRequest(websocket, result, usernameSender, usernameReceiver);
        }

        private async Task HandleSendFriendRequest(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String usernameSender = msgJson.RootElement.GetProperty("usernameSender").ToString();
            String usernameReceiver = msgJson.RootElement.GetProperty("usernameReceiver").ToString();
            await _usersService.SendFriendRequest(websocket, result, usernameSender, usernameReceiver);
        }

        private async Task HandleModificationTeam(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String oldNameTeam = msgJson.RootElement.GetProperty("oldNameTeam").ToString();
            String newNameTeam = msgJson.RootElement.GetProperty("newNameTeam").ToString();
            String compo = msgJson.RootElement.GetProperty("compo").ToString();
            await _usersService.ModifyTeam(websocket, result, username, oldNameTeam, newNameTeam, compo);
        }

        private async Task HandleTeams(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            await _usersService.getTeams(websocket, result, username);
        }

        private async Task HandleBuyAuctionHouse(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String cardName = msgJson.RootElement.GetProperty("cardName").ToString();
            String ownerName = msgJson.RootElement.GetProperty("ownerName").ToString();
            String price = msgJson.RootElement.GetProperty("price").ToString();
            String quantity = msgJson.RootElement.GetProperty("quantity").ToString();
            await _auctionHouseService.BuyCardInAuctionHouse(websocket, result, username, cardName, ownerName, price,
                quantity);
        }

        private async Task HandleSellAuctionHouse(WebSocket websocket, WebSocketReceiveResult result,
            JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String cardName = msgJson.RootElement.GetProperty("cardName").ToString();
            String price = msgJson.RootElement.GetProperty("price").ToString();
            String quantity = msgJson.RootElement.GetProperty("quantity").ToString();
            await _auctionHouseService.SellCardInAuctionHouse(websocket, result, username, cardName, price, quantity);
        }

        private async Task HandleAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            await _auctionHouseService.GetAuctionHouse(websocket, result);
        }

        private async Task HandleGenerateCard(WebSocket webSocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String pantheon = msgJson.RootElement.GetProperty("pantheon").ToString();
            await _usersService.Pray(username, pantheon, webSocket, result);
        }

        public async Task HandleConnection(WebSocket webSocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            if (!_usersService.mapActivePlayersWebsocket.ContainsKey(username))
            {
                //On met à jour une map globale des utilisateurs connectés avec leur websocket
                _usersService.mapActivePlayersWebsocket.Add(username, webSocket);

                await _usersService.WarnUsersOfConnection(username, webSocket, result);
            }
            else
            {
                await _usersService.WarnUseAlreadyConnected(webSocket, result, username);
            }
        }

        public async Task HandleCollection(WebSocket webSocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();

            await _usersService.GetCollection(username, webSocket, result);
        }

        private void HandleDeconnection(WebSocket webSocket)
        {
            foreach (var keyValuePair in _usersService.mapActivePlayersWebsocket)
            {
                if (keyValuePair.Value.State == WebSocketState.CloseReceived ||
                    keyValuePair.Value.State == WebSocketState.Closed)
                {
                    _usersService.mapActivePlayersWebsocket.Remove(keyValuePair.Key);
                }

                if (_usersService.mapQueuePlayersWebsocket.Keys.Contains(keyValuePair.Key))
                {
                    _usersService.mapQueuePlayersWebsocket.Remove(keyValuePair.Key);
                }
            }
        }

        private async Task PickTeamForDuel(WebSocketReceiveResult result, JsonDocument msgJson)
        {
            int teamIndex = msgJson.RootElement.GetProperty("teamIndex").GetInt32();
            String username = msgJson.RootElement.GetProperty("username").GetString();
            await _usersService.PickTeamForDuel(username, teamIndex, result);
        }
    }
}