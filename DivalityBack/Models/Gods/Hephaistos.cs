namespace DivalityBack.Models.Gods
{
    public class Hephaistos : GenericGod
    {
        public Hephaistos()
        {
            Name = "Hephaistos";
            Life = 168;
            MaxLife = 168;
            Armor = 24;
            Power = 25;
            Speed = 41;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}