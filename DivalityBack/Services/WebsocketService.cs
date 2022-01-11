using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Services;

namespace DivalityBack.Services
{
    [ExcludeFromCodeCoverage]
    public class WebsocketService
    {
        private readonly UsersService _usersService;
        private readonly CardsService _cardsService;
        private readonly AuctionHouseService _auctionHouseService; 
        
        public WebsocketService(UsersService usersService, CardsService cardsService, AuctionHouseService auctionHouseService)
        {
            _usersService = usersService;
            _cardsService = cardsService;
            _auctionHouseService = auctionHouseService; 
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
                                default:
                                    await websocket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;
                            }
                        }
                        ms.SetLength(0);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                    }

                    if (websocket.State == WebSocketState.CloseReceived)
                    {
                        await HandleDeconnection(websocket);
                    }
                }
            } catch (InvalidOperationException e) {
                Console.Write("ERREUR WS: " + e.Message);
            }
        }

        private async Task HandleBuyAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String cardName = msgJson.RootElement.GetProperty("cardName").ToString();
            String ownerName = msgJson.RootElement.GetProperty("ownerName").ToString();
            String price = msgJson.RootElement.GetProperty("price").ToString();
            await _auctionHouseService.BuyCardInAuctionHouse(websocket, result, username, cardName, ownerName, price); 
        }

        private async Task HandleSellAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();
            String cardName = msgJson.RootElement.GetProperty("cardName").ToString();
            String price = msgJson.RootElement.GetProperty("price").ToString();
            await _auctionHouseService.SellCardInAuctionHouse(websocket, result, username, cardName, price);
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

            //On met à jour une map globale des utilisateurs connectés avec leur websocket
            _usersService.mapActivePlayersWebsocket.Add(username, webSocket);

            await _usersService.WarnUsersOfConnection(username,webSocket, result);
         
        }

        public async Task HandleCollection(WebSocket webSocket, WebSocketReceiveResult result, JsonDocument msgJson)
        {
            String username = msgJson.RootElement.GetProperty("username").ToString();

            await _usersService.GetCollection(username, webSocket, result); 
        }
        
        private async Task HandleDeconnection(WebSocket webSocket)
        {
            Console.Write("ok");
        }
        
        
    }
}