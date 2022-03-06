using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
        private readonly UsersService _usersService;
        private readonly CardsService _cardsService; 
        
        public AuctionHouseService(AuctionHousesCRUDService auctionHousesCrudService, CardsCRUDService cardsCrudService, UsersCRUDService usersCrudService, UtilServices utilServices, UsersService usersService, CardsService cardsService)
        {
            _auctionHousesCrudService = auctionHousesCrudService;
            _cardsCrudService = cardsCrudService;
            _usersCrudService = usersCrudService;
            _utilServices = utilServices;
            _usersService = usersService;
            _cardsService = cardsService; 
        }

        [ExcludeFromCodeCoverage]
        public async Task GetAuctionHouse(WebSocket websocket, WebSocketReceiveResult result)
        {
            List<AuctionHouse> listAuctionHouse = _auctionHousesCrudService.Get();
            String jsonAuctionHouse = _utilServices.ListAuctionToJson(listAuctionHouse);

            await WarnUserOfAuctionHouse(websocket, result, jsonAuctionHouse);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserOfAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string jsonAuctionHouse)
        {
            byte[] byteAuctionHouse = Encoding.UTF8.GetBytes(jsonAuctionHouse);
            await websocket.SendAsync(byteAuctionHouse, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }

        public async Task SellCardInAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string username, string cardName, string price, string quantity)
        {
            Card card = _cardsCrudService.GetCardByName(cardName);
            User user = _usersCrudService.GetByUsername(username);
            
            //On vérifie que l'utilisateur possède la bonne quantité de la carte mise en vente, les cartes mises
            // en vente dans l'HdV sont retirées de la collection
            
            int quantityInCollection = user.Collection.Where(c => c.Equals(card.Id)).Count();

            if (card != null && quantityInCollection >= int.Parse(quantity))
            {

                for (int i = 0; i < int.Parse(quantity); i++)
                {
                    //On enlève les cartes de la collection
                    user.Collection.Remove(card.Id);
                    
                    //On rajoute les ventes dans l'HdV
                    AuctionHouse auction = new AuctionHouse();
                    auction.Price = int.Parse(price);
                    auction.CardId = card.Id;
                    auction.OwnerId = user.Id;
                    _auctionHousesCrudService.Create(auction); 
                }
                
                _usersCrudService.Update(user.Id, user);
                await GetAuctionHouse(websocket, result);
            }
            else
            {
                string messageErreur = "L'utilisateur ne possède pas assez d'exemplaires de cette carte";
                await websocket.SendAsync(Encoding.UTF8.GetBytes(messageErreur), result.MessageType,
                    result.EndOfMessage, CancellationToken.None); 
            }
        }

        public async Task BuyCardInAuctionHouse(WebSocket websocket, WebSocketReceiveResult result, string username, string cardName, string ownerName, string price, string quantity)
        {
            Card card = _cardsCrudService.GetCardByName(cardName);
            User user = _usersCrudService.GetByUsername(username);
            User owner = _usersCrudService.GetByUsername(ownerName);

            if (!(user.Disciples < int.Parse(price) * int.Parse(quantity)))
            {
                List<AuctionHouse> auctions =
                    _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice(card.Id, owner.Id, price);
                if (int.Parse(quantity) <= auctions.Count)
                {


                    for (int i = 0; i < int.Parse(quantity); i++)
                    {
                        AuctionHouse auction = auctions[i];
                        _auctionHousesCrudService.Remove(auction);
                        owner.Disciples += int.Parse(price);
                        user.Disciples -= int.Parse(price);
                        user.Collection.Add(card.Id);
                    }

                    _usersCrudService.Update(user.Id, user);
                    _usersCrudService.Update(owner.Id, owner);

                    await GetAuctionHouse(websocket, result);
                }
                else
                {
                    string messageErreur = "La quantité demandée est supérieure à la quantité mise en vente";
                    await websocket.SendAsync(Encoding.UTF8.GetBytes(messageErreur), result.MessageType,
                        result.EndOfMessage, CancellationToken.None);
                }
            }
            else
                {
                    string messageErreur = "L'utilisateur ne possède pas assez de disciples";
                    await websocket.SendAsync(Encoding.UTF8.GetBytes(messageErreur), result.MessageType,
                        result.EndOfMessage, CancellationToken.None);
                }
            }

        public async Task GetAuctionsByUsername(WebSocket websocket, WebSocketReceiveResult result, string username)
        {
            User user = _usersCrudService.GetByUsername(username);
            if (user != null)
            {
                List<AuctionHouse> auctions = _auctionHousesCrudService.getByOwnerId(user.Id);
                String jsonAuctions = _utilServices.AuctionsToJson(auctions);

                await WarnUserOfAuctionsByUsername(websocket, result, jsonAuctions);
            }
            else
            {
                await _usersService.WarnUserNotFound(websocket, result);
            }
        }
        
        private async Task WarnUserOfAuctionsByUsername(WebSocket websocket, WebSocketReceiveResult result, string jsonAuctions)
        {
            byte[] byteAuctions = Encoding.UTF8.GetBytes(jsonAuctions);
            await websocket.SendAsync(byteAuctions, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }

        public async Task CancelAuction(WebSocket websocket, WebSocketReceiveResult result, string username, string cardName, string price, string quantity)
        {
            User user = _usersCrudService.GetByUsername(username);
            if (user != null)
            {
                Card card = _cardsCrudService.GetCardByName(cardName);
                if (card != null)
                {
                    List<AuctionHouse> auctions = _auctionHousesCrudService.getByOwnerId(user.Id);
                    List<AuctionHouse> auctionsToRemove = auctions.FindAll(auction =>
                        auction.CardId.Equals(card.Id) && auction.Price.Equals(int.Parse(price)));
                    if (int.Parse(quantity) <= auctionsToRemove.Count)
                    {
                        for (int i = 0; i < int.Parse(quantity); i++)
                        {
                            AuctionHouse auctionToRemove = auctionsToRemove[i];
                            _auctionHousesCrudService.Remove(auctionToRemove);
                            user.Collection.Add(auctionsToRemove[i].CardId);
                        }
                    }
                    else
                    {
                        foreach (AuctionHouse auctionToRemove in auctionsToRemove)
                        {
                            _auctionHousesCrudService.Remove(auctionToRemove);
                            user.Collection.Add(auctionToRemove.CardId);
                        }
                    }
                    _usersCrudService.Update(user.Id, user);
                    String jsonAuctionHouse = _utilServices.ListAuctionToJson(_auctionHousesCrudService.Get());
                    await WarnUserOfAuctionHouse(websocket, result, jsonAuctionHouse);
                }
                else
                {
                    await _cardsService.WarnCardNotFound(websocket, result);
                }

            }
            else
            {
                await _usersService.WarnUserNotFound(websocket, result);
            }
        }
    }
}