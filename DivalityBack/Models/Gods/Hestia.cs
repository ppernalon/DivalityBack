namespace DivalityBack.Models.Gods
{
    public class Hestia : GenericGod
    {
        public Hestia()
        {
            Name = "Hestia";
            Life = 103;
            MaxLife = 103;
            Armor = 48;
            Power = 49;
            Speed = 15;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}