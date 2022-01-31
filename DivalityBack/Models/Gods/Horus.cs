namespace DivalityBack.Models.Gods
{
    public class Horus : GenericGod
    {
        public Horus()
        {
            Life = 200;
            Armor = 20;
            Power = 15;
            Speed = 30;
            GlobalAllyEffect = new EffectOnGod(0, 10, 10, 0); // tous les dieux ont + 10 d'armure
        }
    }
}