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
            ToLeftRate = 0.5;
            ToRightRate = 1 - ToLeftRate;
            GlobalAllyEffect = new EffectOnGod();
            GlobalEnnemyEffect = new EffectOnGod();
        }
        
        public EffectOnGod GlobalAllyEffect { get; set; }
        public EffectOnGod GlobalEnnemyEffect { get; set; }
        public double ToLeftRate { get; set; }
        public double ToRightRate { get; set; }

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

        public int getRebound()
        {
            // return -1 if left, 1 if right

            int leftOrRight;
            Random random = new Random();
            double randomValue = random.NextDouble();
            if (randomValue <= ToLeftRate)
            {
                leftOrRight = -1;
            }
            else
            {
                leftOrRight = 1;
            }

            return leftOrRight;
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

            int[] firstAttackIndex = whoToAttackFirst(opponentGodTeam);
            attackedPositions.Add(firstAttackIndex);

            // TopGod is not attacked first so attack can rebound
            // if TopGod is attacked the pattern ends
            if (firstAttackIndex[0] != 0)
            {
                GenericGod firstAttackedGod = opponentGodTeam.getGod(firstAttackIndex);
                int firstLeftOrRight = firstAttackedGod.getRebound();
                
                if (firstAttackIndex[0] == 1) // MiddleGods
                {
                    middleGodsAttacked(
                        firstAttackIndex[1], 
                        firstLeftOrRight, 
                        attackedPositions, 
                        opponentGodTeam.TopGod
                    );
                }
                else if (firstAttackIndex[0] == 2) // BaseGods
                {
                    baseGodsAttacked(
                        firstAttackIndex,
                        firstLeftOrRight,
                        attackedPositions,
                        opponentGodTeam
                    );
                }
            }

            return attackedPositions.ToArray();
        }

        private void middleGodsAttacked(int attackedPosition, int leftOrRight, List<int[]> attackedPositions, GenericGod topGod)
        {
            if (attackedPosition == 0) // MiddleGods on Left
            {
                if (leftOrRight == -1) // Left
                {
                    attackedPositions.Add(new [] {-1, -1}); // rebound ext
                }

                if (leftOrRight == 1) // right
                {
                    if (topGod.isAlive())
                    {
                        attackedPositions.Add(new [] {0, 0});
                    }  
                }
            }
            else if (attackedPosition == 1) // MiddleGods on Right
            {
                if (leftOrRight == 1) // Right
                {
                    attackedPositions.Add(new [] {-1, -1}); // rebound ext
                }

                if (leftOrRight == -1) // Left
                {
                    if (topGod.isAlive())
                    {
                        attackedPositions.Add(new [] {0, 0});
                    }  
                }
            }
        }

        private void baseGodsAttacked(int[] attackIndex, int leftOrRight, List<int[]> attackedPositions, GodTeam opponentGodTeam)
        {
            int[] secondAttackIndex;
            // rebound ext after first attack
            // left rebound left or right rebound right
            if (
                ( attackIndex[1] == 0 && leftOrRight == -1 )
                || ( attackIndex[1] == 2 && leftOrRight == 1 )
            )
            {
                return;
            }
            // rebound on left of MiddleGods
            // left rebound right or middle rebound left
            else if ( 
                ( attackIndex[0] == 0 && leftOrRight == 1)
                || ( attackIndex[0] == 1 && leftOrRight == -1)
            ) {
                secondAttackIndex = new[] {1, 0};
                GenericGod secondAttackedGod = opponentGodTeam.getGod(secondAttackIndex);
                int secondLeftOrRight = secondAttackedGod.getRebound();
                middleGodsAttacked(
                    secondAttackIndex[1], 
                    secondLeftOrRight, 
                    attackedPositions, 
                    opponentGodTeam.TopGod
                );
            }
            // rebound on right of MiddleGods
            // right rebound left or middle rebound right
            else if ( 
                ( attackIndex[0] == 2 && leftOrRight == -1) 
                || ( attackIndex[0] == 1 && leftOrRight == 1) 
            ) {
                secondAttackIndex = new[] {1, 1};
                GenericGod secondAttackedGod = opponentGodTeam.getGod(secondAttackIndex);
                int secondLeftOrRight = secondAttackedGod.getRebound();
                middleGodsAttacked(
                    secondAttackIndex[1], 
                    secondLeftOrRight, 
                    attackedPositions, 
                    opponentGodTeam.TopGod
                );
            }
        }
        
        private int[] whoToAttackFirst(GodTeam opponentGodTeam)
        {
            // the godTeam has to be alive
            // return { a, b } where a is the row attacked (0 : top, 2: base) and b the position (0 : left)
            if (GodTeam.rowIsAlive(opponentGodTeam.BaseGods))
            {
                return new [] {2, randomPick(opponentGodTeam.BaseGods)};
            }
            else if (GodTeam.rowIsAlive(opponentGodTeam.MiddleGods))
            {
                return new[] {1, randomPick(opponentGodTeam.BaseGods)};
            }
            else
            {
                return new[] {0, 0};
            }
        }

        private int randomPick(GenericGod[] rowGods)
        {
            List<int> canBeAttackedGods = new List<int>();

            for (int godIndex = 0; godIndex < 3; godIndex++)
            {
                if (rowGods[godIndex].isAlive()) canBeAttackedGods.Add(godIndex);
            }
            
            int indexPicked;

            // random pick between last possibilities
            int numberOfPossibilities = canBeAttackedGods.Count;
            Random aleatoire = new Random();
            indexPicked = canBeAttackedGods[aleatoire.Next(0, numberOfPossibilities)];

            return indexPicked;
        }
    }
}