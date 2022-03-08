using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
            
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "collection");
            
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

            Dictionary<string, Object> data = new Dictionary<string, object>();
            data.Add("greek", listGreek);
            data.Add("nordic", listNordic);
            data.Add("egyptian", listEgyptian);
            
            dictRes.Add("data", data);
            
            
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString; 
        }

        public string CardToJson(Card card)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "card");
            
            dictRes.Add("name", card.Name);
            dictRes.Add("pantheon", card.Pantheon);
            dictRes.Add("rarity", card.Rarity);
            dictRes.Add("life", card.Life);
            dictRes.Add("speed", card.Speed);
            dictRes.Add("power", card.Power);
            dictRes.Add("armor", card.Armor);
            dictRes.Add("offensiveAbility", card.OffensiveAbility);
            dictRes.Add("offensiveEfficiency", card.OffensiveEfficiency);
            dictRes.Add("defensiveAbility", card.DefensiveAbility);
            dictRes.Add("defensiveEfficiency", card.DefensiveEfficiency);
            dictRes.Add("isLimited", card.isLimited);
            dictRes.Add("distributed", card.Distributed);
            dictRes.Add("available", card.Available);

            
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string FriendsToJson(List<string> listUsernameConnected, List<string> listUsernameDisconnected,
            List<string> listSenderOfFriendRequests)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "friends");

            List<Dictionary<String, String>> listDictConnected = new List<Dictionary<string, string>>();
            List<Dictionary<String, String>> listDictDisconnected = new List<Dictionary<string, string>>();
            List<Dictionary<String, String>> listDictRequest = new List<Dictionary<string, string>>();

            //Gestion des amis connectés
            foreach (string username in listUsernameConnected)
            {
                User user = _usersCrudService.GetByUsername(username);
                Dictionary<String, String> dictConnected = new Dictionary<string, string>();
                
                dictConnected.Add("username", username);
                dictConnected.Add("victory", user.Victory.ToString());
                dictConnected.Add("defeat", user.Defeat.ToString());
                listDictConnected.Add(dictConnected);
            }
            
            //Gestion des amis déconnectés
            foreach (string username in listUsernameDisconnected)
            {
                User user = _usersCrudService.GetByUsername(username);
                Dictionary<String, String> dictDisconnected = new Dictionary<string, string>();
                
                dictDisconnected.Add("username", username);
                dictDisconnected.Add("victory", user.Victory.ToString());
                dictDisconnected.Add("defeat", user.Defeat.ToString());
                listDictDisconnected.Add(dictDisconnected);
            }
            
            //Gestion des requêtes d'ami
            foreach (string username in listSenderOfFriendRequests)
            {
                User user = _usersCrudService.GetByUsername(username);
                Dictionary<String, String> dictRequest = new Dictionary<string, string>();
                
                dictRequest.Add("username", username);
                dictRequest.Add("victory", user.Victory.ToString());
                dictRequest.Add("defeat", user.Defeat.ToString());
                listDictRequest.Add(dictRequest);
            }
            
            dictRes.Add("connected", listDictConnected);
            dictRes.Add("disconnected", listDictDisconnected);
            dictRes.Add("request", listDictRequest);

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string AuctionToJson(String cardId, String ownerId, string price, string quantity)
        {
            String cardName = _cardsCrudService.Get(cardId).Name;
            String ownerName = _usersCrudService.Get(ownerId).Username;
            
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            
            dictRes.Add("cardName", cardName);
            dictRes.Add("ownerName", ownerName);
            dictRes.Add("price", price);
            dictRes.Add("quantity", quantity);

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;            
        }


        public string ListAuctionToJson(List<AuctionHouse> listAuctionHouse)    
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "auctionHouse");
            

            //Regroupement des ventes de l'HdV par Owner, Card et Price
            var groupedAuctions = listAuctionHouse
                .GroupBy(auction => new {auction.OwnerId, auction.CardId, auction.Price}).Select(a => new
                {
                    ownerName = _usersCrudService.Get(a.Key.OwnerId).Username,
                    cardName = _cardsCrudService.Get(a.Key.CardId).Name,
                    price = a.Key.Price,
                    quantity = a.Count()
                }).ToList();
            
            dictRes.Add("shopData", groupedAuctions);

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }
        
        public string TeamsToJson(List<Team> teams)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "teams");
            
            List<Dictionary<String, Object>> listTeams = new List<Dictionary<string, object>>();

            foreach (Team team in teams)
            {
                Dictionary<String, Object> dictTeam = new Dictionary<string, object>();
                dictTeam.Add("name", team.Name);
                dictTeam.Add("compo", _cardsCrudService.Get(team.Compo).Select(c => c.Name).ToList());
                
                listTeams.Add(dictTeam);
            }
            dictRes.Add("teamsdata", listTeams);
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }
        
        public string DuelToJson(string username)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "duel");
            
            dictRes.Add("opponent", username);
            
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string DisciplesToJson(int disciples)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "disciples");
            
            dictRes.Add("disciples", disciples);
  
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string GodListToJson(List<string> namesOfGods)
        {
            
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "opponentPickedTeam");
            
            while (namesOfGods.Count < 6)
            {
                namesOfGods.Add("");
            }
            dictRes.Add("opponentGods", namesOfGods);
            
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
            
        }       
        
        public string AuctionsToJson(List<AuctionHouse> auctions)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "auctions");
            
            //Regroupement des ventes de l'HdV par Owner, Card et Price
            var groupedAuctions = auctions
                .GroupBy(auction => new {auction.OwnerId, auction.CardId, auction.Price}).Select(a => new
                {
                    ownerName = _usersCrudService.Get(a.Key.OwnerId).Username,
                    cardName = _cardsCrudService.Get(a.Key.CardId).Name,
                    price = a.Key.Price,
                    quantity = a.Count()
                }).ToList();

            dictRes.Add("auctionsData", groupedAuctions);

              
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string WinnerJson()
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "duelWinner");

            dictRes.Add("rewards", "300");

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }
        
        public string LooserJson()
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "duelLooser");

            dictRes.Add("rewards", "300");

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public static string UpdateDuelJson(List<GenericGod> godsAttacked, string attacker, int[][] attackPattern, int turn, int attackerPosition)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "updatingDuelState");
            dictRes.Add("offensivePlayer", attacker);
            dictRes.Add("turn", turn);
            dictRes.Add("attackerPosition", attackerPosition);
            List<Dictionary<String, String>> listUpdatedAttackedGods = new List<Dictionary<String, String>>();

            foreach (GenericGod god in godsAttacked)
            {
                Dictionary<String, String> dictGod = new Dictionary<string, string>(); 
                dictGod.Add("god", god.Name);
                dictGod.Add("maxLife", god.MaxLife.ToString());
                dictGod.Add("currentLife", god.Life.ToString());
                listUpdatedAttackedGods.Add(dictGod);
            }
            
            dictRes.Add("updatedAttackedGods", listUpdatedAttackedGods);

            List<List<int>> listAttackPattern = new List<List<int>>();

            for (int index = 0; index < attackPattern.Length; index++)
            {
                int row = attackPattern[index][0];
                int col = attackPattern[index][1];
                
                listAttackPattern.Add(new List<int>() {row, col});
            }

            dictRes.Add("attackPattern", listAttackPattern);

              
            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public string StartDuelJson(Player firstPlayer, Player secondPlayer)
        {
            Dictionary<String, Object> dictRes = new Dictionary<string, Object>();
            dictRes.Add("type", "startDuel");

            List<Dictionary<String, Object>> listGods = new List<Dictionary<string, object>>(); 
            // first player initial state
            for (int index = 0; index < firstPlayer.GodTeam.AllGods.Length; index++)
            {
                Dictionary<String, Object> dictGod = new Dictionary<string, object>();
                GenericGod god = firstPlayer.GodTeam.AllGods[index];
                dictGod.Add("god", god.Name);
                dictGod.Add("maxLife", god.MaxLife);
                dictGod.Add("currentLife", god.Life);
                listGods.Add(dictGod);
            }
            dictRes.Add(firstPlayer.Username, listGods);        
    
            listGods = new List<Dictionary<string, object>>();
            // second player initial state
            for (int index = 0; index < secondPlayer.GodTeam.AllGods.Length; index++)
            {
                Dictionary<String, Object> dictGod = new Dictionary<string, object>();
                GenericGod god = secondPlayer.GodTeam.AllGods[index];
                dictGod.Add("god", god.Name);
                dictGod.Add("maxLife", god.MaxLife);
                dictGod.Add("currentLife", god.Life);
                listGods.Add(dictGod);
            }
            dictRes.Add(secondPlayer.Username, listGods);      

            string jsonString = JsonSerializer.Serialize(dictRes);
            return jsonString;
        }

        public String ChallengeToJson(string username)
        {
            Dictionary<String, Object> dictChallenge = new Dictionary<string, object>();
            dictChallenge.Add("type", "challenge");
            dictChallenge.Add("username", username);
            string jsonString = JsonSerializer.Serialize(dictChallenge);
            return jsonString; 
        }

        public String ChallengeCancelledToJson(string username, String usernameChallenged)
        {
            Dictionary<String, Object> dictChallengeCancelled = new Dictionary<string, object>();
            dictChallengeCancelled.Add("type", "challengeCancelled");
            dictChallengeCancelled.Add("username", username);
            dictChallengeCancelled.Add("usernameChallenged", usernameChallenged);
            string jsonString = JsonSerializer.Serialize(dictChallengeCancelled);
            return jsonString; 
        }

        public String ChallengeRefusedToJson(String username, string usernameChallenged)
        {
            Dictionary<String, Object> dictChallengeCancelled = new Dictionary<string, object>();
            dictChallengeCancelled.Add("type", "challengeRefused");
            dictChallengeCancelled.Add("username", username);
            dictChallengeCancelled.Add("usernameChallenged", usernameChallenged);
            string jsonString = JsonSerializer.Serialize(dictChallengeCancelled);
            return jsonString;
        }

        public String ChallengeAcceptedToJson(String username, String usernameChallenged)
        {
            Dictionary<String, Object> dictChallengeCancelled = new Dictionary<string, object>();
            dictChallengeCancelled.Add("type", "challengeAccepted");
            dictChallengeCancelled.Add("username", username);
            dictChallengeCancelled.Add("usernameChallenged", usernameChallenged);
            string jsonString = JsonSerializer.Serialize(dictChallengeCancelled);
            return jsonString;
        }
    }
}