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
            _settings.DatabaseName = "Divality";
            _settings.UsersCollectionName = "Users";
            
            _usersCrudService = new UsersCRUDService(_settings);
        }

        [TestMethod]
        public void UnitTest()
        {
            Assert.IsTrue(true);
        }
    }
}