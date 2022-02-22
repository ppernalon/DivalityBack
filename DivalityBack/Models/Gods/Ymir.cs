namespace DivalityBack.Models.Gods
{
    public class Ymir : GenericGod
    {
        public Ymir()
        {
            Name = "Ymir";
            Life = 200;
            MaxLife = 200;
            Armor = 50;
            Power = 20;
            Speed = 5;
            GlobalAllyEffect = new EffectOnGod(0, 10, 0, 0);
        }
    }
}