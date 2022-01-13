using System;
using System.Collections.Generic;
using DivalityBack.Models;
using MongoDB.Driver;

namespace DivalityBack.Services.CRUD
{
    public class FriendRequestsCRUDService
    {
        private readonly IMongoCollection<FriendRequest> _friendRequests ;

        public FriendRequestsCRUDService(IDivalityDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _friendRequests = database.GetCollection<FriendRequest>(settings.FriendRequestsCollectionName);
        }

        public List<FriendRequest> Get() =>
            _friendRequests.Find(friendRequest => true).ToList();

        public FriendRequest Get(string id) =>
            _friendRequests.Find<FriendRequest>(_friendRequests => _friendRequests.Id == id).FirstOrDefault();

        public FriendRequest Create(FriendRequest friendRequest)
        {
            _friendRequests.InsertOne(friendRequest);
            return friendRequest;
        }

        public void Update(string id, FriendRequest friendRequestIn) =>
            _friendRequests.ReplaceOne(friendRequest => friendRequest.Id == id, friendRequestIn);

        public void Remove(FriendRequest friendRequestIn) =>
            _friendRequests.DeleteOne(friendRequest => friendRequest.Id == friendRequestIn.Id);

        public void Remove(string id) =>
            _friendRequests.DeleteOne(card => card.Id == id);

        public List<FriendRequest> FindByReceiver(string idReceiver)
        {
            return _friendRequests.Find(request => request.Receiver.Equals(idReceiver)).ToList();
            
        }

        public FriendRequest FindBySenderAndReceiver(string idSender, String idReceiver)
        {
            return _friendRequests
                .Find(request => request.Sender.Equals(idSender) && request.Receiver.Equals(idReceiver))
                .FirstOrDefault();
        }
    }
}