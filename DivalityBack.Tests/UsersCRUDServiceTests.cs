using System;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class UsersCRUDServiceTests
    {
        private static UsersCRUDService _usersCrudService = null;
        private static IDivalityDatabaseSettings _settings = null;
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _settings = new DivalityDatabaseSettings();
            
            // C'est pas propre mais j'ai pas trouvé de moyen pour accéder aux properties depuis le projet de test
            _settings.ConnectionString =
                "mongodb+srv://App:Sc7DflVYPuqlTel4@divality.gouyd.mongodb.net/Divality?retryWrites=true&w=majority";
            _settings.DatabaseName = "DivalityTest";
            _settings.UsersCollectionName = "Users";
            
            _usersCrudService = new UsersCRUDService(_settings);
        }

        [TestMethod]
        public void Create_User_With_Correct_Informations_In_Db()
        {
            //On récupère la date du moment pour avoir un identifiant unique pour chaque user créé par un test
            DateTime date = DateTime.Now;
            
            User newUser = new User();
            
            //On remplit les informations avec des données de test
            newUser.Username = "UserTest_" + date.ToString();
            newUser.Password = "PasswordTest_" + date.ToString();
            newUser.Victory = 10;
            newUser.Defeat = 15;
            newUser.Disciples = 100; 
            
            //On créé l'user en base
            User newUserSavedInDb = _usersCrudService.Create(newUser);
            
            //On vérifie que l'user créé est bien trouvable en base
            Assert.IsNotNull(_usersCrudService.Get(newUserSavedInDb.Id), "L'User n'est pas trouvable en base de données");
            
            //On vérifie ensuite que ses données sont les mêmes que celles entrées
            Assert.IsTrue(newUser.Username.Equals(newUserSavedInDb.Username), "L'Username en base et celui entré sont différents");
            Assert.IsTrue(newUser.Password.Equals(newUserSavedInDb.Password),"Le password en base et celui entré sont différents");
            Assert.IsTrue(newUser.Victory.Equals(newUserSavedInDb.Victory), "Le nombre de victoires en base et celui entré sont différents");
            Assert.IsTrue(newUser.Defeat.Equals(newUserSavedInDb.Defeat), "Le nombre de défaites en base et celui entré sont différents");
            Assert.IsTrue(newUser.Disciples.Equals(newUserSavedInDb.Disciples), "Le nombre de disciples en base et celui entré sont différents");
        }
    }
}