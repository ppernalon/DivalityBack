namespace DivalityBack.Models.Gods
{
    public class Zeus : GenericGod
    {
        public Zeus()
        {
            Name = "Zeus";
            Life = 100;
            MaxLife = 100;
            Armor = 0;
            Power = 50;
            Speed = 80;
            GlobalAllyEffect = new EffectOnGod(5, 5, 5, 5);
        }
    }
}