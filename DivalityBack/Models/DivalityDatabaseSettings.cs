namespace DivalityBack.Models
{
    public class DivalityDatabaseSettings: IDivalityDatabaseSettings
    {
        public string UsersCollectionName { get; set; }
        public string TeamsCollectionName { get; set; }
        public string CardsCollectionName { get; set; }
        public string AuctionHouseCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDivalityDatabaseSettings
    {
        string UsersCollectionName { get; set; }
        string TeamsCollectionName { get; set; }
        string CardsCollectionName { get; set; }
        string AuctionHouseCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}

