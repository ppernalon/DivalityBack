namespace DivalityBack.Models.Gods
{
    public class Isis : GenericGod
    {
        public Isis()
        {
            Name = "Isis";
            Life = 154;
            MaxLife = 154;
            Armor = 38;
            Power = 21;
            Speed = 63;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}