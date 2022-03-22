namespace DivalityBack.Models.Gods
{
    public class Loki : GenericGod
    {
        public Loki()
        {
            Name = "Loki";
            Life = 197;
            MaxLife = 197;
            Armor = 46;
            Power = 35;
            Speed = 7;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}