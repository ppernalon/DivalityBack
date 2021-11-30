using System;
using MongoDB.Driver;
using System.Collections.Generic;
using DivalityBack.Models;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Services.CRUD
{
    public class UsersCRUDService
    {
        private readonly IMongoCollection<User> _users ;
        public UsersCRUDService(IDivalityDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public List<User> Get() =>
            _users.Find(user => true).ToList();

        public User Get(string id) =>
            _users.Find<User>(_users => _users.Id == id).FirstOrDefault();

        public User Create(User user)
        {
            _users.InsertOne(user);
            return user;
        }

        public void Update(string id, User userIn) =>
            _users.ReplaceOne(user => user.Id == id, userIn);

        public void Remove(User userIn) =>
            _users.DeleteOne(user => user.Id == userIn.Id);

        public void Remove(string id) =>
            _users.DeleteOne(user => user.Id == id);

        public User GetByUsername(String username)
        {
            User user = _users.Find(_users => _users.Username.Equals(username)).FirstOrDefault();
            return user;
        }

        public User GetByUsernameAndPassword(String username, String password)
        {
            User user = _users.Find(_users => _users.Username.Equals(username) && _users.Password.Equals(password))
                .FirstOrDefault();
            return user; 
        }
        
    }
}
