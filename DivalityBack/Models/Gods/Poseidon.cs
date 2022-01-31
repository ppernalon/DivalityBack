namespace DivalityBack.Models.Gods
{
    public class Poseidon : GenericGod
    {
        public Poseidon()
        {
            Life = 140;
            Armor = 0;
            Power = 20;
            Speed = 55;
            GlobalAllyEffect = new EffectOnGod(0, 0, 0, 15);
        }
    }
}