namespace DivalityBack.Models.Gods
{
    public class Demeter : GenericGod
    {
        public Demeter()
        {
            Name = "Demeter";
            Life = 94;
            MaxLife = 94;
            Armor = 5;
            Power = 50;
            Speed = 24;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}