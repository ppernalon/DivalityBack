using System;
using System.Collections.Generic;
using DivalityBack.Services.CRUD;

namespace DivalityBack.Models.Gods
{
    public class GenericGod : Card
    {
        public GenericGod(string name, int life, int armor, int speed, int power)
        {
            Name = name; // identify the god
            Life = life; // current life of the god
            MaxLife = life;
            Armor = armor; // percentage of damage reduction
            Speed = speed; // define de the order of attack
            Power = power; // define the damage of ability
            ToLeftRate = 0.5;
            ToRightRate = 1 - ToLeftRate;
            GlobalAllyEffect = new EffectOnGod();
            GlobalEnnemyEffect = new EffectOnGod();
        }

        public GenericGod()
        {
            Name = ""; // identify the god
            Life = -1; // current life of the god
            MaxLife = -1;
            Armor = -1; // percentage of damage reduction
            Speed = -1; // define de the order of attack
            Power = -1; // define the damage of ability
            ToLeftRate = -1;
            ToRightRate = -1;
            GlobalAllyEffect = new EffectOnGod();
            GlobalEnnemyEffect = new EffectOnGod();
        }

        public int MaxLife { get; set; }
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
            MaxLife += effect.MaxLife;
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

            if (MaxLife - effect.MaxLife >= 1)
            {
                MaxLife -= effect.MaxLife;
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
            int reducedDamage = amountOfDamage * ( 100 - Armor ) / 100;
            Life -= reducedDamage;
            if (Life < 0) Life = 0;
            return reducedDamage;
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
                ( attackIndex[1] == 0 && leftOrRight == 1)
                || ( attackIndex[1] == 1 && leftOrRight == -1)
            ) {
                secondAttackIndex = new[] {1, 0};
                GenericGod secondAttackedGod = opponentGodTeam.getGod(secondAttackIndex);
                if (secondAttackedGod.isAlive())
                {
                    attackedPositions.Add(secondAttackIndex);
                    int secondLeftOrRight = secondAttackedGod.getRebound();
                    middleGodsAttacked(
                        secondAttackIndex[1], 
                        secondLeftOrRight, 
                        attackedPositions, 
                        opponentGodTeam.TopGod
                    );
                }
                else
                {
                    // the attack follows his path to TopGod
                    if ( attackIndex[1] == 0 && leftOrRight == 1 && opponentGodTeam.TopGod.isAlive())
                    {
                        attackedPositions.Add(new[] {0, 0});
                    }
                }
            }
            // rebound on right of MiddleGods
            // right rebound left or middle rebound right
            else if ( 
                ( attackIndex[1] == 2 && leftOrRight == -1) 
                || ( attackIndex[1] == 1 && leftOrRight == 1) 
            ) {
                secondAttackIndex = new[] {1, 1};
                GenericGod secondAttackedGod = opponentGodTeam.getGod(secondAttackIndex);
                if (secondAttackedGod.isAlive())
                {
                    attackedPositions.Add(secondAttackIndex);
                    int secondLeftOrRight = secondAttackedGod.getRebound();
                    middleGodsAttacked(
                        secondAttackIndex[1], 
                        secondLeftOrRight, 
                        attackedPositions, 
                        opponentGodTeam.TopGod
                    );
                }
                else
                {
                    // the attack follows his path to TopGod
                    if (attackIndex[1] == 2 && leftOrRight == -1 && opponentGodTeam.TopGod.isAlive())
                    {
                        attackedPositions.Add(new [] {0, 0});
                    }
                }
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
                return new[] {1, randomPick(opponentGodTeam.MiddleGods)};
            }
            else
            {
                return new[] {0, 0};
            }
        }

        private int randomPick(GenericGod[] rowGods)
        {
            List<int> canBeAttackedGods = new List<int>();

            for (int godIndex = 0; godIndex < rowGods.Length; godIndex++)
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

        public static GenericGod getGodByName(string name)
        {
            switch (name.ToLower())
            {
                // egyptian
                case "amon":
                    return new Amon();
                case "anubis":
                    return new Anubis();
                case "bastet":
                    return new Bastet();
                case "hathor":
                case "hator":
                    return new Hator();
                case "horus":
                    return new Horus();
                case "isis":
                    return new Isis();
                case "osiris":
                    return new Osiris();
                case "ptah":
                    return new Ptah();
                case "ra":
                    return new Ra();
                
                
                //greek
                case "ares":
                    return new Ares();
                case "artemis":
                    return new Artemis();
                case "athena":
                    return new Athena();
                case "demeter":
                    return new Demeter();
                case "hades":
                    return new Hades();
                case "hephaitos":
                case "hephaistos":
                    return new Hephaistos();
                case "hera":
                    return new Hera();
                case "hestia":
                    return new Hestia();
                case "poseidon":
                    return new Poseidon();
                case "zeus":
                    return new Zeus();
                
                // nordic
                case "baldr":
                    return new Baldr();
                case "freyr":
                    return new Freyr();
                case "frigg":
                    return new Frigg();
                case "loki":
                    return new Loki();
                case "njor":
                    return new Njor();
                case "skadi":
                    return new Skadi();
                case "odin":
                    return new Odin();
                case "hel":
                    return new Hel();
                case "thor":
                    return new Thor();
                case "ymir":
                    return new Ymir();
            }

            return new GenericGod();
        }
    }
}