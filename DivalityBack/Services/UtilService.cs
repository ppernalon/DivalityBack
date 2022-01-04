using System;
using System.Collections.Generic;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace Divality.Services
{
    public class UtilServices
    {
        private readonly CardsCRUDService _cardsCrudService;

        public UtilServices(CardsCRUDService cardsCrudService)
        {
            _cardsCrudService = cardsCrudService; 
        }
        
        public string CollectionToJson(List<String> collection)
        {
            String jsonCollection = "";
            jsonCollection += "{";
            jsonCollection += "\"type\":\"collection\",";
            foreach (string cardId in collection)
            {
                jsonCollection += "\"card\":";
                jsonCollection += CardToJson(_cardsCrudService.Get(cardId)) + ",";
            }

            jsonCollection = jsonCollection.Remove(jsonCollection.Length -1); 

            jsonCollection += "}";
            return jsonCollection; 
        }

        public string CardToJson(Card card)
        {
            String jsonCard = "";
            jsonCard += "{";
            jsonCard += "\"type\":\"card\",";
            jsonCard += "\"name\":\"" + card.Name + "\",";
            jsonCard += "\"pantheon\":\"" + card.Pantheon + "\",";
            jsonCard += "\"rarity\":\"" + card.Rarity + "\",";
            jsonCard += "\"life\":\"" + card.Life + "\",";
            jsonCard += "\"speed\":\"" + card.Speed + "\",";
            jsonCard += "\"power\":\"" + card.Power + "\",";
            jsonCard += "\"armor\":\"" + card.Armor + "\",";
            jsonCard += "\"offensiveAbility\":\"" + card.OffensiveAbility + "\",";
            jsonCard += "\"offensiveEfficiency\":\"" + card.OffensiveEfficiency + "\",";
            jsonCard += "\"defensiveAbility\":\"" + card.DefensiveAbility + "\",";
            jsonCard += "\"defensiveEfficiency\":\"" + card.DefensiveEfficiency + "\",";
            jsonCard += "\"isLimited\":\"" + card.isLimited + "\",";
            jsonCard += "\"distributed\":\"" + card.Distributed + "\",";
            jsonCard += "\"available\":\"" + card.Available + "\"";
            jsonCard += "}";

            return jsonCard; 

        }

        public String FriendsToJson(List<String> listUsernameConnected, List<String> listUsernameDisconnected)
        {
            String jsonUsernames = "";
            jsonUsernames += "{";
                jsonUsernames += "\"type\":\"friends\",";
                jsonUsernames += "\"connected\":";
                jsonUsernames += ListUsernameToJson(listUsernameConnected);
                jsonUsernames += ",";
                jsonUsernames += "\"disconnected\":";
                jsonUsernames += ListUsernameToJson(listUsernameDisconnected);
            jsonUsernames += "}";
            return jsonUsernames; 
        }

        private string ListUsernameToJson(List<string> listUsername)
        {
            String jsonUsernames = "{"; 
            if (listUsername.Count > 0){
                foreach (String username in listUsername)
                {
                    jsonUsernames += "\"username\":\"" + username + "\",";
                }
                jsonUsernames = jsonUsernames.Remove(jsonUsernames.Length -1);
            }
            jsonUsernames += "}"; 
            return jsonUsernames; 
        }
    }
}