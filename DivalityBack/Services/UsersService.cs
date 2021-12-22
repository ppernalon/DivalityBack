using System;
using System.Collections.Generic;
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

namespace Divality.Services
{
    public class UsersService
    {
        private readonly UsersCRUDService _usersCRUDService;
        private readonly CardsService _cardsService;
        public Dictionary<String, WebSocket> mapActivePlayersWebsocket = new Dictionary<String, WebSocket>();

        public UsersService(UsersCRUDService usersCRUDService, CardsService cardsService)
        {
            _usersCRUDService = usersCRUDService;
            _cardsService = cardsService; 
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
            Console.Write("USERJSON : " + userJson.ToString());
            //On remplit l'username et le password depuis le body de la requête POST;
            newUser.Username = userJson.GetProperty("username").GetString();
            Console.Write(newUser.Username);
            Console.Write(userJson.GetProperty("username").ToString());
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
            Console.Write(user);
            if (user == null)
            {
                return null;
            }

            Dictionary<String, String> dictRes = new Dictionary<string, string>();
            dictRes.Add("disciples", user.Disciples.ToString());
            string jsonString = JsonSerializer.Serialize(dictRes);

            return jsonString;
        }

        public async Task WarnFriendsOfUserOfConnection(String username, WebSocketReceiveResult result)
        {
            User userConnected = _usersCRUDService.GetByUsername(username);
            List<String> listOfFriendsConnected = _usersCRUDService.GetUsersById(userConnected.Friends)
                .Select(s => s.Username)
                .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            foreach (String friendConnected in listOfFriendsConnected)
            {
                await WarnUserOfFriendsConnected(friendConnected, result);
            }
        }

        public async Task WarnFriendsOfUserOfDisconnection(String username, WebSocketReceiveResult result)
        {
            User userDisconnected = _usersCRUDService.GetByUsername(username);
            List<String> listOfFriendsConnected = _usersCRUDService.GetUsersById(userDisconnected.Friends)
                .Select(s => s.Username)
                .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

            foreach (String friendConnected in listOfFriendsConnected)
            {
                await WarnUserOfFriendsConnected(friendConnected, result);
            }
        }

        public async Task WarnUserOfFriendsConnected(string username, WebSocketReceiveResult result)
        {
            User userFriendConnected = _usersCRUDService.GetByUsername(username);
            if (userFriendConnected != null)
            {
                //On récupère la liste des amis connectés de l'User
                List<String> listOfFriendsConnected = _usersCRUDService.GetUsersById(userFriendConnected.Friends)
                    .Select(s => s.Username)
                    .Intersect(new List<string>(mapActivePlayersWebsocket.Keys)).ToList();

                byte[] bytesFriendsConnected = listOfFriendsConnected
                    .SelectMany(s => Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();

                WebSocket websocketFriendUserConnected = mapActivePlayersWebsocket[username];
                await websocketFriendUserConnected.SendAsync(bytesFriendsConnected, result.MessageType,
                    result.EndOfMessage, CancellationToken.None);
            }
        }


        public async Task BuyCard(string username, string pantheon, WebSocket webSocket, WebSocketReceiveResult result)
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

                await WarnUserPurchaseCard(webSocket, result, card.Id);
            }
            else
            {
                await WarnUserNotEnoughDisciples(webSocket, result); 
            }
        }

        private async Task WarnUserNotEnoughDisciples(WebSocket webSocket, WebSocketReceiveResult result)
        {
            byte[] byteNotEnoughDisciples = Encoding.UTF8.GetBytes("L'utilisateur ne possède pas assez de disciples");
            await webSocket.SendAsync(byteNotEnoughDisciples, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

        public Boolean CanAffordCard(User user)
        {
            return user.Disciples >= _cardsService.priceOfCard;
        }

        public async Task WarnUserPurchaseCard(WebSocket webSocket, WebSocketReceiveResult result, String cardId)
        {
            byte[] byteCardObtained = Encoding.UTF8.GetBytes(cardId);
            await webSocket.SendAsync(byteCardObtained, result.MessageType, result.EndOfMessage, CancellationToken.None); 
        }
    }
}