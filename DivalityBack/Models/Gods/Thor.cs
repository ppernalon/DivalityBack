namespace DivalityBack.Models.Gods
{
    public class Thor : GenericGod
    {
        public Thor()
        {
            Name = "Thor";
            Life = 180;
            Armor = 20;
            Power = 35;
            Speed = 65;
            ToRightRate = 0.7;
            ToLeftRate = 0.3;
        }
    }
}