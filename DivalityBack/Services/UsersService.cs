using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Models.Gods;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.VisualBasic;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DivalityBack.Services
{
    public class UsersService
    {
        private readonly UsersCRUDService _usersCRUDService;
        private readonly CardsCRUDService _cardsCrudService;
        private readonly FriendRequestsCRUDService _friendRequestsCrudService;
        private readonly CardsService _cardsService;
        private readonly UtilServices _utilService;
        private readonly BackgroundWorkerQueue _backgroundWorkerQueue;

        public Dictionary<String, WebSocket> mapActivePlayersWebsocket = new Dictionary<String, WebSocket>();
        public Dictionary<String, WebSocket> mapQueuePlayersWebsocket = new Dictionary<String, WebSocket>();
        public Dictionary<String, String> mapInFightPlayersCouple = new Dictionary<String, String>();
        public Dictionary<String, int> mapPlayerCurrentGodTeam = new Dictionary<String, int>();

        public UsersService(BackgroundWorkerQueue backgroundWorkerQueue, UsersCRUDService usersCRUDService,
            CardsCRUDService cardsCrudService, FriendRequestsCRUDService friendRequestsCrudService,
            CardsService cardsService, UtilServices utilService)
        {
            _usersCRUDService = usersCRUDService;
            _cardsCrudService = cardsCrudService;
            _friendRequestsCrudService = friendRequestsCrudService;
            _cardsService = cardsService;
            _utilService = utilService;
            _backgroundWorkerQueue = backgroundWorkerQueue;
        }

        public string HashPassword(String password)
        {
            byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);
            byte[] passwordBytesHash = new MD5CryptoServiceProvider().ComputeHash(passwordBytes);
            string passwordHash = Encoding.Default.GetString(passwordBytesHash);
            return passwordHash;
        }

        public User SignUp(JsonElement userJson)
        {
            User newUser = new User();
            //On remplit l'username et le password depuis le body de la requête POST;
            newUser.Username = userJson.GetProperty("username").GetString();
            //On hash le password
            newUser.Password = HashPassword(userJson.GetProperty("password").GetString());

            // On vérifie que l'username n'existe pas déjà en base, sinon on renvoie null
            if (_usersCRUDService.GetByUsername(newUser.Username) != null)
            {
                return null;
            }

            //Sinon
            //On créé l'entrée en base
            return _usersCRUDService.Create(newUser);
        }

        public String SignIn(string username, string password)
        {
            password = HashPassword(password);
            User user = _usersCRUDService.GetByUsernameAndPassword(username, password);
            if (user == null)
            {
                return null;
            }

            Dictionary<String, String> dictRes = new Dictionary<string, string>();
            dictRes.Add("disciples", user.Disciples.ToString());
            string jsonString = JsonSerializer.Serialize(dictRes);

            return jsonString;
        }

        public async Task Pray(string username, string pantheon, WebSocket webSocket, WebSocketReceiveResult result)
        {
            User user = _usersCRUDService.GetByUsername(username);
            if (CanAffordCard(user))
            {
                //On génère la carte aléatoire en fonction du pantheon choisi
                Card card = _cardsService.GenerateNewCard(pantheon);
                while (card == null)
                {
                    card = _cardsService.GenerateNewCard(pantheon);
                }

                //On met à jour le nombre de disciples et la collection
                user.Disciples -= _cardsService.priceOfCard;
                user.Collection.Add(card.Id);

                _usersCRUDService.Update(user.Id, user);

                await WarnUserPurchaseCard(webSocket, result, card);
            }
            else
            {
                await WarnUserNotEnoughDisciples(webSocket, result);
            }
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserNotEnoughDisciples(WebSocket webSocket, WebSocketReceiveResult result)
        {
            byte[] byteNotEnoughDisciples = Encoding.UTF8.GetBytes("L'utilisateur ne possède pas assez de disciples");
            await webSocket.SendAsync(byteNotEnoughDisciples, result.MessageType, result.EndOfMessage,
                CancellationToken.None);
        }

        public Boolean CanAffordCard(User user)
        {
            return user.Disciples >= _cardsService.priceOfCard;
        }

        [ExcludeFromCodeCoverage]
        public async Task WarnUserPurchaseCard(WebSocket webSocket, WebSocketReceiveResult result, Card card)
        {
            string jsonCard = _utilService.CardToJson(card);
            byte[] byteCardObtained = Encoding.UTF8.GetBytes(jsonCard);
            await webSocket.SendAsync(byteCardObtained, result.MessageType, result.EndOfMessage,
                CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        public async Task GetCollection(string username, WebSocket webSocket, WebSocketReceiveResult result)
        {
            User user = _usersCRUDService.GetByUsername(username);
            if (user != null)
            {
                List<String> collection = user.Collection;
                String jsonCollection = _utilService.CollectionToJson(collection);
                await WarnUserCollection(webSocket, result, jsonCollection);
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task WarnUserCollection(WebSocket webSocket, WebSocketReceiveResult result, String jsonCollection)
        {
            byte[] byteCollection = Encoding.UTF8.GetBytes(jsonCollection);
            await webSocket.SendAsync(byteCollection, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        public async Task WarnUsersOfConnection(string username, WebSocket webSocket, WebSocketReceiveResult result)
        {
            User user = _usersCRUDService.GetByUsername(username);

            //On récupère la liste des amis connectés de l'User
            List<String> listFriendsConnected = _usersCRUDService.GetUsersById(user.Friends)
                .Select(s => s.Username)
                .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            //On récupère la liste des amis déconnectés de l'User
            List<String> listFriendsDisconnected = _usersCRUDService.GetUsersById(user.Friends)
                .Select(s => s.Username)
                .Except(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            //On récupère la liste des requêtes d'amis reçues par l'User
            List<FriendRequest> listFriendRequests = _friendRequestsCrudService.FindByReceiver(user.Id);
            List<String> listSenderOfFriendRequests = new List<string>();
            foreach (FriendRequest friendRequest in listFriendRequests)
            {
                listSenderOfFriendRequests.Add(_usersCRUDService.Get(friendRequest.Sender).Username);
            }

            String jsonFriends = _utilService.FriendsToJson(listFriendsConnected, listFriendsDisconnected,
                listSenderOfFriendRequests);

            byte[] byteFriends = Encoding.UTF8.GetBytes(jsonFriends);
            await webSocket.SendAsync(byteFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);

            foreach (string friendUsername in listFriendsConnected)
            {
                user = _usersCRUDService.GetByUsername(friendUsername);
                //On récupère la liste des amis connectés de l'User
                List<String> friendsConnected = _usersCRUDService.GetUsersById(user.Friends)
                    .Select(s => s.Username)
                    .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

                //On récupère la liste des amis déconnectés de l'User
                listFriendsDisconnected = _usersCRUDService.GetUsersById(user.Friends)
                    .Select(s => s.Username)
                    .Except(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

                //On récupère la liste des requêtes d'amis reçues par l'User
                listFriendRequests = _friendRequestsCrudService.FindByReceiver(user.Id);
                listSenderOfFriendRequests = new List<string>();
                foreach (FriendRequest friendRequest in listFriendRequests)
                {
                    listSenderOfFriendRequests.Add(_usersCRUDService.Get(friendRequest.Sender).Username);
                }

                jsonFriends = _utilService.FriendsToJson(friendsConnected, listFriendsDisconnected,
                    listSenderOfFriendRequests);
                byteFriends = Encoding.UTF8.GetBytes(jsonFriends);

                webSocket = mapActivePlayersWebsocket[user.Username];
                await webSocket.SendAsync(byteFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task getTeams(WebSocket websocket, WebSocketReceiveResult result, string username)
        {
            List<Team> teams = _usersCRUDService.GetByUsername(username).Teams.OrderBy(t => t.Name).ToList();
            String jsonTeams = _utilService.TeamsToJson(teams);

            await WarnUserTeams(websocket, result, jsonTeams);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserTeams(WebSocket websocket, WebSocketReceiveResult result, string jsonTeams)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonTeams);
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task ModifyTeam(WebSocket websocket, WebSocketReceiveResult result, string username,
            string oldNameTeam, string newNameTeam, string compo)
        {
            User user = _usersCRUDService.GetByUsername(username);
            Team teamToModify = user.Teams.FindAll(t => t.Name.Equals(oldNameTeam)).FirstOrDefault();
            if (teamToModify == null)
            {
                teamToModify = new Team();
            }
            else
            {
                user.Teams.Remove(teamToModify);
                teamToModify.Compo.Clear();
            }

            teamToModify.Name = newNameTeam;
            List<String> listNewCardName = new List<string>(compo.Remove(0, 1).Remove(compo.Length - 2).Split(","));
            foreach (string cardName in listNewCardName)
            {
                Card card = _cardsCrudService.GetCardByName(cardName.Replace("\"", "").Trim());
                teamToModify.Compo.Add(card.Id);
            }

            user.Teams.Add(teamToModify);
            _usersCRUDService.Update(user.Id, user);
            await getTeams(websocket, result, username);
        }

        public async Task SendFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender,
            string usernameReceiver)
        {
            User sender = _usersCRUDService.GetByUsername(usernameSender);
            User receiver = _usersCRUDService.GetByUsername(usernameReceiver);
            if (receiver != null)
            {
                if (!sender.Friends.Contains(receiver.Id))
                {


                    FriendRequest request = _friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id);
                    if (request == null)
                    {
                        FriendRequest newRequest = new FriendRequest();
                        newRequest.Sender = sender.Id;
                        newRequest.Receiver = receiver.Id;

                        _friendRequestsCrudService.Create(newRequest);

                        if (mapActivePlayersWebsocket.ContainsKey(receiver.Username))
                        {
                            WebSocket webSocketReceiver = mapActivePlayersWebsocket[receiver.Username];
                            await WarnUserOfFriendRequest(webSocketReceiver, result, receiver.Username);
                        }
                    }
                    else
                    {
                        await WarnUserOfFriendRequestAlreadyExisting(websocket, result);
                    }
                }
                else
                {
                    await WarnUserAlreadyFriend(websocket, result);
                }
            }
            else
            {
                await WarnUserOfUserNotFound(websocket, result);
            }
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserAlreadyFriend(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Vous êtes déjà ami avec ce joueur");
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserOfUserNotFound(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Joueur introuvable");
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserOfFriendRequestAlreadyExisting(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Vous avez déjà envoyé une requête d'ami à cette personne");
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        private async Task WarnUserOfFriendRequest(WebSocket webSocketReceiver, WebSocketReceiveResult result,
            string receiverUsername)
        {
            User user = _usersCRUDService.GetByUsername(receiverUsername);

            //On récupère la liste des amis connectés de l'User
            List<String> listFriendsConnected = _usersCRUDService.GetUsersById(user.Friends)
                .Select(s => s.Username)
                .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            //On récupère la liste des amis déconnectés de l'User
            List<String> listFriendsDisconnected = _usersCRUDService.GetUsersById(user.Friends)
                .Select(s => s.Username)
                .Except(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            //On récupère la liste des requêtes d'amis reçues par l'User
            List<FriendRequest> listFriendRequests = _friendRequestsCrudService.FindByReceiver(user.Id);
            List<String> listSenderOfFriendRequests = new List<string>();
            foreach (FriendRequest friendRequest in listFriendRequests)
            {
                listSenderOfFriendRequests.Add(_usersCRUDService.Get(friendRequest.Sender).Username);
            }

            String jsonFriends = _utilService.FriendsToJson(listFriendsConnected, listFriendsDisconnected,
                listSenderOfFriendRequests);

            byte[] byteFriends = Encoding.UTF8.GetBytes(jsonFriends);
            await webSocketReceiver.SendAsync(byteFriends, result.MessageType, result.EndOfMessage,
                CancellationToken.None);
        }

        public async Task AcceptFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender,
            string usernameReceiver)
        {
            User sender = _usersCRUDService.GetByUsername(usernameSender);
            User receiver = _usersCRUDService.GetByUsername(usernameReceiver);
            FriendRequest request = _friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id);

            sender.Friends.Add(receiver.Id);
            receiver.Friends.Add(sender.Id);

            _friendRequestsCrudService.Remove(request);
            _usersCRUDService.Update(sender.Id, sender);
            _usersCRUDService.Update(receiver.Id, receiver);

            if (mapActivePlayersWebsocket.ContainsKey(usernameSender))
            {
                await WarnUserOfFriendRequest(mapActivePlayersWebsocket[usernameSender], result, usernameSender);
            }

            await WarnUserOfFriendRequest(websocket, result, usernameReceiver);

        }

        public async Task RefuseFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender,
            string usernameReceiver)
        {
            User sender = _usersCRUDService.GetByUsername(usernameSender);
            User receiver = _usersCRUDService.GetByUsername(usernameReceiver);
            FriendRequest request = _friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id);

            _friendRequestsCrudService.Remove(request);

            await WarnUserOfFriendRequest(websocket, result, usernameSender);
        }

        public async Task DeleteFriend(WebSocket websocket, WebSocketReceiveResult result, string username,
            string usernameFriendToDelete)
        {
            User user = _usersCRUDService.GetByUsername(username);
            User friendToDelete = _usersCRUDService.GetByUsername(usernameFriendToDelete);

            user.Friends.Remove(friendToDelete.Id);
            friendToDelete.Friends.Remove(user.Id);

            _usersCRUDService.Update(user.Id, user);
            _usersCRUDService.Update(friendToDelete.Id, friendToDelete);

            await WarnUserOfFriendRequest(websocket, result, username);
            if (mapActivePlayersWebsocket.ContainsKey(usernameFriendToDelete))
            {
                await WarnUserOfFriendRequest(mapActivePlayersWebsocket[usernameFriendToDelete], result,
                    usernameFriendToDelete);
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task WarnUseAlreadyConnected(WebSocket webSocket, WebSocketReceiveResult result, string username)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Le compte " + username + " est déjà connecté");
            await webSocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public void StartMatchmaking()
        {
            _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    await Matchmaking();
                }
            });
        }

        private async Task Matchmaking()
        {
            if (mapQueuePlayersWebsocket.Count > 1)
            {
                List<List<List<User>>> combinaisons = new List<List<List<User>>>();

                //On récupère la file d'attente et on la vide
                Dictionary<String, WebSocket> queue = new Dictionary<string, WebSocket>(mapQueuePlayersWebsocket);
                mapQueuePlayersWebsocket.Clear();
                //On récupère tous les joueurs
                List<User> listUserInQueue = new List<User>();

                foreach (var keyValuePair in queue)
                {
                    listUserInQueue.Add(_usersCRUDService.GetByUsername(keyValuePair.Key));
                }

                //On calcule toutes les combinaisons possibles de duels
                combinaisons = CalculCombinaison(listUserInQueue);

                Dictionary<int, double> mapCombinaisonScore = new Dictionary<int, double>();

                //On leur attribue à chacune un score
                foreach (List<List<User>> combinaison in combinaisons)
                {
                    double score = CalculScoreCombinaison(combinaison);
                    mapCombinaisonScore.Add(combinaisons.IndexOf(combinaison), score);
                }

                //On prend une des combinaisons avec le score le plus faible 
                int indexOfMinimalScore = mapCombinaisonScore.OrderBy(k => k.Value).FirstOrDefault().Key;
                List<List<User>> combinaisonKept = combinaisons[indexOfMinimalScore];

                //On prévient tous les joueurs de leur opponent et on les ajoute dans la map InFight
                foreach (List<User> pair in combinaisonKept)
                {
                   await WarnUsersOfDuel(pair, queue);
                   mapInFightPlayersCouple.Add(pair[0].Username, pair[1].Username);
                   mapInFightPlayersCouple.Add(pair[1].Username, pair[0].Username);
                }

                //S'il reste un joueur sans match, on le replace dans la liste d'attente
                foreach (List<User> pair in combinaisonKept)
                {
                    if (queue.Keys.Contains(pair[0].Username))
                    {
                        queue.Remove(pair[0].Username);
                    }

                    if (queue.Keys.Contains(pair[1].Username))
                    {
                        queue.Remove(pair[1].Username);
                    }
                }

                foreach (var keyValuePair in queue)
                {
                    mapQueuePlayersWebsocket.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        private async Task WarnUsersOfDuel(List<User> pair, Dictionary<string, WebSocket> queue)
        {
            String jsonOpponent = _utilService.DuelToJson(pair[1].Username);
            WebSocket webSocket = queue[pair[0].Username];
            byte[] bytes = Encoding.UTF8.GetBytes(jsonOpponent);
            await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

            jsonOpponent = _utilService.DuelToJson(pair[0].Username);
            webSocket = queue[pair[1].Username];
            bytes = Encoding.UTF8.GetBytes(jsonOpponent);
            await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private double CalculScoreCombinaison(List<List<User>> combinaison)
        {
            double score = 0;
            foreach (List<User> pair in combinaison)
            {
                int winrateUser0 = 0;
                int winrateUser1 = 0;

                if (!(pair[0].Victory + pair[0].Defeat).Equals(0))
                {
                    winrateUser0 = pair[0].Victory / (pair[0].Victory + pair[0].Defeat);
                }

                if (!(pair[1].Victory + pair[1].Defeat).Equals(0))
                {
                    winrateUser1 = pair[1].Victory / (pair[1].Victory + pair[1].Defeat);
                }

                score += Math.Pow(winrateUser0 - winrateUser1, 2);
            }

            return score;
        }

        private List<List<List<User>>> CalculCombinaison(List<User> listUserInQueue)
        {
            List<List<List<User>>> combinaisons = new List<List<List<User>>>();
            List<List<User>> pairs = GetAllPairsOfUsers(listUserInQueue);

            int numberOfPairs = (int) Math.Floor((decimal) (listUserInQueue.Count / 2));

            GetAllCombinaisonsFromPairs(pairs, combinaisons, new List<List<User>>(), new List<List<User>>(),
                numberOfPairs);

            return combinaisons;
        }

        public List<List<User>> GetAllPairsOfUsers(List<User> users)
        {
            List<List<User>> listPairs = new List<List<User>>();
            for (int i = 0; i < users.Count; i++)
            {
                for (int j = i; j < users.Count; j++)
                {
                    if (i != j)
                    {
                        List<User> pair = new List<User>();
                        pair.Add(users[i]);
                        pair.Add(users[j]);
                        listPairs.Add(pair);
                    }
                }
            }

            return listPairs;
        }

        private void GetAllCombinaisonsFromPairs(List<List<User>> pairs, List<List<List<User>>> combinaisons,
            List<List<User>> pairsKept, List<List<User>> combinaison, int numberOfPairs)
        {
            List<List<User>> pairsFiltered = new List<List<User>>();
            //Pour toutes les paires
            foreach (List<User> pair in pairs)
            {

                if (pairsKept.Count == 0)
                {
                    combinaison.Clear();
                }

                combinaison.Add(pair);
                pairsKept.Add(pair);

                //On maj la liste des users utilisés
                List<User> userUsed = new List<User>();
                foreach (List<User> pairKept in pairsKept)
                {
                    userUsed.Add(pairKept[0]);
                    userUsed.Add(pairKept[1]);
                }

                pairsFiltered = pairs.Except(pairs.FindAll(p => userUsed.Contains(p[0]) || userUsed.Contains(p[1])))
                    .ToList();


                if (combinaison.Count == numberOfPairs)
                {
                    pairsKept.Remove(pairsKept[pairsKept.Count - 1]);
                    combinaisons.Add(new List<List<User>>(combinaison));
                    combinaison = new List<List<User>>(pairsKept);
                }

                if (pairsFiltered.Count > 0)
                {
                    //On réitère pour toutes les paires restantes
                    GetAllCombinaisonsFromPairs(pairsFiltered, combinaisons, pairsKept, combinaison, numberOfPairs);
                }
            }

            if (!pairsKept.Count.Equals(0))
            {
                pairsKept.Remove(pairsKept[pairsKept.Count - 1]);
            }
        }

        public void WaitForDuel(string username)
        {
            if (!(mapQueuePlayersWebsocket.Keys.Contains(username)) &&
                mapActivePlayersWebsocket.Keys.Contains(username))
            {
                mapQueuePlayersWebsocket.Add(username, mapActivePlayersWebsocket[username]);
            }
        }

        public void CancelWaitForDuel(string username)
        {
            if ((mapQueuePlayersWebsocket.Keys.Contains(username)))
            {
                mapQueuePlayersWebsocket.Remove(username);
            }
        }

        public async Task GetDisciples(WebSocket webSocket, WebSocketReceiveResult result, string username)
        {
            int disciples = _usersCRUDService.GetByUsername(username).Disciples;
            String jsonDisciples = _utilService.DisciplesToJson(disciples);
            await WarnUserOfDisciples(webSocket, result, jsonDisciples);
        }

        private async Task WarnUserOfDisciples(WebSocket webSocket, WebSocketReceiveResult result, string jsonDisciples)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonDisciples);
            await webSocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public async Task PickTeamForDuel(String username, int teamIndex, WebSocketReceiveResult result)
        {
            mapPlayerCurrentGodTeam.Add(username, teamIndex);
            
            // Récupération de la liste des noms de dieux
            List<string> namesOfGods = getNamesOfGods(username, teamIndex);

            // Avertissement du joueur adverse
            byte[] bytes = Encoding.UTF8.GetBytes(_utilService.GodListToJson(namesOfGods));
            String opponentUsername = mapInFightPlayersCouple[username];
            WebSocket opponentWebsocket = mapActivePlayersWebsocket[opponentUsername];
            await opponentWebsocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
            
            // Lancement du duel si les deux joueurs ont choisi leur team
            if (mapPlayerCurrentGodTeam.Keys.Contains(opponentUsername))
            {
                // Récupération de la liste des noms de dieux opposants
                int opponentTeamIndex = mapPlayerCurrentGodTeam[opponentUsername];
                List<string> opponentNamesOfGods = getNamesOfGods(opponentUsername, opponentTeamIndex);
                await StartDuel(
                    namesOfGods, 
                    opponentNamesOfGods, 
                    username, 
                    opponentUsername, 
                    result
                );
            }
        }

        private List<string> getNamesOfGods(string username, int teamIndex)
        {
            User user = _usersCRUDService.GetByUsername(username);
            Team userTeam = user.Teams[teamIndex];
            List<string> idsOfGods = userTeam.Compo;
            List<string> namesOfGods = new List<string>();
            foreach (var id in idsOfGods)
            {
                namesOfGods.Add(_cardsCrudService.Get(id).Name);
            }

            return namesOfGods;
        }

        private async Task StartDuel(
            List<string> namesOfGods1, 
            List<string> namesOfGods2, 
            string username1, 
            string username2,
            WebSocketReceiveResult result
        ) {
            // création de l'équipe de dieux et des joueurs
            List<GenericGod> godList1 = new List<GenericGod>();
            List<GenericGod> godList2 = new List<GenericGod>();;

            for (int index = 0; index < namesOfGods1.Count; index++)
            {
                godList1.Add(GenericGod.getGodByName(namesOfGods1[index]));
                godList2.Add(GenericGod.getGodByName(namesOfGods2[index]));
            }

            GodTeam godTeam1 = new GodTeam(godList1.ToArray());
            GodTeam godTeam2 = new GodTeam(godList2.ToArray());

            Player player1 = new Player(godTeam1, username1);
            Player player2 = new Player(godTeam2, username2);

            // lancement du duel
            Duel duel = new Duel(player1, player2);
            
            duel.initDuel();

            while (player1.isAlive() && player2.isAlive())
            {
                duel.play();
            }

            // avertissement du résultat
            string winner = duel.winner().Username;
            string looser = duel.looser().Username;

            string winnerJson = _utilService.WinnerJson();
            string looserJson = _utilService.LooserJson();
            
            byte[] winnerBytes = Encoding.UTF8.GetBytes(winnerJson);
            byte[] looserBytes = Encoding.UTF8.GetBytes(looserJson);
            
            WebSocket winnerWebSocket = mapActivePlayersWebsocket[winner];
            WebSocket looserWebSocket = mapActivePlayersWebsocket[looser];

            await winnerWebSocket.SendAsync(winnerBytes, result.MessageType, result.EndOfMessage,
                CancellationToken.None);
            await looserWebSocket.SendAsync(looserBytes, result.MessageType, result.EndOfMessage,
                CancellationToken.None);
        }

        public async Task WarnUserNotFound(WebSocket webSocket, WebSocketReceiveResult result){
            byte[] bytes = Encoding.UTF8.GetBytes("Le joueur n'a pas pu être trouvé, veuillez réessayer");
            await webSocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}

