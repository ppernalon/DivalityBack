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
        private readonly UtilServices _utilServices; 
        
        public AuctionHouseService(AuctionHousesCRUDService auctionHousesCrudService, UtilServices utilServices)
        {
            _auctionHousesCrudService = auctionHousesCrudService;
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
    }
}