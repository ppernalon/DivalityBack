namespace DivalityBack.Models.Gods
{
    public class Artemis : GenericGod
    {
        public Artemis()
        {
            Name = "Artemis";
            Life = 153;
            MaxLife = 153;
            Armor = 43;
            Power = 28;
            Speed = 2;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}