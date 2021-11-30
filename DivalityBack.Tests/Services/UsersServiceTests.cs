using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;

namespace DivalityBack.Tests
{
    [TestClass]
    public class UsersServiceTests
    {
        private static UsersService _usersService = null;
        private static UsersCRUDService _usersCrudService = null; 
        private static IDivalityDatabaseSettings _settings = null;

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _settings = new DivalityDatabaseSettings();
            _settings.ConnectionString =
                "mongodb+srv://App:Sc7DflVYPuqlTel4@divality.gouyd.mongodb.net/Divality?retryWrites=true&w=majority";
            _settings.DatabaseName = "DivalityTest";
            _settings.UsersCollectionName = "Users";
            _usersCrudService = new UsersCRUDService(_settings);
            _usersService = new UsersService(_usersCrudService);
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
        }
        
    }

}
