using System;
using System.Collections.Generic;
using DivalityBack.Models;
using MongoDB.Driver;

namespace DivalityBack.Services.CRUD
{
    public class CardsCRUDService
    {
        private readonly IMongoCollection<Card> _cards ;

        public CardsCRUDService(IDivalityDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _cards = database.GetCollection<Card>(settings.CardsCollectionName);
        }

        public List<Card> Get() =>
            _cards.Find(card => true).ToList();

        public Card Get(string id) =>
            _cards.Find<Card>(_cards => _cards.Id == id).FirstOrDefault();
        
        public Card Create(Card card)
        {
            _cards.InsertOne(card);
            return card;
        }

        public void Update(string id, Card cardIn) =>
            _cards.ReplaceOne(card => card.Id == id, cardIn);

        public void Remove(Card cardIn) =>
            _cards.DeleteOne(card => card.Id == cardIn.Id);

        public void Remove(string id) =>
            _cards.DeleteOne(card => card.Id == id);

        public List<Card> GetCardsByPantheonAndRarity(string pantheon, string rarity)
        {
            return _cards.Find(card =>
                card.Pantheon.Equals(pantheon) && card.Rarity.Equals(rarity) &&
                ((card.isLimited && card.Available > 0) || !card.isLimited)).ToList(); 
        }

        public Card GetCardByName(String name)
        {
            return _cards.Find(card => card.Name.Equals(name)).FirstOrDefault(); 
        }

        public List<Card> Get(List<String> cardsId)
        {
            List<Card> listCard = new List<Card>(); 
          
            foreach (String id in cardsId)
            {
                listCard.Add(Get(id));
            }

            return listCard; 
        }
    }
}
