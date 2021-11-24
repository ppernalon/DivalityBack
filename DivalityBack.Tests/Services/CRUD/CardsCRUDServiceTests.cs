using System;
using System.Collections.Generic;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class CardsCRUDServiceTests
    {
        private static CardsCRUDService _cardsCrudService = null;
        private static IDivalityDatabaseSettings _settings = null;
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _settings = new DivalityDatabaseSettings();
            
            // C'est pas propre mais j'ai pas trouvé de moyen pour accéder aux properties depuis le projet de test
            _settings.ConnectionString =
                "mongodb+srv://App:Sc7DflVYPuqlTel4@divality.gouyd.mongodb.net/Divality?retryWrites=true&w=majority";
            _settings.DatabaseName = "DivalityTest";
            _settings.CardsCollectionName = "Cards";
            
            _cardsCrudService = new CardsCRUDService(_settings);
        }

        [TestMethod]
        public void Create_Card_Correct_Informations_In_Db()
        {
            
            Card newCard = new Card();
            //On remplit les informations avec des données de test
            newCard.Armor = 10; 
            newCard.Available = 20;
            newCard.Distributed = 5;
            newCard.isLimited = true;
            newCard.Life = 40;
            newCard.Name = "CardTest";
            newCard.Pantheon = "PantheonTest";
            newCard.Power = 15;
            newCard.Rarity = "LEGENDAIRE DOREEEEE";
            newCard.Speed = 20;

            //On créé la carte en base
            Card newCardSavedInDb = _cardsCrudService.Create(newCard);
            
            //On vérifie que la carte créée est bien trouvable en base
            Assert.IsNotNull(_cardsCrudService.Get(newCardSavedInDb.Id), "La carte n'est pas trouvable en base de données");
            
            //On vérifie ensuite que ses données sont les mêmes que celles entrées
            Assert.IsTrue(newCard.Equals(newCardSavedInDb), "Les données enregistrées en base de données sont différentes de celles envoyées");
            
            //On supprime la carte à la fin du test pour ne pas polluer la base
            _cardsCrudService.Remove(newCardSavedInDb);
        }

        [TestMethod]
        public void Get_One_Card_Returns_Correct_Informations()
        {
            //Cette carte est une carte créé à la main pour les tests uniquement
            //il ne faut surtout pas la modifier ou la supprimer
            Card cardInDb = _cardsCrudService.Get("619dea7a9d2400c0e324547b");
            
            //On vérifie qu'on arrive bien à le trouver en base de données
            Assert.IsNotNull(cardInDb, "L'User est introuvable en base de données");
            
            //On  vérifie que les informations remontées sont correctes
            Assert.IsTrue(cardInDb.Name.Equals("CardTestGetOne"), "Le nom de la carte en base n'est pas celui attendu");
            Assert.IsTrue(cardInDb.Pantheon.Equals("PantheonTest"), "Le pantheon de la carte en base n'est pas celui attendu");
            Assert.IsTrue(cardInDb.Rarity.Equals("Rare"), "La rareté de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.Life.Equals(40), "La vie de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.Speed.Equals(20), "La vitesse de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.Power.Equals(15), "La force de la carte en base n'est pas celle attendue");
            Assert.IsNull(cardInDb.OffensiveAbility, "L'abilité offensive de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.OffensiveEfficiency.Equals(0), "L'efficacité offensive de la carte en base n'est pas celle attendue");
            Assert.IsNull(cardInDb.DefensiveAbility, "L'habilité défensive de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.DefensiveEfficiency.Equals(0), "L'efficacité défensive de la carte en base n'est pas celle attendue");
            Assert.IsTrue(cardInDb.isLimited.Equals(true), "Le caractère limité de la carte en base n'est pas celui attendu");
            Assert.IsTrue(cardInDb.Distributed.Equals(5), "Le nombre de cartes distribuées en base n'est pas celui attendu");
            Assert.IsTrue(cardInDb.Available.Equals(20), "Le nombre de cartes disponibles en base n'est pas celui attendu");
        }

        [TestMethod]
        public void Get_All_Cards_Returns_Correct_Informations()
        {
            List<Card> allCardsInDb = _cardsCrudService.Get(); 
            
            //On teste que la liste des cartes récupérée n'est pas nulle, nous avons au moins une donnée de test en base
            Assert.IsNotNull(allCardsInDb, "Aucune carte n'a pu être récupérée en base de données");
            
            //On vérifie que les données récupérées sont bien des Cards
            Assert.IsInstanceOfType(allCardsInDb[0], typeof(Card), "Les objets récupérés en base de données ne sont pas du type Card");
            
        }

        [TestMethod]
        public void Update_One_Card_With_Correct_Informations()
        {
            //On créé une Carte pour le test
            Card newCard = new Card();
            
            newCard.Armor = 10; 
            newCard.Available = 20;
            newCard.Distributed = 5;
            newCard.isLimited = true;
            newCard.Life = 40;
            newCard.Name = "CardTestUpdate";
            newCard.Pantheon = "PantheonTest";
            newCard.Power = 15;
            newCard.Rarity = "Commune";
            newCard.Speed = 20;
            
            _cardsCrudService.Create(newCard);

            newCard.Name = "CardTestUpdateModified";
            
            _cardsCrudService.Update(newCard.Id, newCard);
            //On récupère la carte modifiée en base afin de faire les tests
            Card cardInDatabase = _cardsCrudService.Get(newCard.Id); 

            Assert.IsTrue(cardInDatabase.Id.Equals(newCard.Id), "La carte trouvée en base et celle qu'on a modifié sont différentes");
            Assert.IsFalse(cardInDatabase.Name.Equals("CardTestUpdate"), "Le nom de la carte n'a pas été modifié en base de données");
            Assert.IsTrue(newCard.Id.Equals(cardInDatabase.Id), "L'Id a été modifié pendant la modification de l'Username");

            //On supprime l'User de la base de données
            _cardsCrudService.Remove(cardInDatabase);
        }

        [TestMethod]
        public void Remove_One_User_From_Database()
        {
            //On créé une Carte pour le test
            Card newCard = new Card();
            
            newCard.Armor = 10; 
            newCard.Available = 20;
            newCard.Distributed = 5;
            newCard.isLimited = true;
            newCard.Life = 40;
            newCard.Name = "CardTestRemove";
            newCard.Pantheon = "PantheonTest";
            newCard.Power = 15;
            newCard.Rarity = "Commune";
            newCard.Speed = 20;

            Card cardInDb = _cardsCrudService.Create(newCard);
            Assert.IsNotNull(cardInDb, "La carte de Test n'a pas été créée en base de données");
            
            _cardsCrudService.Remove(cardInDb);
            Assert.IsNull(_cardsCrudService.Get(cardInDb.Id), "La carte n'a pas été supprimée de la base de données");
        } 
    }
}