namespace DivalityBack.Models.Gods
{
    public class Osiris : GenericGod
    {
        public Osiris()
        {
            Name = "Osiris";
            Life = 80;
            MaxLife = 80;
            Armor = 10;
            Power = 50;
            Speed = 70;
            GlobalAllyEffect = new EffectOnGod(10, 0, 0, 10);
        }
        
    }
}