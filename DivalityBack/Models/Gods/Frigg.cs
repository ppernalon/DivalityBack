namespace DivalityBack.Models.Gods
{
    public class Frigg : GenericGod
    {
        public Frigg()
        {
            Name = "Frigg";
            Life = 97;
            MaxLife = 97;
            Armor = 35;
            Power = 41;
            Speed = 10;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}