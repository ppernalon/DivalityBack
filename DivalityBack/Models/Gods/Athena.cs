﻿namespace DivalityBack.Models.Gods
{
    public class Athena : GenericGod
    {
        public Athena()
        {
            Life = 150;
            Armor = 30;
            Power = 50;
            Speed = 80;
            GlobalAllyEffect = new EffectOnGod(0, 0, 0, 0);
        }
    }
}