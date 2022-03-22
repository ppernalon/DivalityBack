namespace DivalityBack.Models.Gods
{
    public class Njor : GenericGod
    {
        public Njor()
        {
            Name = "Njor";
            Life = 151;
            MaxLife = 151;
            Armor = 39;
            Power = 30;
            Speed = 66;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}