namespace DivalityBack.Models.Gods
{
    public class Hera : GenericGod
    {
        public Hera()
        {
            Name = "Hera";
            Life = 179;
            MaxLife = 179;
            Armor = 50;
            Power = 24;
            Speed = 66;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}