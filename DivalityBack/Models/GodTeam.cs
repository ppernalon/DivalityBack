using System;
using System.Collections.Generic;
using DivalityBack.Models.Gods;

namespace DivalityBack.Models
{
    public class GodTeam
    {
        public GodTeam(GenericGod[] gods)
        {
            AllGods = gods;
            AliveGods = new List<GenericGod>(gods);
            TopGod = gods[0]; // index 0 of god rows
            MiddleGods = gods[1..3]; // index 1 of god rows
            BaseGods = gods[3..]; // index 2 of god rows
        }
        
        public GenericGod TopGod { set; get; }
        public GenericGod[] MiddleGods { set; get; }
        public GenericGod[] BaseGods { set; get; }
        public GenericGod[] AllGods { set; get; }
        public List<GenericGod> AliveGods { set; get; }

        public void  getStriked(int initialAmountOfDamage, int[][] positions)
        {
            // position : { row, col } which means { 2, 1 } is the second god on the row of 3 gods
            // if position = {-1, -1} it means that the player is attack
            int amountOfDamage = initialAmountOfDamage;
            
            foreach (var position in positions)
            {
                GenericGod attackedGod;
                if (position[0] == 2) // BaseGods
                {
                    attackedGod = BaseGods[position[1]];
                    amountOfDamage = attackedGod.getStriked(amountOfDamage);
                }
                else if (position[0] == 1)
                {
                    attackedGod = MiddleGods[position[1]];
                    amountOfDamage = MiddleGods[position[1]].getStriked(amountOfDamage);
                }
                else
                {
                    attackedGod = TopGod;
                    amountOfDamage = TopGod.getStriked(amountOfDamage);
                }

                if (!attackedGod.isAlive())
                {
                    AliveGods.Remove(attackedGod);
                }
            }
        }

        public Boolean isAlive()
        {
            return AliveGods.Count > 0;
        }

        public GenericGod getGod(int[] position)
        {
            int row = position[0];
            int col = position[1];
            
            if (row == 0) return TopGod;
            else if (row == 1) return MiddleGods[col];
            else return BaseGods[col];
        }

        public void addGlobalPositiveEffect(EffectOnGod effect)
        {
            foreach (var god in AllGods)
            {
                god.addPositiveEffect(effect);
            }
        }

        public void addGlobalNegativeEffect(EffectOnGod effect)
        {
            foreach (var god in AllGods)
            {
                god.addNegativeEffect(effect);
            } 
        }
        
        public static Boolean rowIsAlive(GenericGod[] gods)
        {
            foreach (var god in gods)
            {
                if (god.isAlive())
                {
                    return true;
                }
            }
            return false;
        }
    }
}