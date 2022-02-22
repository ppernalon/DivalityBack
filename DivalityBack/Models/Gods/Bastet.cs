namespace DivalityBack.Models.Gods
{
    public class Bastet : GenericGod
    {
        public Bastet()
        {
            Name = "Bastet";
            Life = 120;
            MaxLife = 120;
            Armor = 7;
            Power = 50;
            Speed = 100;
            GlobalAllyEffect = new EffectOnGod(0, 0, 10, 0);
        }
        
    }
}