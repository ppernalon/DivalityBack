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
            dictRes.Add("description", card.Description);

            
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

            //Gestion des amis connect??s
            foreach (string username in listUsernameConnected)
            {
                User user = _usersCrudService.GetByUsername(username);
                Dictionary<String, String> dictConnected = new Dictionary<string, string>();
                
                dictConnected.Add("username", username);
                dictConnected.Add("victory", user.Victory.ToString());
                dictConnected.Add("defeat", user.Defeat.ToString());
                listDictConnected.Add(dictConnected);
            }
            
            //Gestion des amis d??connect??s
            foreach (string username in listUsernameDisconnected)
            {
                User user = _usersCrudService.GetByUsername(username);
                Dictionary<String, String> dictDisconnected = new Dictionary<string, string>();
                
                dictDisconnected.Add("username", username);
                dictDisconnected.Add("victory", user.Victory.ToString());
                dictDisconnected.Add("defeat", user.Defeat.ToString());
                listDictDisconnected.Add(dictDisconnected);
            }
            
            //Gestion des requ??tes d'ami
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

        public String RankingToJson(List<Dictionary<string, object>> ranking)
        {
            Dictionary<String, Object> dictRanking = new Dictionary<string, object>();
            dictRanking.Add("type", "ranking");
            dictRanking.Add("dataRanking", ranking);
            string jsonString = JsonSerializer.Serialize(dictRanking);
            return jsonString; 
        }

        public string GetInfoWinRate(User user)
        {
            Dictionary<String, Object> dictInfoWinRate = new Dictionary<string, object>();
            dictInfoWinRate.Add("type","infoWinRate");
            dictInfoWinRate.Add("username", user.Username);
            dictInfoWinRate.Add("victory",user.Victory);
            dictInfoWinRate.Add("defeat", user.Defeat);
            string jsonString = JsonSerializer.Serialize(dictInfoWinRate);
            return jsonString;
        }

        public string UserNoTeamToJson()
        {
            Dictionary<String, Object> dictUserNoTeam = new Dictionary<string, object>();
            dictUserNoTeam.Add("type", "noTeam");
            dictUserNoTeam.Add("info", "noTeam");
            string jsonString = JsonSerializer.Serialize(dictUserNoTeam);
            return jsonString;
        }

        public string UserNotFoundToJson()
        {
            Dictionary<String, Object> dictUserNotFound = new Dictionary<string, object>();
            dictUserNotFound.Add("type", "userNotFound");
            dictUserNotFound.Add("info", "userNotFound");
            string jsonString = JsonSerializer.Serialize(dictUserNotFound);
            return jsonString;
        }


        public string FriendChallengedToJson()
        {
            Dictionary<String, Object> dictFriendChallenged = new Dictionary<string, object>();
            dictFriendChallenged.Add("type", "friendChallenged");
            dictFriendChallenged.Add("info", "friendChallenged");
            string jsonString = JsonSerializer.Serialize(dictFriendChallenged);
            return jsonString;
        }

        public string UserAlreadyInDuelToJson()
        {
            Dictionary<String, Object> dictUserAlreadyInDuel = new Dictionary<string, object>();
            dictUserAlreadyInDuel.Add("type", "userAlreadyInDuel");
            dictUserAlreadyInDuel.Add("info", "userAlreadyInDuel");
            string jsonString = JsonSerializer.Serialize(dictUserAlreadyInDuel);
            return jsonString;
        }


        public string MessageChallengeAcceptedToJson()
        {
            Dictionary<String, Object> dictMessageChallengeAccepted = new Dictionary<string, object>();
            dictMessageChallengeAccepted.Add("type", "challengeAccepted");
            dictMessageChallengeAccepted.Add("info", "challengeAccepted");
            string jsonString = JsonSerializer.Serialize(dictMessageChallengeAccepted);
            return jsonString;
        }

        public string MessageChallengeRefusedToJson()
        {
            Dictionary<String, Object> dictMessageChallengeRefused = new Dictionary<string, object>();
            dictMessageChallengeRefused.Add("type", "challengeRefused");
            dictMessageChallengeRefused.Add("info", "challengeRefused");
            string jsonString = JsonSerializer.Serialize(dictMessageChallengeRefused);
            return jsonString;
        }

        public string MessageChallengeCanceledToJson()
        {
            Dictionary<String, Object> dictMessageChallengeCanceled = new Dictionary<string, object>();
            dictMessageChallengeCanceled.Add("type", "challengeCanceled");
            dictMessageChallengeCanceled.Add("info", "challengeCanceled");
            string jsonString = JsonSerializer.Serialize(dictMessageChallengeCanceled);
            return jsonString;
        }

        public string UserNotConnectedToJson()
        {
            Dictionary<String, Object> dictUserNotConnected = new Dictionary<string, object>();
            dictUserNotConnected.Add("type", "userNotConnected");
            dictUserNotConnected.Add("info", "userNotConnected");
            string jsonString = JsonSerializer.Serialize(dictUserNotConnected);
            return jsonString;
        }

        public string NotEnoughCardToJson()
        {
            Dictionary<String, Object> dictNotEnoughCard = new Dictionary<string, object>();
            dictNotEnoughCard.Add("type", "notEnoughCard");
            dictNotEnoughCard.Add("info", "notEnoughCard");
            string jsonString = JsonSerializer.Serialize(dictNotEnoughCard);
            return jsonString;
        }

        public string RequestAutomaticallyAcceptedToJson()
        {
            Dictionary<String, Object> dictRequestAutomaticallyAccepted = new Dictionary<string, object>();
            dictRequestAutomaticallyAccepted.Add("type", "requestAutomaticallyAccepted");
            dictRequestAutomaticallyAccepted.Add("info", "requestAutomaticallyAccepted");
            string jsonString = JsonSerializer.Serialize(dictRequestAutomaticallyAccepted);
            return jsonString;
        }


        public string RequestSentToJson()
        {
            Dictionary<String, Object> dictRequestSent = new Dictionary<string, object>();
            dictRequestSent.Add("type", "requestSent");
            dictRequestSent.Add("info", "requestSent");
            string jsonString = JsonSerializer.Serialize(dictRequestSent);
            return jsonString;
        }

        public string AlreadyFriendToJson()
        {
            Dictionary<String, Object> dictAlreadyFriend = new Dictionary<string, object>();
            dictAlreadyFriend.Add("type", "alreadyFriend");
            dictAlreadyFriend.Add("info", "alreadyFriend");
            string jsonString = JsonSerializer.Serialize(dictAlreadyFriend);
            return jsonString;
        }

        public string RequestAlreadySentToJson()
        {
            Dictionary<String, Object> dictRequestAlreadySent = new Dictionary<string, object>();
            dictRequestAlreadySent.Add("type", "requestAlreadySent");
            dictRequestAlreadySent.Add("info", "requestAlreadySent");
            string jsonString = JsonSerializer.Serialize(dictRequestAlreadySent);
            return jsonString;
        }

        public string UserAlreadyConnectedToJson()
        {
            Dictionary<String, Object> dictUserAlreadyConnected = new Dictionary<string, object>();
            dictUserAlreadyConnected.Add("type", "userAlreadyConnected");
            dictUserAlreadyConnected.Add("info", "userAlreadyConnected");
            string jsonString = JsonSerializer.Serialize(dictUserAlreadyConnected);
            return jsonString;
        }

        public string CardNotFoundToJson()
        {
            Dictionary<String, Object> dictCardNotFound = new Dictionary<string, object>();
            dictCardNotFound.Add("type", "cardNotFound");
            dictCardNotFound.Add("info", "cardNotFound");
            string jsonString = JsonSerializer.Serialize(dictCardNotFound);
            return jsonString;
        }

        public string NotEnoughDisciplesToJson()
        {
            Dictionary<String, Object> dictNotEnoughDisciples = new Dictionary<string, object>();
            dictNotEnoughDisciples.Add("type", "notEnoughDisciples");
            dictNotEnoughDisciples.Add("info", "notEnoughDisciples");
            string jsonString = JsonSerializer.Serialize(dictNotEnoughDisciples);
            return jsonString;
        }

        public string ErrorWebsocketToJson()
        {
            Dictionary<String, Object> dictErrorWebsocket = new Dictionary<string, object>();
            dictErrorWebsocket.Add("type", "errorWebsocket");
            dictErrorWebsocket.Add("info", "errorWebsocket");
            string jsonString = JsonSerializer.Serialize(dictErrorWebsocket);
            return jsonString;
        }

        public string PongToJson()
        {
            Dictionary<String, Object> dictPong = new Dictionary<string, object>();
            dictPong.Add("type", "pong");
            string jsonString = JsonSerializer.Serialize(dictPong);
            return jsonString;
        }
    }
}