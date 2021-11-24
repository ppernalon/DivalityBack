using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Divality.Services;

namespace DivalityBack.Tests
{
    [TestClass]
    public class UsersServiceTests
    {
        private static UsersService _usersService = null; 
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        { 
            _usersService = new UsersService();
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
    }

}
