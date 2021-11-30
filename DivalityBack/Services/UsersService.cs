using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace Divality.Services
{
    public class UsersService
    {
        private readonly UsersCRUDService _usersCRUDService;
        public UsersService(UsersCRUDService usersCRUDService)
        {
            _usersCRUDService = usersCRUDService;
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
            Console.Write("USERJSON : "+userJson.ToString());
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
    }
}