using System.Collections.Generic;
using DivalityBack.Models;
using Microsoft.AspNetCore.Server.HttpSys;
using MongoDB.Driver;

namespace DivalityBack.Services.CRUD
{
    public class AuctionHousesCRUDService
    {
        private readonly IMongoCollection<AuctionHouse> _auctionHouses ;

        public AuctionHousesCRUDService(IDivalityDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _auctionHouses = database.GetCollection<AuctionHouse>(settings.AuctionHouseCollectionName);
        }

        public List<AuctionHouse> Get() =>
            _auctionHouses.Find(auctionHouse => true).ToList();

        public AuctionHouse Get(string id) =>
            _auctionHouses.Find<AuctionHouse>(_auctionHouses => _auctionHouses.Id == id).FirstOrDefault();

        public AuctionHouse Create(AuctionHouse auctionHouse)
        {
            _auctionHouses.InsertOne(auctionHouse);
            return auctionHouse;
        }

        public void Update(string id, AuctionHouse auctionHouseIn) =>
            _auctionHouses.ReplaceOne(auctionHouse => auctionHouse.Id == id, auctionHouseIn);

        public void Remove(AuctionHouse auctionHouseIn) =>
            _auctionHouses.DeleteOne(auctionHouse => auctionHouse.Id == auctionHouseIn.Id);

        public void Remove(string id) =>
            _auctionHouses.DeleteOne(auctionHouse => auctionHouse.Id == id);

        public AuctionHouse GetByCardIdAndOwnerIdAndPrice(string cardId, string ownerId, string price)
        {
            return _auctionHouses.Find(auction =>
                auction.CardId.Equals(cardId) && auction.OwnerId.Equals(ownerId) &&
                auction.Price.Equals(int.Parse(price))).FirstOrDefault();
        }

        public List<AuctionHouse> GetByCardIdAndOwnerId(string cardId, string ownerId)
        {
            return _auctionHouses.Find(auction => auction.CardId.Equals(cardId) && auction.OwnerId.Equals(ownerId))
                .ToList();
        }
    }
}