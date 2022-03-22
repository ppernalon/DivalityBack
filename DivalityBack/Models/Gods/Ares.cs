namespace DivalityBack.Models.Gods
{
    public class Ares : GenericGod
    {
        public Ares()
        {
            Name = "Ares";
            Life = 173;
            MaxLife = 173;
            Armor = 44;
            Power = 34;
            Speed = 35;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}