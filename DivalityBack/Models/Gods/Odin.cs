namespace DivalityBack.Models.Gods
{
    public class Odin : GenericGod
    {
        public Odin()
        {
            Name = "Odin";
            Life = 130;
            MaxLife = 130;
            Armor = 15;
            Power = 30;
            Speed = 50;
            GlobalAllyEffect = new EffectOnGod(5, 5, 5, 10);
        }
    }
}