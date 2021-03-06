using System;
using System.Globalization;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class CardsServiceTests
    {
        private static CardsCRUDService _cardsCrudService;
        private static CardsService _cardsService; 
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _cardsCrudService = new CardsCRUDService(SetupAssemblyInitializer._settings);
            _cardsService = new CardsService(_cardsCrudService); 
        }

        [TestMethod]
        public void Generate_Card_With_Good_Pantheon_Returns_A_Card()
        {
            Card card = _cardsService.GenerateNewCard("Nordique");
            card.Distributed--; 
            _cardsCrudService.Update(card.Id, card);
            Assert.IsNotNull(card, "Aucune carte n'a été remontée");
        }

        [TestMethod]
        public void Generate_Card_With_Good_Pantheon_Does_Not_Ignore_Case()
        {
            Card card = _cardsService.GenerateNewCard("NoRdIQuE");
            Assert.IsNull(card, "Une carte a été remontée");
        }

        [TestMethod]
        public void Generate_Card_With_Wrong_Pantheon_Returns_Null()
        {
            Card card = _cardsService.GenerateNewCard("auifgaeuifae");
            Assert.IsNull(card, "Une carte a été remontée");
        }

        [TestMethod]
        public void Generate_Rarity_Can_Generate_Commune()
        {
            String rarity = "";
            int compteur = 0; 
            //On ne fait que 1000 essais 
            while (!rarity.Equals("Commune") && compteur <= 1000)
            {
                rarity = _cardsService.GenerateRarity();
                compteur++; 
            }
            Assert.IsTrue(rarity.Equals("Commune"), "Sur 1000 essais, nous n'avons eu aucune commune");
        }
        
        [TestMethod]
        public void Generate_Rarity_Can_Generate_Rare()
        {
            String rarity = "";
            int compteur = 0; 
            //On ne fait que 1000 essais 
            while (!rarity.Equals("Rare") && compteur <= 1000)
            {
                rarity = _cardsService.GenerateRarity();
                compteur++; 
            }
            Assert.IsTrue(rarity.Equals("Rare"), "Sur 1000 essais, nous n'avons eu aucune rare");
        }
        
        [TestMethod]
        public void Generate_Rarity_Can_Generate_Legendaire()
        {
            String rarity = "";
            int compteur = 0; 
            //On ne fait que 1000 essais 
            while (!rarity.Equals("Légendaire") && compteur <= 1000)
            {
                rarity = _cardsService.GenerateRarity();
                compteur++; 
            }
            Assert.IsTrue(rarity.Equals("Légendaire"), "Sur 1000 essais, nous n'avons eu aucune legendaire");
        }

        [TestMethod]
        public void Generate_Limited_Card_Increase_Distributed_Counter()
        {
            Card card = _cardsService.GenerateNewCard("Limited");
            Assert.IsTrue(card.Distributed.Equals(6), "Le nombre de carte distribuées n'a pas été mis à jour");

            card.Distributed = 5;
            _cardsCrudService.Update(card.Id, card);
        }
    }
}