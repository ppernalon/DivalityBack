using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace DivalityBack.Services
{
    public class AuctionHouseService
    {
        private readonly AuctionHousesCRUDService _auctionHousesCrudService;
        private readonly CardsCRUDService _cardsCrudService;
        private readonly UsersCRUDService _usersCrudService; 
        private readonly UtilServices _utilServices; 
        
        public AuctionHouseService(AuctionHousesCRUDService auctionHousesCrudService, CardsCRUDService cardsCrudService, UsersCRUDService usersCrudService, UtilServices utilServices)
        {
            _auctionHousesCrudService = auctionHousesCrudService;
            _cardsCrudService = cardsCrudService;
            _usersCrudService = usersCrudService;
            _utilServices = utilServices; 
        }

        [ExcludeFromCodeCoverage]
        public async Task GetAuctionHouse(WebSocket websocket, WebSocketReceiveResult result)
        {
            List<AuctionHouse> listAuctionHouse = _auctionHousesCrudService.Get();
            String jsonAuctionHouse = _utilServices.listAuctionToJson(listAuctionHouse);

            await WarnUserOfAuctionHouse(websocket, result, jsonAuctionHouse);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserOfAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string jsonAuctionHouse)
        {
            byte[] byteAuctionHouse = Encoding.UTF8.GetBytes(jsonAuctionHouse);
            await websocket.SendAsync(byteAuctionHouse, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }

        public async Task SellCardInAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string username, string cardName, string price)
        {
            Card card = _cardsCrudService.GetCardByName(cardName);
            User user = _usersCrudService.GetByUsername(username);

            //On vérifie que l'utilisateur possède la carte
            if (card != null && user.Collection.Contains(card.Id))
            {
                //On enlève la carte de la collection
                user.Collection.Remove(card.Id);
                _usersCrudService.Update(user.Id, user);
                
                //On rajoute la vente dans l'HdV
                AuctionHouse auction = new AuctionHouse();
                auction.Price = int.Parse(price);
                auction.CardId = card.Id;
                auction.OwnerId = user.Id;
                _auctionHousesCrudService.Create(auction); 
                
                await GetAuctionHouse(websocket, result);
            }
            else
            {
                string messageErreur = "L'utilisateur ne possède pas cette carte";
                await websocket.SendAsync(Encoding.UTF8.GetBytes(messageErreur), result.MessageType,
                    result.EndOfMessage, CancellationToken.None); 
            }
        }

        public async Task BuyCardInAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string username, string cardName, string ownerName, string price)
        {
            Card card = _cardsCrudService.GetCardByName(cardName);
            User user = _usersCrudService.GetByUsername(username);
            User owner = _usersCrudService.GetByUsername(ownerName);

            if (!(user.Disciples < int.Parse(price)))
            {
                AuctionHouse auction = _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice(card.Id, owner.Id, price);
                _auctionHousesCrudService.Remove(auction);

                owner.Disciples += int.Parse(price);

                user.Disciples -= int.Parse(price);
                user.Collection.Add(card.Id);
                _usersCrudService.Update(user.Id, user);
                _usersCrudService.Update(owner.Id, owner);
                
                await GetAuctionHouse(websocket, result);

            }
            else
            {
                string messageErreur = "L'utilisateur ne possède pas assez de disciples";
                await websocket.SendAsync(Encoding.UTF8.GetBytes(messageErreur), result.MessageType,
                    result.EndOfMessage, CancellationToken.None); 
            }
        }
    }
}