using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;

namespace DivalityBack.Tests
{
    [TestClass]
    public class UsersServiceTests
    {
        private static UsersService _usersService;
        private static CardsService _cardsService;
        private static UtilServices _utilService;
        private static CardsCRUDService _cardsCrudService; 
        private static UsersCRUDService _usersCrudService; 

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _cardsService = new CardsService(new CardsCRUDService(SetupAssemblyInitializer._settings));
            _usersCrudService = new UsersCRUDService(SetupAssemblyInitializer._settings);
            _cardsCrudService = new CardsCRUDService(SetupAssemblyInitializer._settings);
            _utilService = new UtilServices(_cardsCrudService); 
            _usersService = new UsersService(_usersCrudService, _cardsService, _utilService);
        }
        
        [TestMethod]
        public void Hash_Two_Same_Passwords_Same_Result()
        {
            string passwordHash1 = _usersService.HashPassword("passwordTest");
            string passwordHash2 = _usersService.HashPassword("passwordTest");
            Assert.IsTrue(passwordHash1.Equals(passwordHash2), "Hashing the same string twice does not returns the same value");
        }
        
        [TestMethod]
        public void Hash_Two_Different_Passwords_Different_Result()
        {
            string passwordHash1 = _usersService.HashPassword("passwordTest1");
            string passwordHash2 = _usersService.HashPassword("passwordTest2");
            Assert.IsFalse(passwordHash1.Equals(passwordHash2), "Hashing different strings twice does returns the same value");
        }
        
        [TestMethod]
        public void Hashed_Password_Different_From_Original_Password()
        {
            Assert.IsFalse(_usersService.HashPassword("passwordTest").Equals("passwordTest"), "Hashing a password does return the same value as the password given");
        }

        [TestMethod]
        public void Sign_In_User_With_Correct_Informations_Returns_This_User()
        {
            //On créé un User pour le test
            User newUser = new User();
            newUser.Username = "UserTestSignIn";
            newUser.Password = _usersService.HashPassword("PasswordTest");
            newUser.Victory = 10;
            newUser.Defeat = 5;
            _usersCrudService.Create(newUser);

            String userInDbInfo = _usersService.SignIn("UserTestSignIn", "PasswordTest");
            Assert.IsNotNull(userInDbInfo);
            
            _usersCrudService.Remove(newUser);
        }
        
        [TestMethod]
        public void Sign_In_User_With_Wrong_Informations_Returns_Null()
        {
            //On créé un User pour le test
            User newUser = new User();
            newUser.Username = "UserTestSignIn";
            newUser.Password = _usersService.HashPassword("PasswordTest");
            newUser.Victory = 10;
            newUser.Defeat = 5;
            _usersCrudService.Create(newUser);

            String userInDbWrongPasswordInfo = _usersService.SignIn("UserTestSignIn", "Wrong");
            Assert.IsNull(userInDbWrongPasswordInfo);
            
            String userInDbWrongUsernameInfo = _usersService.SignIn("Wrong", "PasswordTest");
            Assert.IsNull(userInDbWrongUsernameInfo);
            
            String userInDbWrongUsernameAndPasswordInfo = _usersService.SignIn("Wrong", "Wrong");
            Assert.IsNull(userInDbWrongUsernameAndPasswordInfo);
            
            _usersCrudService.Remove(newUser);
        }

        [TestMethod]
        public void Can_Afford_Card_Returns_True_When_User_Can_Afford_A_Card()
        {
            User user = _usersCrudService.Get("61c346cfead236cba80ecec5");
            Assert.IsTrue(_usersService.CanAffordCard(user), "La méthode CanAffordCard renvoie false alors que l'user peut acheter une carte");
        }
        
        [TestMethod]
        public void Can_Afford_Card_Returns_False_When_User_Cannot_Afford_A_Card()
        {
            User user = _usersCrudService.Get("61c34840ead236cba80ecec6");
            Assert.IsFalse(_usersService.CanAffordCard(user), "La méthode CanAffordCard renvoie true alors que l'user ne peut pas acheter de carte");
        }
    }
}
