namespace DivalityBack.Models.Gods
{
    public class Freyr : GenericGod
    {
        public Freyr()
        {
            Name = "Freyr";
            Life = 135;
            MaxLife = 135;
            Armor = 10;
            Power = 32;
            Speed = 14;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}