namespace DivalityBack.Models.Gods
{
    public class Hades : GenericGod
    {
        public Hades()
        {
            Name = "Hades";
            Life = 200;
            Armor = 40;
            Power = 10;
            Speed = 10;
            GlobalAllyEffect = new EffectOnGod(10, 5, 0, 5);
        }
    }
}