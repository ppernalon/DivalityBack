namespace DivalityBack.Models.Gods
{
    public class Hator : GenericGod
    {
        public Hator()
        {
            Name = "Hator";
            Life = 140;
            MaxLife = 140;
            Armor = 46;
            Power = 27;
            Speed = 63;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}