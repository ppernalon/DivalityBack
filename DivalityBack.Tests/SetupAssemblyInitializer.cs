using DivalityBack.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{


    [TestClass]
    public class SetupAssemblyInitializer
    {
        public static DivalityDatabaseSettings _settings = new DivalityDatabaseSettings();

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();

            _settings.ConnectionString = config["DivalityDatabaseSettings:ConnectionString"];
            _settings.DatabaseName = config["DivalityDatabaseSettings:DatabaseName"];
            _settings.UsersCollectionName = config["DivalityDatabaseSettings:UsersCollectionName"];
            _settings.AuctionHouseCollectionName = config["DivalityDatabaseSettings:AuctionHouseCollectionName"];
            _settings.CardsCollectionName = config["DivalityDatabaseSettings:CardsCollectionName"];
            _settings.TeamsCollectionName = config["DivalityDatabaseSettings:TeamsCollectionName"];
            _settings.FriendRequestsCollectionName = config["DivalityDatabaseSettings:FriendRequestsCollectionName"];

        }
    }
}