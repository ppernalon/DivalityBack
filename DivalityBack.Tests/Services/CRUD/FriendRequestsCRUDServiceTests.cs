using System;
using System.Collections.Generic;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DivalityBack.Tests
{
    [TestClass]
    public class FriendRequestsCRUDServiceTests
    {
        private static FriendRequestsCRUDService _friendRequestsCrudService; 
        
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            _friendRequestsCrudService = new FriendRequestsCRUDService(SetupAssemblyInitializer._settings);
        }

        [TestMethod]
        public void Create_FriendRequest_Correct_Informations_In_Db()
        {
            FriendRequest request = new FriendRequest();

            request.Receiver = "619d14e4494e6d757649e48d";
            request.Sender = "619de7a37c00c8a09bed8bc3";

            FriendRequest requestDb = _friendRequestsCrudService.Create(request); 
            
            //On vérifie que l'user créé est bien trouvable en base
            Assert.IsNotNull(_friendRequestsCrudService.Get(requestDb.Id), "La demande d'ami n'est pas trouvable en base de données");
            
            //On vérifie ensuite que ses données sont les mêmes que celles entrées
            Assert.IsTrue(request.Equals(requestDb), "L'User en base et celui entré sont différents");
            
            //On supprime le User à la fin du test pour ne pas polluer la base
            _friendRequestsCrudService.Remove(requestDb);
        }

        [TestMethod]
        public void Get_One_FriendRequest_Returns_Correct_Informations()
        {
            FriendRequest request = _friendRequestsCrudService.Get("61e05e7e5525855aa9d08ef5"); 
            
            Assert.IsNotNull(request, "La demande d'ami est introuvable en base de données");
            
            //On  vérifie que les informations remontées sont correctes
            Assert.IsTrue(request.Receiver.Equals("619d14e4494e6d757649e48d"), "Le receiver remonté n'est celui attendu ");
            Assert.IsTrue(request.Sender.Equals("619de7a37c00c8a09bed8bc3"), "Le sender remonté n'est celui attendu ");
        }

        [TestMethod]
        public void Get_All_FriendRequests_Returns_Correct_Informations()
        {
            List<FriendRequest> listRequests = _friendRequestsCrudService.Get();             

            Assert.IsNotNull(listRequests, "Aucune demande d'ami n'a pu être récupérée en base de données");
            
            //On vérifie que les données récupérées sont bien des demandes d'ami
            Assert.IsInstanceOfType(listRequests[0], typeof(FriendRequest), "Les objets récupérés en base de données ne sont pas du type FriendRequest");
            
        }

        [TestMethod]
        public void Update_One_FriendRequest_With_Correct_Informations()
        {
            FriendRequest newRequest = new FriendRequest();
            newRequest.Sender = "619de7a37c00c8a09bed8bc3";
            newRequest.Receiver = "619d14e4494e6d757649e48d";
            
            _friendRequestsCrudService.Create(newRequest);

            newRequest.Sender = "619de8f95e072a55ed22a7a0";
            
            _friendRequestsCrudService.Update(newRequest.Id, newRequest);

            FriendRequest requestInDataBase= _friendRequestsCrudService.Get(newRequest.Id); 
            
            Assert.IsTrue(requestInDataBase.Id.Equals(newRequest.Id), "La demande d'ami trouvée en base et celle qu'on a modifiée sont différents");
            Assert.IsFalse(requestInDataBase.Sender.Equals("619de7a37c00c8a09bed8bc3"), "Le Sender n'a pas été modifié en base de données");
            Assert.IsTrue(requestInDataBase.Receiver.Equals(newRequest.Receiver), "Le Receiver a été modifié pendant la modification du Sender");
            //On supprime l'User de la base de données
            _friendRequestsCrudService.Remove(requestInDataBase);
        }

        [TestMethod]
        public void Remove_One_User_From_Database_By_Passing_Object()
        {
            FriendRequest newRequest = new FriendRequest();
            newRequest.Sender = "619de7a37c00c8a09bed8bc3";
            newRequest.Receiver = "619d14e4494e6d757649e48d";
            
            FriendRequest requestInDb = _friendRequestsCrudService.Create(newRequest);
            
            Assert.IsNotNull(requestInDb, "La demande d'ami n'a pas été créée en base de données");
            
            _friendRequestsCrudService.Remove(requestInDb);
            Assert.IsNull(_friendRequestsCrudService.Get(requestInDb.Id), "La demande d'ami n'a pas été supprimée de la base de données");
        }
        
        [TestMethod]
        public void Remove_One_User_From_Database_By_Passing_Id()
        {
            FriendRequest newRequest = new FriendRequest();
            newRequest.Sender = "619de7a37c00c8a09bed8bc3";
            newRequest.Receiver = "619d14e4494e6d757649e48d";
            
            FriendRequest requestInDb = _friendRequestsCrudService.Create(newRequest);
            
            Assert.IsNotNull(requestInDb, "La demande d'ami n'a pas été créée en base de données");
            
            _friendRequestsCrudService.Remove(requestInDb.Id);
            Assert.IsNull(_friendRequestsCrudService.Get(requestInDb.Id), "La demande d'ami n'a pas été supprimée de la base de données");
        }

        [TestMethod]
        public void FindByReceiver_Returns_Correct_Informations()
        {
            List<FriendRequest> listRequests = _friendRequestsCrudService.FindByReceiver("619d14e4494e6d757649e48d");
            Assert.IsTrue(listRequests.Count > 1, "La taille de la liste retournée n'est pas celle attendue");
            
            foreach (FriendRequest request in listRequests)
            {
                Assert.IsTrue(request.Receiver.Equals("619d14e4494e6d757649e48d"), "La demande d'ami d'Id " + request.Id + " a été remontée alors que le receiver n'est pas celui attendu");
            }
        }

        [TestMethod]
        public void FindBySenderAndReceiver_Returns_Correct_Informations()
        {
            FriendRequest request =
                _friendRequestsCrudService.FindBySenderAndReceiver("619de7a37c00c8a09bed8bc3",
                    "619d14e4494e6d757649e48d");
            Assert.IsNotNull(request, "Aucune demande d'ami n'a été trouvée en base avec ces informations");

            FriendRequest requestNull =
                _friendRequestsCrudService.FindBySenderAndReceiver("619de7a37c00c8a09bed8bc3",
                    "619d14e4494e6d757649e49f");
            Assert.IsNull(requestNull, "Une requête a été trouvée avec des informations erronées");
        }
    }
}