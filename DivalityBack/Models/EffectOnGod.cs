namespace DivalityBack.Models.Gods
{
    public class EffectOnGod
    {
        public EffectOnGod(int life, int armor, int speed, int power)
        {
            Life = life; // added life
            MaxLife = life; // added max life
            Armor = armor; // added percentage of damage reduction
            Speed = speed; // added speed
            Power = power; // added power
        }

        public EffectOnGod()
        {
            Life = 0;
            MaxLife = 0;
            Armor = 0;
            Speed = 0;
            Power = 0;
        }
        
        public int MaxLife { get; set; }
        public int Life { get; set; }
        public int Armor { get; set; }
        public int Speed { get; set; }
        public int Power { get; set; }
    }
}