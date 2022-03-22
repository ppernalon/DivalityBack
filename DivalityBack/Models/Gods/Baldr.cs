namespace DivalityBack.Models.Gods
{
    public class Baldr : GenericGod
    {
        public Baldr()
        {
            Name = "Baldr";
            Life = 182;
            MaxLife = 182;
            Armor = 22;
            Power = 43;
            Speed = 92;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}