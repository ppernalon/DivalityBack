using MongoDB.Driver;
using System.Collections.Generic;
using Divality.Models;

namespace Divality.Services.CRUD
{
    public class TeamsCRUDService
    {
        private readonly IMongoCollection<Team> _teams;

        public TeamsCRUDService(IDivalityDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _teams = database.GetCollection<Team>(settings.TeamsCollectionName);
        }

        public List<Team> Get() =>
            _teams.Find(team => true).ToList();

        public Team Get(string id) =>
            _teams.Find<Team>(_teams => _teams.Id == id).FirstOrDefault();

        public Team Create(Team team)
        {
            _teams.InsertOne(team);
            return team;
        }

        public void Update(string id, Team teamIn) =>
            _teams.ReplaceOne(team => team.Id == id, teamIn);

        public void Remove(Team teamIn) =>
            _teams.DeleteOne(team => team.Id == teamIn.Id);

        public void Remove(string id) =>
            _teams.DeleteOne(team => team.Id == id);
    }
}