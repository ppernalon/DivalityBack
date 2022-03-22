namespace DivalityBack.Models.Gods
{
    public class Skadi : GenericGod
    {
        public Skadi()
        {
            Name = "Skadi";
            Life = 101;
            MaxLife = 101;
            Armor = 38;
            Power = 22;
            Speed = 61;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}