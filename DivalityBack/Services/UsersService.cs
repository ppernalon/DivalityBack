using System;
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
            //On remplit l'username et le password depuis le body de la requête POST;
            newUser.Username = userJson.GetProperty("username").GetString();
            //On hash le password
            newUser.Password = HashPassword(userJson.GetProperty("password").GetString());
        
            //On créé l'entrée en base
            return _usersCRUDService.Create(newUser);
        }

        public User SignIn(string username, string password)
        {
            password = HashPassword(password);
            User user = _usersCRUDService.GetByUsernameAndPassword(username, password);
            return user;
        }
    }
}