using System;
using System.Collections.Generic;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using MongoDB.Driver;

namespace DivalityBack.Services
{
    public class CardsService
    {
        private readonly CardsCRUDService _cardsCrudService; 
        
        private Dictionary<String, int> mapRarityPurcentage = new Dictionary<String,int>()
        {
            ["commune"]= 75, //75% de chances d'avoir une commune
            ["rare"]=95, //20% de chances d'avoir une rare
            ["legendaire"]=5 //5% de chances d'avoir une légendaire
        };

        public int priceOfCard = 10; 
        
        public CardsService(CardsCRUDService cardsCrudService)
        {
            _cardsCrudService = cardsCrudService; 
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
            Card cardGenerated = listOfCards[new Random().Next(listOfCards.Count)];

            if (cardGenerated.isLimited)
            {
                cardGenerated.Available -= 1;
                _cardsCrudService.Update(cardGenerated.Id, cardGenerated);
            }
            
            return cardGenerated; 
        }
    }
}