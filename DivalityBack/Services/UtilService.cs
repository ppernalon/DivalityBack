using System;
using System.Collections.Generic;
using System.Linq;
using DivalityBack.Models;
using DivalityBack.Models.Gods;
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
           
            List<String> listGreek = new List<string>();
            List<String> listEgyptian = new List<string>();
            List<String> listNordic = new List<string>();
            foreach (string cardId in collection)
            {
                Card card = _cardsCrudService.Get(cardId);
                switch (card.Pantheon)
                {
                    case "greek":
                        listGreek.Add(card.Name);
                        break;
                    case "egyptian":
                        listEgyptian.Add(card.Name);
                        break;
                    case "nordic":
                        listNordic.Add(card.Name);
                        break;
                }
            }
            
            jsonCollection += "\"data\" :{ ";
            
            jsonCollection += "\"greek\" :" + "[";
            foreach (string name in listGreek)
            {
                jsonCollection += "\"" + name + "\"" + ",";
            }

            if (jsonCollection.EndsWith(","))
            {
                jsonCollection = jsonCollection.Remove(jsonCollection.Length - 1);
            }

            jsonCollection += "],";
            
            jsonCollection += "\"egyptian\" :" + "[";
            foreach (string name in listEgyptian)
            {
                jsonCollection += "\"" + name + "\"" + ",";
            }
            if (jsonCollection.EndsWith(","))
            {
                jsonCollection = jsonCollection.Remove(jsonCollection.Length - 1);
            }            jsonCollection += "],"; 
            
            jsonCollection += "\"nordic\" :" + "[";
            foreach (string name in listNordic)
            {
                jsonCollection += "\"" + name + "\"" + ",";
            }
            if (jsonCollection.EndsWith(","))
            {
                jsonCollection = jsonCollection.Remove(jsonCollection.Length - 1);
            }            jsonCollection += "]"; 
           
            jsonCollection += "}"; 
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

        public string FriendsToJson(List<string> listUsernameConnected, List<string> listUsernameDisconnected,
            List<string> listSenderOfFriendRequests)
        {
            String jsonUsernames = "";
            jsonUsernames += "{";
                jsonUsernames += "\"type\":\"friends\",";
                jsonUsernames += "\"connected\":";
                jsonUsernames += ListUsernameToJson(listUsernameConnected);
                jsonUsernames += ",";
                jsonUsernames += "\"disconnected\":";
                jsonUsernames += ListUsernameToJson(listUsernameDisconnected);
                jsonUsernames += ",";
                jsonUsernames += "\"requests\":";
                jsonUsernames += ListUsernameToJson(listSenderOfFriendRequests);
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

        public string AuctionToJson(String cardId, String ownerId, string price, string quantity)
        {
            String cardName = _cardsCrudService.Get(cardId).Name;
            String ownerName = _usersCrudService.Get(ownerId).Username;
            
            String jsonAuction = "";
            jsonAuction += "{";
            jsonAuction += "\"cardName\": \"" + cardName + "\",";
            jsonAuction += "\"ownerName\": \"" + ownerName + "\",";
            jsonAuction += "\"price\":\"" + price + "\","; 
            jsonAuction += "\"quantity\":\"" + quantity + "\"";
            jsonAuction += "}";
            return jsonAuction;
        }


        public string ListAuctionToJson(List<AuctionHouse> listAuctionHouse)    
        {
            String jsonListAuctionHouse = "";

            jsonListAuctionHouse += "{";
            jsonListAuctionHouse += "\"type\":\"auctionHouse\",";
            jsonListAuctionHouse += "\"shopData\":[";

            //Regroupement des ventes de l'HdV par Owner, Card et Price
            var groupedAuctions = listAuctionHouse
                .GroupBy(auction => new {auction.OwnerId, auction.CardId, auction.Price}).Select(a => new
                {
                    OwnerId = a.Key.OwnerId,
                    CardId = a.Key.CardId,
                    Price = a.Key.Price,
                    Quantity = a
                }).ToList();
            
            foreach (var groupedAuction in groupedAuctions)
            {
                jsonListAuctionHouse += AuctionToJson(groupedAuction.CardId, groupedAuction.OwnerId, groupedAuction.Price.ToString(), groupedAuction.Quantity.Count().ToString()) + ",";
            }
            
            jsonListAuctionHouse = jsonListAuctionHouse.Remove(jsonListAuctionHouse.Length - 1);

            jsonListAuctionHouse += "]}";

            return jsonListAuctionHouse; 
        }
        
        public string TeamsToJson(List<Team> teams)
        {
            String jsonTeams = "";

            jsonTeams += "{";
            jsonTeams += "\"type\":\"teams\",";
            jsonTeams += "\"teamsdata\": [";
            
            foreach (Team team in teams)
            {
                jsonTeams += "{";
                jsonTeams += "\"name\" : \"" + team.Name + "\",";
                jsonTeams += "\"compo\" : [";
                foreach (string cardId in team.Compo)
                {
                    jsonTeams += "\"" + _cardsCrudService.Get(cardId).Name + "\",";
                }

                if (jsonTeams.EndsWith(","))
                {
                    jsonTeams = jsonTeams.Remove(jsonTeams.Length - 1);
                }
                jsonTeams += "]";
                jsonTeams += "},";
            }
            if(jsonTeams.Substring(jsonTeams.Length - 1) == ","){
                jsonTeams = jsonTeams.Remove(jsonTeams.Length - 1);
            }
            jsonTeams += "]"; 
            jsonTeams += "}";

            return jsonTeams;
        }

        public string TeamToJson(Team team)
        {
            String jsonTeam = "";

            jsonTeam += "{";
            jsonTeam += "\"type\":\"team\",";
            jsonTeam += "\"name\":\"" + team.Name + "\",";
            jsonTeam += "\"compo\": {";
            foreach (string cardId in team.Compo)
            {
                jsonTeam += "\"card\":";
                Card card = _cardsCrudService.Get(cardId);
                jsonTeam += CardToJson(card) + ",";
            }

            jsonTeam = jsonTeam.Remove(jsonTeam.Length - 1);
            jsonTeam += "}";
            jsonTeam += "}";

            return jsonTeam; 
        }

        public string DuelToJson(string username)
        {
            String jsonDuel = "";

            jsonDuel += "{";
            jsonDuel += "\"type\":\"duel\",";
            jsonDuel += "\"opponent\":\"" + username + "\"";
            jsonDuel += "}";

            return jsonDuel; 
        }

        public string DisciplesToJson(int disciples)
        {
            String jsonDisciples = "";

            jsonDisciples += "{";
            jsonDisciples += "\"type\":\"disciples\",";
            jsonDisciples += "\"disciples\":\"" + disciples + "\"";
            jsonDisciples += "}";

            return jsonDisciples;         
        }

        public string GodListToJson(List<string> namesOfGods)
        {
            string jsonGodList = "";

            jsonGodList += "{";
            jsonGodList += "\"type\":\"opponentPickedTeam\",";
            jsonGodList += "\"opponentGods\": [";
            for (int index = 0; index < 6; index++)
            {
                jsonGodList += "\"" + namesOfGods[index] + "\"";
                if (index < 5) jsonGodList += ",";
            }
            jsonGodList += "]}";
            
            return jsonGodList;
        }       
        
        public string AuctionsToJson(List<AuctionHouse> auctions)
        {
            String jsonAuctions = "";

            jsonAuctions += "{";

            jsonAuctions += "\"type\" : \"auctions\",";
            jsonAuctions += "\"auctionsData\" : [";
            
            //Regroupement des ventes de l'HdV par Owner, Card et Price
            var groupedAuctions = auctions
                .GroupBy(auction => new {auction.OwnerId, auction.CardId, auction.Price}).Select(a => new
                {
                    OwnerId = a.Key.OwnerId,
                    CardId = a.Key.CardId,
                    Price = a.Key.Price,
                    Quantity = a
                }).ToList();
            
            foreach (var groupedAuction in groupedAuctions)
            {
                jsonAuctions += AuctionToJson(groupedAuction.CardId, groupedAuction.OwnerId, groupedAuction.Price.ToString(), groupedAuction.Quantity.Count().ToString()) + ",";
            }
            if (jsonAuctions.EndsWith(","))
            {
                jsonAuctions = jsonAuctions.Remove(jsonAuctions.Length - 1); 
            }
            jsonAuctions += "]";
            jsonAuctions += "}";

            return jsonAuctions;
        }

        public string WinnerJson()
        {
            string winnerJson = "{";
            winnerJson += "\"type\":\"duelWinner\",";
            winnerJson += "\"rewards\":\"300\"";
            winnerJson += "}";
            return winnerJson;
        }
        
        public string LooserJson()
        {
            string looserJson = "{";
            looserJson += "\"type\":\"duelLooser\",";
            looserJson += "\"rewards\":\"150\"";
            looserJson += "}";
            return looserJson;
        }

        public static string UpdateDuelJson(List<GenericGod> godsAttacked, string attacker, int[][] attackPattern)
        {
            string updateDuelJson = "{";
            
            updateDuelJson += "\"type\":\"updatingDuelState\",";
            updateDuelJson += "\"offensivePlayer\":\"" + attacker + "\",";
            updateDuelJson += "\"updatedAttackedGod\": [";
            
            for (int index = 0; index < godsAttacked.Count; index++)
            {
                GenericGod god = godsAttacked[index];
                updateDuelJson += "{\"" + god.Name + "\": " + god.Life + "}";
                if (index != godsAttacked.Count - 1) updateDuelJson += ",";
            }
            
            updateDuelJson += "],";
            updateDuelJson += "\"attackPattern\": [";
            
            for (int index = 0; index < attackPattern.Length; index++)
            {
                int row = attackPattern[index][0];
                int col = attackPattern[index][1];
                
                updateDuelJson += "[" + row + "," + col + "]";
                if (index != attackPattern.Length - 1) updateDuelJson += ",";
            }

            updateDuelJson += "]}";
            
            return updateDuelJson;
        }
    }
}