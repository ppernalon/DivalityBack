namespace DivalityBack.Models.Gods
{
    public class Ra : GenericGod
    {
        public Ra()
        {
            Name = "Ra";
            Life = 99;
            MaxLife = 99;
            Armor = 24;
            Power = 28;
            Speed = 80;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}