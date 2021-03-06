using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private static FriendRequestsCRUDService _friendRequestsCrudService;

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _cardsService = new CardsService(new CardsCRUDService(SetupAssemblyInitializer._settings));
            _usersCrudService = new UsersCRUDService(SetupAssemblyInitializer._settings);
            _cardsCrudService = new CardsCRUDService(SetupAssemblyInitializer._settings);
            _friendRequestsCrudService = new FriendRequestsCRUDService(SetupAssemblyInitializer._settings);
            _utilService = new UtilServices(_cardsCrudService, _usersCrudService); 
            _usersService = new UsersService(_usersCrudService, _cardsCrudService, _friendRequestsCrudService, _cardsService, _utilService);
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
            //On cr???? un User pour le test
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
            //On cr???? un User pour le test
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
            Assert.IsTrue(_usersService.CanAffordCard(user), "La m??thode CanAffordCard renvoie false alors que l'user peut acheter une carte");
        }
        
        [TestMethod]
        public void Can_Afford_Card_Returns_False_When_User_Cannot_Afford_A_Card()
        {
            User user = _usersCrudService.Get("61c34840ead236cba80ecec6");
            Assert.IsFalse(_usersService.CanAffordCard(user), "La m??thode CanAffordCard renvoie true alors que l'user ne peut pas acheter de carte");
        }

        [TestMethod]
        public void SignUp_With_A_New_Username_Creates_Entry_In_DB()
        {
            var jsonInput= @"{
                        ""username"": ""newUsername"",
                        ""password"":""test""
                        }";
            
            JsonDocument doc = JsonDocument.Parse(jsonInput);
            JsonElement jsonUser = doc.RootElement;

            User user = _usersService.SignUp(jsonUser);
            Assert.IsNotNull(user, "L'username existe d??j??");

            User userInDb = _usersCrudService.GetByUsername("newUsername");
            Assert.IsNotNull(userInDb, "L'User n'a pas ??t?? cr???? en base"); 
            
            _usersCrudService.Remove(userInDb);
        }
        
        
        [TestMethod]
        public void SignUp_With_An_Already_Taken_Username_Does_Not_Create_Entry_In_DB()
        {
            var jsonInput= @"{
                        ""username"": ""UserTestSignIn"",
                        ""password"":""test""
                        }";
            
            JsonDocument doc = JsonDocument.Parse(jsonInput);
            JsonElement jsonUser = doc.RootElement;

            User user = _usersService.SignUp(jsonUser);
            Assert.IsNull(user, "L'username n'existe pas ");

            User userInDb = _usersCrudService.GetByUsername("newUsername");
            Assert.IsNull(userInDb, "L'User a pas ??t?? cr???? en base"); 
        }

        [TestMethod]
        public void Pray_With_Not_Enough_Disciples_Does_Not_Add_The_Card_To_Collection_Neither_Affect_Disciples()
        { 
            User backup = _usersCrudService.GetByUsername("UserTestCanAffordCardFalse");
            _usersService.Pray("UserTestCanAffordCardFalse", "NotLimited", null, null);
            User user = _usersCrudService.GetByUsername("UserTestCanAffordCardFalse"); 
            Assert.IsTrue(user.Disciples.Equals(0), "Le nombre de disciples a ??t?? modifi??");
            Assert.IsTrue(user.Collection.Count.Equals(0), "La collection a ??t?? modifi??e");
            _usersCrudService.Update(user.Id, backup);
        }
        
        [TestMethod]
        public void Pray_With_Enough_Disciples_Add_The_Card_To_Collection_And_Affect_Disciples()
        {
            User backup = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");
            _usersService.Pray("UserTestCanAffordCardTrue", "NotLimited", null, null);
            User user = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue"); 
            Assert.IsTrue(user.Disciples.Equals(90), "Le nombre de disciples n'est pas celui attendu");
            Assert.IsTrue(user.Collection.Count.Equals(1), "La collection n'a pas ??t?? modifi??e");
            _usersCrudService.Update(user.Id, backup);
        }

        [TestMethod]
        public void Modify_Team_Does_Affect_Database()
        {
            User backup = _usersCrudService.GetByUsername("UserTestGetOneUser");

            _usersService.ModifyTeam(null, null, "UserTestGetOneUser", "NameTeam", "NameTeamChanged",
                "CardTestUpdateModified,CardTestUpdateModified,CardTestNordiqueLegendaire");
            
            User user = _usersCrudService.GetByUsername("UserTestGetOneUser");

            Assert.IsNotNull(user.Teams.Find(team => team.Name.Equals("NameTeamChanged")), "Le nom de l'??quipe n'a pas ??t?? chang??");
            Assert.IsTrue(user.Teams.Find(team => team.Name.Equals("NameTeamChanged")).Compo.Contains(_cardsCrudService.GetCardByName("CardTestUpdateModified").Id), "Les cartes n'ont pas ??t?? modifi??es");
            Assert.IsFalse(user.Teams.Find(team => team.Name.Equals("NameTeamChanged")).Compo.Contains(_cardsCrudService.GetCardByName("CardTestGetOne").Id), "Les cartes n'ont pas ??t?? modifi??es");

            _usersCrudService.Update(user.Id, backup);
        }

        [TestMethod]
        public void SendFriendRequest_Not_Existing_Create_Entry_In_Database()
        {
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");
            _usersService.SendFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardTrue");
            FriendRequest request = _friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id); 
            Assert.IsNotNull(request, "Aucune demande n'a ??t?? cr????e en base");
            
            _friendRequestsCrudService.Remove(request);
            
        }

        [TestMethod]
        public void SendFriendRequest_Already_Existing_Does_Not_Create_Entry_In_Database()
        {
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardFalse");
            _usersService.SendFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardFalse");
            _usersService.SendFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardFalse");
            Assert.IsTrue(_friendRequestsCrudService.FindByReceiver(receiver.Id).Count == 1, "Il y a plus qu'une demande d'ami remont??e");
            
            _friendRequestsCrudService.Remove(_friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id));
        }
        
        [TestMethod]
        public void SendFriendRequest_To_A_Friend_Does_Not_Create_Entry_In_Database()
        {
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardFalse");
            _usersService.SendFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardFalse");
            _usersService.AcceptFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardFalse");
            _usersService.SendFriendRequest(null, null, "UserTestGetOneUser", "UserTestCanAffordCardFalse");
            Assert.IsNull(_friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id), "Une demande d'ami a ??t?? remont??e");

            _usersCrudService.Update(sender.Id, sender);
            _usersCrudService.Update(receiver.Id, receiver);
        }

        
        [TestMethod]
        public void AcceptFriendRequest_Modify_Entries_In_DataBase()
        {
            User backupSender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User backupReceiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");
            _usersService.SendFriendRequest(null, null, backupSender.Username, backupReceiver.Username);
            
            _usersService.AcceptFriendRequest(null, null, backupSender.Username, backupReceiver.Username);
            
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");        
            
            Assert.IsTrue(sender.Friends.Contains(receiver.Id), "La liste d'ami du sender ne contient pas le receiver");
            Assert.IsTrue(receiver.Friends.Contains(sender.Id), "La liste d'ami du receiver ne contient pas le sender");
            Assert.IsNull(_friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id), "La demande d'ami n'a pas ??t?? supprim??e");
            
            _usersCrudService.Update(sender.Id, backupSender);
            _usersCrudService.Update(receiver.Id, backupReceiver);

        }
        
        [TestMethod]
        public void RefuseFriendRequest_Modify_Entries_In_DataBase()
        {
            User backupSender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User backupReceiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");
            _usersService.SendFriendRequest(null, null, backupSender.Username, backupReceiver.Username);
            
            _usersService.RefuseFriendRequest(null, null, backupSender.Username, backupReceiver.Username);
            
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");        
            
            Assert.IsFalse(sender.Friends.Contains(receiver.Id), "La liste d'ami du sender contient le receiver");
            Assert.IsFalse(receiver.Friends.Contains(sender.Id), "La liste d'ami du receiver contient le sender");
            Assert.IsNull(_friendRequestsCrudService.FindBySenderAndReceiver(sender.Id, receiver.Id), "La demande d'ami n'a pas ??t?? supprim??e");
            
            _usersCrudService.Update(sender.Id, backupSender);
            _usersCrudService.Update(receiver.Id, backupReceiver);

        }

        [TestMethod]
        public void DeleteFriend_Modify_Entries_In_DataBase()
        {
            User sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            User receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");        
            _usersService.SendFriendRequest(null, null, sender.Username, receiver.Username);
            _usersService.AcceptFriendRequest(null, null, sender.Username, receiver.Username);
            
            _usersService.DeleteFriend(null, null, sender.Username, receiver.Username); 
            
            sender = _usersCrudService.GetByUsername("UserTestGetOneUser");
            receiver = _usersCrudService.GetByUsername("UserTestCanAffordCardTrue");
            
            Assert.IsFalse(sender.Friends.Contains(receiver.Id), "La liste d'ami du sender contient le receiver");
            Assert.IsFalse(receiver.Friends.Contains(sender.Id), "La liste d'ami du receiver contient le sender");
        }
    }
}
