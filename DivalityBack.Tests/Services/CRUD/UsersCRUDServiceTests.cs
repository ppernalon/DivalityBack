using System;
using System.Collections.Generic;
using Divality.Services;
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
        public void Create_User_Correct_Informations_In_Db()
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
            Assert.IsTrue(newUser.Equals(newUserSavedInDb), "L'User en base et celui entré sont différents");
            
            //On supprime le User à la fin du test pour ne pas polluer la base
            _usersCrudService.Remove(newUserSavedInDb);
        }

        [TestMethod]
        public void Get_One_User_Returns_Correct_Informations()
        {
            //Cet User est un User créé à la main pour les tests uniquement
            //il ne faut surtout pas le modifier ou le supprimer
            User userInDb = _usersCrudService.Get("619d14e4494e6d757649e48d");
            
            //On vérifie qu'on arrive bien à le trouver en base de données
            Assert.IsNotNull(userInDb, "L'User est introuvable en base de données");
            
            //On  vérifie que les informations remontées sont correctes
            Assert.IsTrue(userInDb.Username.Equals("UserTestGetOneUser"), "L'Username de l'User remonté n'est celui attendu ");
            Assert.IsTrue(userInDb.Password.Equals("PasswordTestGetOneUser"), "Le Password de l'User remonté n'est celui attendu ");
            Assert.IsTrue(userInDb.Victory.Equals(15), "Le nombre de victoires de l'User remonté n'est celui attendu ");
            Assert.IsTrue(userInDb.Defeat.Equals(2), "Le nombre de défaites de l'User remonté n'est celui attendu ");
            Assert.IsTrue(userInDb.Disciples.Equals(111), "Le nombre de disciples de l'User remonté n'est celui attendu ");
        }

        [TestMethod]
        public void Get_All_Users_Returns_Correct_Informations()
        {
            List<User> allUsersInDb = _usersCrudService.Get(); 
            
            //On teste que la liste des Users récupérée n'est pas nulle, nous avons au moins une donnée de test en base
            Assert.IsNotNull(allUsersInDb, "Aucun User n'a pu être récupéré en base de données");
            
            //On vérifie que les données récupérées sont bien des Users
            Assert.IsInstanceOfType(allUsersInDb[0], typeof(User), "Les objets récupérés en base de données ne sont pas du type User");
            
        }

        [TestMethod]
        public void Update_One_User_With_Correct_Informations()
        {
            //On créé un User pour le test
            User newUser = new User();
            newUser.Username = "UserTestUpdate";
            newUser.Password = "PasswordTest";
            newUser.Victory = 10;
            newUser.Defeat = 5;
            
            _usersCrudService.Create(newUser);

            newUser.Username = "UserTestUpdateModified";
            
            _usersCrudService.Update(newUser.Id, newUser);
            //On récupère l'User modifié en base afin de faire les tests
            User userInDatabase = _usersCrudService.Get(newUser.Id); 
            
            Assert.IsTrue(userInDatabase.Id.Equals(newUser.Id), "L'User trouvé en base et celui qu'on a modifié sont différents");
            Assert.IsFalse(userInDatabase.Username.Equals("UserTestUpdate"), "L'Username n'a pas été modifié en base de données");
            Assert.IsTrue(newUser.Id.Equals(userInDatabase.Id), "L'Id a été modifié pendant la modification de l'Username");
            //On supprime l'User de la base de données
            _usersCrudService.Remove(userInDatabase);
        }

        [TestMethod]
        public void Remove_One_User_From_Database()
        {
            //On créé un User pour le test
            User newUser = new User();
            newUser.Username = "UserTestRemove";
            newUser.Password = "PasswordTest";
            newUser.Victory = 10;
            newUser.Defeat = 5;
            User userInDb = _usersCrudService.Create(newUser);
            
            Assert.IsNotNull(userInDb, "L'User de Test n'a pas été créé en base de données");
            
            _usersCrudService.Remove(userInDb);
            Assert.IsNull(_usersCrudService.Get(userInDb.Id), "L'User n'a pas été supprimé de la base de données");
        }
    }
}