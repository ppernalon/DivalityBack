using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class AuctionHouseServiceTests
    {
        private static AuctionHousesCRUDService _auctionHousesCrudService;
        private static UsersCRUDService _usersCrudService;
        private static CardsCRUDService _cardsCrudService;
        private static UtilServices _utilServices; 
        private static AuctionHouseService _auctionHouseService;
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _auctionHousesCrudService = new AuctionHousesCRUDService(SetupAssemblyInitializer._settings);
            _usersCrudService = new UsersCRUDService(SetupAssemblyInitializer._settings); 
            _cardsCrudService = new CardsCRUDService(SetupAssemblyInitializer._settings);
            _utilServices = new UtilServices(_cardsCrudService, _usersCrudService); 
            _auctionHouseService = new AuctionHouseService(_auctionHousesCrudService, _cardsCrudService, _usersCrudService, _utilServices); 
        }

        [TestMethod]
        public void Sell_Card_In_Auction_House_Create_Entry_In_Table()
        {
            User backup = _usersCrudService.GetByUsername("UserTestGetOneUser"); 
            _auctionHouseService.SellCardInAuctionHouse(null, null, "UserTestGetOneUser", "CardTestGenerateIsLimitedLeg", "100");
            User user = _usersCrudService.GetByUsername("UserTestGetOneUser");
            AuctionHouse auction =
                _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice("61ddb2ab18a572d283307e1f",
                    "619d14e4494e6d757649e48d", "100");
            Assert.IsNotNull(auction, "La vente n'a pas été trouvée en base");
            Assert.IsTrue(!user.Collection.Contains("61ddb2ab18a572d283307e1f"), "La carte n'a pas été enlevée de la collection");
            
            _auctionHousesCrudService.Remove(auction);
            _usersCrudService.Update(user.Id, backup);
        }

        [TestMethod]
        public void Sell_Card_In_Auction_House_Without_The_Card_Does_Nothing()
        {
            User user = _usersCrudService.GetByUsername("UserTestGetOneUser");
            _auctionHouseService.SellCardInAuctionHouse(null, null, "UserTestGetOneUser", "CardTestGenerateIsLimitedRare", "100");
            AuctionHouse auction =
                _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice("61ddbeeb18a572d283307e24",
                    "619d14e4494e6d757649e48d", "100");
            Assert.IsNull(auction, "La vente a été trouvée en base");
            Assert.IsTrue(!user.Collection.Contains("61ddbeeb18a572d283307e24"), "La carte est dans la collection");
        }

        [TestMethod]
        public void Buy_Card_In_Auction_House_Remove_Entry_From_Table()
        {
            User backupOwner =  _usersCrudService.GetByUsername("UserTestGetOneUser");
            _auctionHouseService.SellCardInAuctionHouse(null, null, "UserTestGetOneUser", "CardTestGenerateIsLimitedLeg", "100");

            User backupUser = _usersCrudService.GetByUsername("UserTestUpdateModified");

            _auctionHouseService.BuyCardInAuctionHouse(null, null, "UserTestUpdateModified",
                "CardTestGenerateIsLimitedLeg", "UserTestGetOneUser", "100");
            
            AuctionHouse auction =
                _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice("61ddb2ab18a572d283307e1f",
                    "619d14e4494e6d757649e48d", "100");
            
            User owner = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User user = _usersCrudService.GetByUsername("UserTestUpdateModified");
            Assert.IsTrue(owner.Disciples.Equals(211),"Le nombre de disciples du vendeur n'a pas augmenté");
            Assert.IsTrue(user.Disciples.Equals(10), "Le nombre de disciples de l'acheteur n'est pas descendu");
            Assert.IsTrue(user.Collection.Contains("61ddb2ab18a572d283307e1f"), "La carte n'a pas été ajoutée à la collection de l'acheteur");
            Assert.IsNull(auction, "La vente n'a pas été retirée de l'HdV");

            _usersCrudService.Update(owner.Id, backupOwner);
            _usersCrudService.Update(user.Id, backupUser);
        }

        [TestMethod]
        public void Buy_Card_In_Auction_House_Without_Enough_Disciples_Does_Nothing()
        {
            User backupOwner = _usersCrudService.GetByUsername("UserTestGetOneUser"); 
            _auctionHouseService.SellCardInAuctionHouse(null, null, "UserTestGetOneUser", "CardTestGenerateIsLimitedLeg", "100");
          
            
            _auctionHouseService.BuyCardInAuctionHouse(null, null, "UserTestCanAffordCardFalse",
                "CardTestGenerateIsLimitedLeg", "UserTestGetOneUser", "100");
            
            AuctionHouse auction =
                _auctionHousesCrudService.GetByCardIdAndOwnerIdAndPrice("61ddb2ab18a572d283307e1f",
                    "619d14e4494e6d757649e48d", "100");
            
            User owner = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User user = _usersCrudService.GetByUsername("UserTestCanAffordCardFalse");
            
            Assert.IsTrue(owner.Disciples.Equals(111),"Le nombre de disciples du vendeur a augmenté");
            Assert.IsTrue(user.Disciples.Equals(0), "Le nombre de disciples de l'acheteur est descendu");
            Assert.IsTrue(!user.Collection.Contains("61ddb2ab18a572d283307e1f"), "La carte a été ajoutée à la collection de l'acheteur");
            Assert.IsNotNull(auction, "La vente a été retirée de l'HdV");

            _usersCrudService.Update(owner.Id, backupOwner);
            _auctionHousesCrudService.Remove(auction);
        }
    }
}