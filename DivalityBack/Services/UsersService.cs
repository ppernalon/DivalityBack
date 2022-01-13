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
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
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
        
        public Dictionary<String, WebSocket> mapActivePlayersWebsocket = new Dictionary<String, WebSocket>();

        public UsersService(UsersCRUDService usersCRUDService, CardsCRUDService cardsCrudService, FriendRequestsCRUDService friendRequestsCrudService, CardsService cardsService, UtilServices utilService)
        {
            _usersCRUDService = usersCRUDService;
            _cardsCrudService = cardsCrudService;
            _friendRequestsCrudService = friendRequestsCrudService; 
            _cardsService = cardsService;
            _utilService = utilService; 
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
            await webSocket.SendAsync(byteNotEnoughDisciples, result.MessageType, result.EndOfMessage, CancellationToken.None);
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
            await webSocket.SendAsync(byteCardObtained, result.MessageType, result.EndOfMessage, CancellationToken.None); 
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
            
            String jsonFriends = _utilService.FriendsToJson(listFriendsConnected, listFriendsDisconnected, listSenderOfFriendRequests); 
            
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
                jsonFriends = _utilService.FriendsToJson(friendsConnected, listFriendsDisconnected, listSenderOfFriendRequests);
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

        public async Task ModifyTeam(WebSocket websocket, WebSocketReceiveResult result, string username, string oldNameTeam, string newNameTeam, string compo)
        {
            User user = _usersCRUDService.GetByUsername(username);
            Team teamToModify = user.Teams.FindAll(t => t.Name.Equals(oldNameTeam)).FirstOrDefault();
            user.Teams.Remove(teamToModify); 

            teamToModify.Name = newNameTeam;
            teamToModify.Compo.Clear();
            List<String> listNewCardName = new List<string>(compo.Split(","));
            foreach (string cardName in listNewCardName)
            {
                Card card = _cardsCrudService.GetCardByName(cardName);
                teamToModify.Compo.Add(card.Id);
            }
            user.Teams.Add(teamToModify);
            _usersCRUDService.Update(user.Id, user);
            await getTeams(websocket, result, username); 
        }

        public async Task SendFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender, string usernameReceiver)
        {
            User sender = _usersCRUDService.GetByUsername(usernameSender);
            User receiver = _usersCRUDService.GetByUsername(usernameReceiver);
            if (receiver != null){
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
                await WarnUserOfUserNotFound(websocket, result); 
            }
        }

        private async Task WarnUserOfUserNotFound(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Joueur introuvable");
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None);         }

        private async Task WarnUserOfFriendRequestAlreadyExisting(WebSocket websocket, WebSocketReceiveResult result)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Vous avez déjà envoyé une requête d'ami à cette personne");
            await websocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }

        private async Task WarnUserOfFriendRequest(WebSocket webSocketReceiver, WebSocketReceiveResult result, string receiverUsername)
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
            
            String jsonFriends = _utilService.FriendsToJson(listFriendsConnected, listFriendsDisconnected, listSenderOfFriendRequests); 
            
            byte[] byteFriends = Encoding.UTF8.GetBytes(jsonFriends);
            await webSocketReceiver.SendAsync(byteFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);        }

        public async Task AcceptFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender, string usernameReceiver)
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

        public async Task RefuseFriendRequest(WebSocket websocket, WebSocketReceiveResult result, string usernameSender, string usernameReceiver)
        {
            User sender = _usersCRUDService.GetByUsername(usernameSender);
            User receiver = _usersCRUDService.GetByUsername(usernameReceiver);
            FriendRequest request = _friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id);

            _friendRequestsCrudService.Remove(request);

            await WarnUserOfFriendRequest(websocket, result, usernameSender);
        }

        public async Task DeleteFriend(WebSocket websocket, WebSocketReceiveResult result, string username, string usernameFriendToDelete)
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

        public async Task WarnUseAlreadyConnected(WebSocket webSocket, WebSocketReceiveResult result, string username)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Le compte " + username + " est déjà connecté");
            await webSocket.SendAsync(bytes, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }
    }
}