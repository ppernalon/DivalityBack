using System;
using System.Collections.Generic;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace DivalityBack.Services
{
    public class UtilServices
    {
        private readonly CardsCRUDService _cardsCrudService;
        private readonly UsersCRUDService _usersCrudService;
        public UtilServices(CardsCRUDService cardsCrudService, UsersCRUDService usersCrudService)
        {
            _cardsCrudService = cardsCrudService;
            _usersCrudService = usersCrudService; 
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

        public string AuctionToJson(AuctionHouse auction)
        {
            String cardName = _cardsCrudService.Get(auction.CardId).Name;
            String ownerName = _usersCrudService.Get(auction.OwnerId).Username;
            
            String jsonAuction = "";
            jsonAuction += "{";
            jsonAuction += "\"type\":\"auction\",";
            jsonAuction += "\"cardName\": \"" + cardName + "\",";
            jsonAuction += "\"ownerName\": \"" + ownerName + "\",";
            jsonAuction += "\"price\":\"" + auction.Price + "\""; 
            jsonAuction += "}";
            return jsonAuction;
        }

        public string ListAuctionToJson(List<AuctionHouse> listAuctionHouse)
        {
            String jsonListAuctionHouse = "";

            jsonListAuctionHouse += "{";
            jsonListAuctionHouse += "\"type\":\"auctionHouse\",";
            foreach (AuctionHouse auction in listAuctionHouse)
            {
                jsonListAuctionHouse += "\"auction\" :";
                jsonListAuctionHouse += AuctionToJson(auction) + ",";
            }
            jsonListAuctionHouse = jsonListAuctionHouse.Remove(jsonListAuctionHouse.Length - 1);

            jsonListAuctionHouse += "}";

            return jsonListAuctionHouse; 
        }

        public string TeamsToJson(List<Team> teams)
        {
            String jsonTeams = "";

            jsonTeams += "{";
            jsonTeams += "\"type\":\"teams\",";
            foreach (Team team in teams)
            {
                jsonTeams += "\"team\" :";
                jsonTeams += TeamToJson(team) + ","; 
            }

            jsonTeams = jsonTeams.Remove(jsonTeams.Length - 1);
            jsonTeams += "}";

            return jsonTeams;
        }

        private string TeamToJson(Team team)
        {
            String jsonTeam = "";

            jsonTeam += "{";
            jsonTeam += "\"type\":\"team\",";
            jsonTeam += "\"name\":\"" + team.Name + "\",";
            jsonTeam += "\"compo\":";
            foreach (string cardId in team.Compo)
            {
                Card card = _cardsCrudService.Get(cardId);
                jsonTeam += CardToJson(card) + ",";
            }

            jsonTeam = jsonTeam.Remove(jsonTeam.Length - 1);
            jsonTeam += "}";

            return jsonTeam; 
        }
    }
}