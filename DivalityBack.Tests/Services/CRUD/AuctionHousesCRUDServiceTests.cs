using System;
using System.Collections.Generic;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class AuctionHousesCRUDServiceTests
    {
        private static AuctionHousesCRUDService _auctionHousesCrudService = null;
        private static IDivalityDatabaseSettings _settings = null;
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _settings = new DivalityDatabaseSettings();
            
            // C'est pas propre mais j'ai pas trouvé de moyen pour accéder aux properties depuis le projet de test
            _settings.ConnectionString =
                "mongodb+srv://App:Sc7DflVYPuqlTel4@divality.gouyd.mongodb.net/Divality?retryWrites=true&w=majority";
            _settings.DatabaseName = "DivalityTest";
            _settings.AuctionHouseCollectionName = "AuctionHouse";
            
            _auctionHousesCrudService = new AuctionHousesCRUDService(_settings);
        }

        [TestMethod]
        public void Create_Auction_House_Correct_Informations_In_Db()
        {
            
            AuctionHouse newAuctionHouse = new AuctionHouse();

            //On remplit les informations avec des données de test
            newAuctionHouse.Price = 100;
            newAuctionHouse.CardId = "CardIdTest";
            newAuctionHouse.OwnerId = "OwnerIdTest"; 
            
            //On créé l'offre en base
            AuctionHouse newAuctionHouseSavedInDb = _auctionHousesCrudService.Create(newAuctionHouse); 
            
            //On vérifie que l'offre créée est bien trouvable en base
            Assert.IsNotNull(_auctionHousesCrudService.Get(newAuctionHouseSavedInDb.Id), "L'offre n'est pas trouvable en base de données");
            
            //On vérifie ensuite que ses données sont les mêmes que celles entrées
            Assert.IsTrue(newAuctionHouse.Equals(newAuctionHouseSavedInDb), "Les données enregistrées en base de données sont différentes de celles envoyées");
            
            //On supprime la carte à la fin du test pour ne pas polluer la base
            _auctionHousesCrudService.Remove(newAuctionHouseSavedInDb);
        }

        [TestMethod]
        public void Get_One_Auction_House_Returns_Correct_Informations()
        {
            //Cette offre est une offre créée à la main pour les tests uniquement
            //il ne faut surtout pas la modifier ou la supprimer
            AuctionHouse auctionHouseInDb = _auctionHousesCrudService.Get("619e48d71008623367670915"); 
            
            //On vérifie qu'on arrive bien à le trouver en base de données
            Assert.IsNotNull(auctionHouseInDb, "L'offre est introuvable en base de données");
            
            //On  vérifie que les informations remontées sont correctes
            Assert.IsTrue(auctionHouseInDb.Price.Equals(100), "Le prix trouvé en base n'est pas celui attendu");
            Assert.IsTrue(auctionHouseInDb.CardId.Equals("CardIdTest"), "Le CardId trouvé en base n'est pas celui attendu");
            Assert.IsTrue(auctionHouseInDb.OwnerId.Equals("OwnerIdTest"), "Le OwnerId trouvé en base n'est pas celui attendu");
        }

        [TestMethod]
        public void Get_All_Auction_Houses_Returns_Correct_Informations()
        {
            List<AuctionHouse> allAuctionHousesInDb = _auctionHousesCrudService.Get(); 
            
            //On teste que la liste des offres récupérée n'est pas nulle, nous avons au moins une donnée de test en base
            Assert.IsNotNull(allAuctionHousesInDb, "Aucune offre n'a pu être récupérée en base de données");
            
            //On vérifie que les données récupérées sont bien des AuctionHouses
            Assert.IsInstanceOfType(allAuctionHousesInDb[0], typeof(AuctionHouse), "Les objets récupérés en base de données ne sont pas du type AuctionHouse");
            
        }

        [TestMethod]
        public void Update_One_Auction_House_With_Correct_Informations()
        {
            //On créé une Offre pour le test
            AuctionHouse newAuctionHouse = new AuctionHouse();
            newAuctionHouse.Price = 100;
            newAuctionHouse.CardId = "CardIdTest";
            newAuctionHouse.OwnerId = "OwnerIdTest"; 

            _auctionHousesCrudService.Create(newAuctionHouse);
          
            newAuctionHouse.Price = 110; 
            
            _auctionHousesCrudService.Update(newAuctionHouse.Id, newAuctionHouse);
            //On récupère l'offre modifiée en base afin de faire les tests
            AuctionHouse auctionHouseInDatabase = _auctionHousesCrudService.Get(newAuctionHouse.Id); 

            Assert.IsTrue(auctionHouseInDatabase.Id.Equals(newAuctionHouse.Id), "L'offre trouvée en base et celle qu'on a modifié sont différentes");
            Assert.IsFalse(auctionHouseInDatabase.Price.Equals(100), "Le prix de l'offre n'a pas été modifié en base de données");
            Assert.IsTrue(newAuctionHouse.Id.Equals(auctionHouseInDatabase.Id), "L'Id a été modifié pendant la modification du prix");

            //On supprime l'User de la base de données
            _auctionHousesCrudService.Remove(auctionHouseInDatabase);
        }

        [TestMethod]
        public void Remove_One_Auction_House_From_Database_By_Passing_Object()
        {
            //On créé une Offre pour le test
            AuctionHouse newAuctionHouse = new AuctionHouse();
            newAuctionHouse.Price = 100;
            newAuctionHouse.CardId = "CardIdTest";
            newAuctionHouse.OwnerId = "OwnerIdTest"; 

            AuctionHouse auctionHouseInDb = _auctionHousesCrudService.Create(newAuctionHouse);

            Assert.IsNotNull(auctionHouseInDb, "L'offre de Test n'a pas été créée en base de données");
            
            _auctionHousesCrudService.Remove(auctionHouseInDb);
            Assert.IsNull(_auctionHousesCrudService.Get(auctionHouseInDb.Id), "L'offre n'a pas été supprimée de la base de données");
        }
        
        [TestMethod]
        public void Remove_One_Card_From_Database_By_Passing_Id()
        {
            //On créé une Offre pour le test
            AuctionHouse newAuctionHouse = new AuctionHouse();
            newAuctionHouse.Price = 100;
            newAuctionHouse.CardId = "CardIdTest";
            newAuctionHouse.OwnerId = "OwnerIdTest"; 

            AuctionHouse auctionHouseInDb = _auctionHousesCrudService.Create(newAuctionHouse);

            Assert.IsNotNull(auctionHouseInDb, "L'offre de Test n'a pas été créée en base de données");
            
            _auctionHousesCrudService.Remove(auctionHouseInDb.Id);
            Assert.IsNull(_auctionHousesCrudService.Get(auctionHouseInDb.Id), "L'offre n'a pas été supprimée de la base de données");
        }
    }
}