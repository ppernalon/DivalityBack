namespace DivalityBack.Models.Gods
{
    public class Anubis : GenericGod
    {
        public Anubis()
        {
            Life = 120;
            Armor = 12;
            Power = 40;
            Speed = 80;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}