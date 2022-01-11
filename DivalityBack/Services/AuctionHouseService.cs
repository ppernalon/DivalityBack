using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace Divality.Services
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

        public async Task GetAuctionHouse(WebSocket websocket, WebSocketReceiveResult result)
        {
            List<AuctionHouse> listAuctionHouse = _auctionHousesCrudService.Get();
            String jsonAuctionHouse = _utilServices.listAuctionToJson(listAuctionHouse);

            WarnUserOfAuctionHouse(websocket, result, jsonAuctionHouse);
        }

        private void WarnUserOfAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string jsonAuctionHouse)
        {
            byte[] byteAuctionHouse = Encoding.UTF8.GetBytes(jsonAuctionHouse);
            websocket.SendAsync(byteAuctionHouse, result.MessageType, result.EndOfMessage, CancellationToken.None); 
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
    }
}