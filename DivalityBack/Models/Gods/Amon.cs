namespace DivalityBack.Models.Gods
{
    public class Amon : GenericGod
    {
        public Amon()
        {
            Name = "Amon";
            Life = 87;
            MaxLife = 87;
            Armor = 30;
            Power = 42;
            Speed = 94;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}