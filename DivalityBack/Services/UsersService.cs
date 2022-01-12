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
        private readonly CardsService _cardsService;
        private readonly UtilServices _utilService; 
        
        public Dictionary<String, WebSocket> mapActivePlayersWebsocket = new Dictionary<String, WebSocket>();

        public UsersService(UsersCRUDService usersCRUDService, CardsCRUDService cardsCrudService, CardsService cardsService, UtilServices utilService)
        {
            _usersCRUDService = usersCRUDService;
            _cardsCrudService = cardsCrudService; 
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

            String jsonFriends = _utilService.FriendsToJson(listFriendsConnected, listFriendsDisconnected); 
            
            byte[] byteFriends = Encoding.UTF8.GetBytes(jsonFriends);
            await webSocket.SendAsync(byteFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);
            
            foreach (string friendUsername in listFriendsConnected)
            {
                user = _usersCRUDService.GetByUsername(friendUsername);
                //On récupère la liste des amis connectés de l'User
                List<String> FriendConnected = _usersCRUDService.GetUsersById(user.Friends)
                    .Select(s => s.Username)
                    .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();
            
                //On récupère la liste des amis déconnectés de l'User
                List<String> listDisconnected = _usersCRUDService.GetUsersById(user.Friends)
                    .Select(s => s.Username)
                    .Except(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

                jsonFriends = _utilService.FriendsToJson(FriendConnected, listDisconnected);
                byteFriends = Encoding.UTF8.GetBytes(jsonFriends);

                webSocket = mapActivePlayersWebsocket[user.Username];
                await webSocket.SendAsync(byteFriends, result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }

        public async Task getTeams(WebSocket websocket, WebSocketReceiveResult result, string username)
        {
            List<Team> teams = _usersCRUDService.GetByUsername(username).Teams.OrderBy(t => t.Name).ToList();
            String jsonTeams = _utilService.TeamsToJson(teams);

            await WarnUserTeams(websocket, result, jsonTeams); 
        }

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
    }
}