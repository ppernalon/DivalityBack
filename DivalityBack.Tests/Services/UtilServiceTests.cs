using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class UtilServiceTests
    {
        private static CardsCRUDService _cardsCrudService;
        private static UsersCRUDService _usersCrudService;
        private static UtilServices _utilServices;
        private static AuctionHousesCRUDService _auctionHousesCrudService;
        
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _cardsCrudService = new CardsCRUDService(SetupAssemblyInitializer._settings);
            _usersCrudService = new UsersCRUDService(SetupAssemblyInitializer._settings);
            _utilServices = new UtilServices(_cardsCrudService, _usersCrudService);
            _auctionHousesCrudService = new AuctionHousesCRUDService(SetupAssemblyInitializer._settings); 
        }

        [TestMethod]
        public void Card_To_Json_Returns_Correct_Informations()
        {
            Card card = _cardsCrudService.Get("619dea7a9d2400c0e324547b");
            String jsonCard = _utilServices.CardToJson(card);

            JsonDocument json = JsonDocument.Parse(jsonCard);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("card"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(root.GetProperty("name").ToString().Equals(card.Name),"Le nom de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("armor").ToString().Equals(card.Armor.ToString()),"L'armure de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("available").ToString().Equals(card.Available.ToString()),"Le nombre de cartes disponibles n'est pas correct");
            Assert.IsTrue(root.GetProperty("distributed").ToString().Equals(card.Distributed.ToString()),"Le nombre de cartes distribuées n'est pas correct");
            Assert.IsTrue(root.GetProperty("isLimited").ToString().Equals(card.isLimited.ToString()),"Le caractère limité de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("life").ToString().Equals(card.Life.ToString()),"La vie de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("pantheon").ToString().Equals(card.Pantheon),"Le pantheon de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("power").ToString().Equals(card.Power.ToString()),"La force de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("rarity").ToString().Equals(card.Rarity),"La rareté de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("speed").ToString().Equals(card.Speed.ToString()),"La vitesse de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("defensiveAbility").ToString().Equals(card.DefensiveAbility),"L'habilité défensive de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("defensiveEfficiency").ToString().Equals(card.DefensiveEfficiency.ToString()),"L'efficacité défensive de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("offensiveAbility").ToString().Equals(card.OffensiveAbility),"L'habilité offensive de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("offensiveEfficiency").ToString().Equals(card.OffensiveEfficiency.ToString()),"L'efficacité offensive de la carte n'est pas correct");
        }

        [TestMethod]
        public void Collection_To_Json_Returns_Correct_Informations()
        {
            User user = _usersCrudService.Get("619d14e4494e6d757649e48d");
            String jsonCollection = _utilServices.CollectionToJson(user.Collection);
            List<String> listJsonCard = new List<string>();  
            foreach (string cardId in user.Collection)
            {
                Card card = _cardsCrudService.Get(cardId);
                listJsonCard.Add(_utilServices.CardToJson(card));
            }
            
            JsonDocument json = JsonDocument.Parse(jsonCollection);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("collection"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(listJsonCard.Contains(root.GetProperty("card").ToString()),
                "La collection n'est pas correctement transformée en Json");
        }

        [TestMethod]
        public void Friends_To_Json_Returns_Correct_Informations()
        {
            User user = _usersCrudService.Get("619de7a37c00c8a09bed8bc3");
            String jsonFriends = _utilServices.FriendsToJson(new List<string>(), user.Friends);
            
            JsonDocument json = JsonDocument.Parse(jsonFriends);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("friends"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(root.GetProperty("connected").ToString().Equals("{}"), "La liste des amis connectés n'est pas correcte");
            Assert.IsFalse(root.GetProperty("disconnected").ToString().Equals("{}"),"La liste des amis deconnectés n'est pas correcte");
        }

        [TestMethod]
        public void Auction_To_Json_Returns_Correct_Informations()
        {
            AuctionHouse auction = _auctionHousesCrudService.Get("619e48d71008623367670915");
            String jsonAuction = _utilServices.AuctionToJson(auction);

            String cardName = _cardsCrudService.Get(auction.CardId).Name;
            String ownerName = _usersCrudService.Get(auction.OwnerId).Username; 
            
            JsonDocument json = JsonDocument.Parse(jsonAuction);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("auction"),"Le type renvoyé n'est pas correct");
            Assert.IsTrue(root.GetProperty("cardName").ToString().Equals(cardName),
                "Le nom de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("ownerName").ToString().Equals(ownerName), "Le nom du détenteur de la carte n'est pas correct");
            Assert.IsTrue(root.GetProperty("price").ToString().Equals(auction.Price.ToString()), "Le prix de la carte n'est pas correct");
        }

        [TestMethod]
        public void List_Auction_To_Json_Returns_Correct_Informations()
        {
            List<AuctionHouse> listAuction = _auctionHousesCrudService.Get();
            List<String> listJsonAuction = new List<string>(); 
            String jsonListAuction = _utilServices.ListAuctionToJson(listAuction);
            
            foreach (AuctionHouse auction in listAuction)
            {
                listJsonAuction.Add(_utilServices.AuctionToJson(auction));
            }
            
            JsonDocument json = JsonDocument.Parse(jsonListAuction);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("auctionHouse"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(jsonListAuction.Contains(root.GetProperty("auction").ToString()),
                "La liste des opérations de l'HdV n'est pas correctement transformée en Json");
        }

        [TestMethod]
        public void Teams_To_Json_Returns_Correct_Informations()
        {
            List<Team> teams = _usersCrudService.GetByUsername("UserTestGetOneUser").Teams;
            String jsonTeams = _utilServices.TeamsToJson(teams); 
            
            JsonDocument json = JsonDocument.Parse(jsonTeams);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("teams"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(jsonTeams.Contains(root.GetProperty("team").ToString()),
                "Les équipes ne sont pas transformées en Json");
        }
        
        [TestMethod]
        public void Team_To_Json_Returns_Correct_Informations()
        {
            Team team = _usersCrudService.GetByUsername("UserTestGetOneUser").Teams.FirstOrDefault();
            String jsonTeam = _utilServices.TeamToJson(team); 
            
            JsonDocument json = JsonDocument.Parse(jsonTeam);
            JsonElement root = json.RootElement;
            
            Assert.IsTrue(root.GetProperty("type").ToString().Equals("team"), "Le type renvoyé n'est pas correct");
            Assert.IsTrue(jsonTeam.Contains(root.GetProperty("compo").ToString()),
                "L'équipe n'est pas transformée en Json"); 

        }
    }
}