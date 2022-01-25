using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DivalityBack.Models.Gods
{
    public class GenericGod
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
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Name { get; set; }
        public int Life { get; set; }
        public int Armor { get; set; }
        public int Speed { get; set; }
        public int Power { get; set; }
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

        public void strike(List<List<GenericGod>> opponentGods)
        {
            /*
                opponentGods is a matrix from which we get the god placements
                [
                    [ X G4 G5 G6 ]
                    [ P G2 G3 P ]
                    [ P G1 P X ]                   
                ]
                G stands for gods, 
                P for player,
                X for nothing
            */

            List<int[]> strikeIndexes = new List<int[]>();

            List<GenericGod> firstRow = opponentGods[0];
            int firstStrikedCol = strikeOneRow(firstRow, -1);
            if (firstStrikedCol != -1)
            {
                strikeIndexes.Add(new int[] {0, firstStrikedCol});
            }

            List<GenericGod> secondRow = opponentGods[1];
            int secondStrikedCol = strikeOneRow(secondRow, firstStrikedCol);
            if (secondStrikedCol != -1)
            {
                strikeIndexes.Add(new int[] {1, secondStrikedCol});
            }

            if (secondStrikedCol != 0 && secondStrikedCol != 3) // 0 and 3 are the player
            {
                List<GenericGod> thirdRow = opponentGods[2];
                int thirdStrikedCol = strikeOneRow(thirdRow, secondStrikedCol);
                if (thirdStrikedCol != -1)
                {
                    strikeIndexes.Add(new int[] {2, thirdStrikedCol});
                }
            }

            foreach (int[] strikeIndex in strikeIndexes)
            {
                int line = strikeIndex[0];
                int col = strikeIndex[1];
                opponentGods[line][col].onEnnemyStrike(Power);
            }
        }

        private int strikeOneRow(List<GenericGod> rowOfGods, int strikeComesFrom)
        {
            List<int> canBeStrikedGods = new List<int>();
            
            // only alive gods can be attacked
            for (int index = 0; index < rowOfGods.Count; index++)
            {
                GenericGod god = rowOfGods[index];
                if (god.isAlive())
                {
                    canBeStrikedGods.Add(index);
                }
            }

            if (canBeStrikedGods.Count == 0) // all dead
            {
                return -1;
            }

            if (strikeComesFrom != -1) // -1 stands for first row to be attacked or dead row just before 
            {
                foreach (int godIndex in canBeStrikedGods)
                {
                    if (Math.Abs(godIndex - strikeComesFrom) <= 1) // no rebound possible
                    {
                        canBeStrikedGods.RemoveAt(godIndex);
                    }
                }

            }
            
            int indexPicked;

            // random pick between last possibilities
            int numberOfPossibilities = canBeStrikedGods.Count;
            Random aleatoire = new Random();
            indexPicked = canBeStrikedGods[aleatoire.Next(0, numberOfPossibilities)];

            return indexPicked;
        }

        public void onEnnemyStrike(int amountOfDamage)
        {
            Life -= amountOfDamage * Armor / 100;
        }
    }
}