using System;
using System.Collections.Generic;

namespace DivalityBack.Models.Gods
{
    public class GenericGod : Card
    {
        GenericGod(string name, int life, int armor, int speed, int power)
        {
            Name = name; // identify the god
            Life = life; // current life of the god
            Armor = armor; // percentage of damage reduction
            Speed = speed; // define de the order of attack
            Power = power; // define the damage of ability
            GlobalAllyEffect = new EffectOnGod();
            GlobalEnnemyEffect = new EffectOnGod();
        }
        
        public EffectOnGod GlobalAllyEffect { get; set; }
        public EffectOnGod GlobalEnnemyEffect { get; set; }

        public Boolean isAlive()
        {
            return Life > 0;
        }

        public void addPositiveEffect(EffectOnGod effect)
        {
            Life += effect.Life;
            Speed += effect.Speed;
            Power += effect.Power;
            
            const int ARMOR_CAP = 75;
            if (Armor + effect.Armor < ARMOR_CAP)
            {
                Armor += effect.Armor;
            }
            else
            {
                Armor = ARMOR_CAP;
            }
        }

        public void addNegativeEffect(EffectOnGod effect)
        {
            if (Life - effect.Life >= 1)
            {
                Life -= effect.Life;
            }
            else
            {
                Life = 1;
            }
            
            if (Power - effect.Power >= 0)
            {
                Power -= effect.Power;
            }
            else
            {
                Power = 0;
            }
            
            if (Armor - effect.Armor >= 0)
            {
                Armor -= effect.Armor;
            }
            else
            {
                Armor = 0;
            }
            
            Speed -= effect.Speed;
        }

        public int getStriked(int amountOfDamage)
        {
            int reducedDamge = amountOfDamage * Armor / 100;
            Life -= reducedDamge;
            return reducedDamge;
        }

        public static int compareSpeed(GenericGod g1, GenericGod g2)
        {
            if (g1.Speed > g2.Speed) return -1;
            else if (g1.Speed < g2.Speed) return 1;
            else return 0;
        }

        public int[][] getAttackPattern(GodTeam opponentGodTeam)
        {
            List<int[]> attackedPositions = new List<int[]>();

            int baseAttackIndex = whoToAttackBase(opponentGodTeam.BaseGods);
            if (baseAttackIndex != -2) attackedPositions.Add(new int[]{2, baseAttackIndex});
            
            int middleAttackIndex = whoToAttackMiddle(opponentGodTeam.MiddleGods, baseAttackIndex);
            if (middleAttackIndex != -2 && middleAttackIndex != -1) attackedPositions.Add(new int[]{2, middleAttackIndex});
            
            if (middleAttackIndex == -1) // player is attacked, no more bounds
            {
                attackedPositions.Add(new int[]{-1, middleAttackIndex});
            }
            else
            {
                int topAttackIndex = whoToAttackTop(opponentGodTeam.TopGod, middleAttackIndex);
                if (topAttackIndex == -1) // player is attacked
                {
                    attackedPositions.Add(new int[]{-1, middleAttackIndex});
                }
                else
                {
                    attackedPositions.Add(new int[]{0, topAttackIndex});
                    attackedPositions.Add(new int[]{-1, middleAttackIndex}); // player is attacked if TopGod is attacked
                }
            }

            return attackedPositions.ToArray();
        }

        private int whoToAttackBase(GenericGod[] opponentBaseGods)
        {
            List<int> canBeAttackedGods = new List<int>();

            for (int godIndex = 0; godIndex < 3; godIndex++)
            {
                if (opponentBaseGods[godIndex].isAlive()) canBeAttackedGods.Add(godIndex);
            }
            
            if (canBeAttackedGods.Count == 0) // all BaseGods are dead
            {
                return -2;
            }

            else
            {
                int indexPicked;

                // random pick between last possibilities
                int numberOfPossibilities = canBeAttackedGods.Count;
                Random aleatoire = new Random();
                indexPicked = canBeAttackedGods[aleatoire.Next(0, numberOfPossibilities)];

                return indexPicked;
            }
        }

        private int whoToAttackMiddle(GenericGod[] opponentMiddleGos, int baseGodAttacked)
        {
            List<int> canBeAttackedGods = new List<int>();

            for (int godIndex = 0; godIndex < 3; godIndex++)
            {
                if (opponentMiddleGos[godIndex].isAlive()) canBeAttackedGods.Add(godIndex);
            }
            
            if (canBeAttackedGods.Count == 0) // all MiddleGods are dead
            {
                return -2;
            }
            else
            {
                // if baseGodAttacked == -2 all gods can be attacked but no player
                if (baseGodAttacked == 0) canBeAttackedGods.Remove(1);
                if (baseGodAttacked == 2) canBeAttackedGods.Remove(0);
                if (baseGodAttacked != 1 && baseGodAttacked != -2) canBeAttackedGods.Add(-1); // player can be attacked
                
                int indexPicked;

                // random pick between last possibilities
                int numberOfPossibilities = canBeAttackedGods.Count;
                Random aleatoire = new Random();
                indexPicked = canBeAttackedGods[aleatoire.Next(0, numberOfPossibilities)];

                return indexPicked;
            }
        }

        private int whoToAttackTop(GenericGod topGod, int middleGodAttacked)
        {
            if (topGod.isAlive())
            {
                List<int> canBeAttackedGods = new List<int>();
                canBeAttackedGods.Add(0); // topGod is alive so he can be attacked
                // if middleGodAttacked =-2 the topGod is attacked
                if (middleGodAttacked != -2) canBeAttackedGods.Add(-1); // player can be attack from the left or the right
                
                int indexPicked;

                // random pick between last possibilities
                int numberOfPossibilities = canBeAttackedGods.Count;
                Random aleatoire = new Random();
                indexPicked = canBeAttackedGods[aleatoire.Next(0, numberOfPossibilities)];

                return indexPicked;
            }
            else
            {
                return -1; // the player is attacked if topGod is dead
            }
        }
    }
}