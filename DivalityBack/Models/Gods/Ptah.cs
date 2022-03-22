namespace DivalityBack.Models.Gods
{
    public class Ptah : GenericGod
    {
        public Ptah()
        {
            Name = "Ptah";
            Life = 181;
            MaxLife = 181;
            Armor = 43;
            Power = 22;
            Speed = 15;
            GlobalAllyEffect = new EffectOnGod(15, 0, 0, 0);
        }
    }
}