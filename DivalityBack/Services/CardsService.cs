using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using MongoDB.Driver;

namespace DivalityBack.Services
{
    public class CardsService
    {
        private readonly CardsCRUDService _cardsCrudService;
        private readonly UtilServices _utilServices; 
        
        private Dictionary<String, int> mapRarityPurcentage = new Dictionary<String,int>()
        {
            ["commune"]= 75, //75% de chances d'avoir une commune
            ["rare"]=95, //20% de chances d'avoir une rare
            ["legendaire"]=100 //5% de chances d'avoir une légendaire
        };

        public int priceOfCard = 10; 
        
        public CardsService(CardsCRUDService cardsCrudService, UtilServices utilServices)
        {
            _cardsCrudService = cardsCrudService;
            _utilServices = utilServices; 
        }

        public String GenerateRarity()
        {
            int randomNumberRarity = new Random().Next(1, 101);
            if (randomNumberRarity <= mapRarityPurcentage["commune"])
            {
                return "commune";
            }
            if (randomNumberRarity <= mapRarityPurcentage["rare"])
            {
                return "rare";
            }
            return "legendaire";
        }

        public Card GenerateNewCard(string pantheon)
        {
            String rarity = GenerateRarity();
            List<Card> listOfCards = _cardsCrudService.GetCardsByPantheonAndRarity(pantheon, rarity);
            if (listOfCards.Count > 0 )
            {
                Card cardGenerated = listOfCards[new Random().Next(listOfCards.Count)];

                if (cardGenerated.isLimited)
                {
                    cardGenerated.Distributed += 1;
                    _cardsCrudService.Update(cardGenerated.Id, cardGenerated);
                }

                return cardGenerated;
            }
            return null; 
        }

        public async Task WarnCardNotFound(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(_utilServices.CardNotFoundToJson());
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task GetCard(WebSocket websocket, WebSocketReceiveResult result, string cardName)
        {
            Card card = _cardsCrudService.GetCardByName(cardName);
            if (card == null)
            {
                await WarnCardNotFound(websocket, result);
            }
            else
            {
                String jsonCard = _utilServices.CardToJson(card);
                await WarnCard(websocket, result, jsonCard); 
            }
        }

        private async Task WarnCard(WebSocket websocket, WebSocketReceiveResult result, string jsonCard)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonCard);
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}