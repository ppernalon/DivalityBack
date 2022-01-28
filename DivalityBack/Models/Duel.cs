using System;
using System.Collections.Generic;
using System.Linq;
using DivalityBack.Models.Gods;

namespace DivalityBack.Models
{
    public class Duel
    {
        public Duel(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
            TurnCount = 0;
            GodsBySpeed = sortGodsBySpeed();
        }
        
        public Player Player1 { set; get; }
        public Player Player2 { set; get; }
        public Player OffensivePlayer { set; get; }
        public Player DefensivePlayer { set; get; }
        public int TurnCount { set; get; }
        public Dictionary<Player, GenericGod> GodsBySpeed { set; get; }

        private Dictionary<Player, GenericGod> sortGodsBySpeed()
        {
            List<GenericGod> godsBySpeed1 = Player1.GodTeam.AllGods.ToList();
            godsBySpeed1.Sort(GenericGod.compareSpeed);
            List<GenericGod> godsBySpeed2 = Player2.GodTeam.AllGods.ToList();
            godsBySpeed2.Sort(GenericGod.compareSpeed);
            Dictionary<Player, GenericGod> sortedGods = new Dictionary<Player, GenericGod>();

            int index1 = 0;
            int index2 = 0;
            
            while (index1 + index2 < 12)
            {
                if (index1 == 6 && index2 < 6)
                {
                    sortedGods.Add(Player2, godsBySpeed2[index2]);
                    index2++;
                }
                else if (index1 < 6 && index2 == 6)
                {
                    sortedGods.Add(Player1, godsBySpeed1[index2]);
                    index1++;
                }
                else
                {
                    if (godsBySpeed1[index1].Speed >= godsBySpeed2[index2].Speed)
                    {
                        sortedGods.Add(Player1, godsBySpeed1[index1]);
                        index1++;
                    }
                    else
                    {
                        sortedGods.Add(Player2, godsBySpeed2[index2]);
                        index2++;
                    }
                }
            }

            return sortedGods;
        }
        
        public Boolean isOver()
        {
            return !(Player1.isAlive() && Player2.isAlive());
        }

        public Player winner()
        {
            if (Player1.isAlive()) return Player1;
            else return Player2;
        }

        public Boolean didPlayerOneWon()
        {
            return Player1.isAlive();
        }
        
        public Boolean didPlayerTwoWon()
        {
            return Player2.isAlive();
        }

        public Boolean play()
        {
            foreach (var GodAndPlayer in GodsBySpeed)
            {
                TurnCount++;
                
                GenericGod attacker = GodAndPlayer.Value;
                Player offensivePlayer = GodAndPlayer.Key;

                if (attacker.isAlive()) // a dead god can't attack
                {
                    if (offensivePlayer.isEqual(Player1)) // player 2 is attacked
                    {
                        godAttack(attacker, Player2);
                    }
                    else // player 1 is attacked
                    {
                        godAttack(attacker, Player1);
                    }
                }

                if (!(Player1.isAlive() && Player2.isAlive())) // the game end when one player is dead
                {
                    break;
                }
            }
            
            return true;
        }

        private void godAttack(GenericGod attackerGod, Player opponentPlayer)
        {
            int[][] attackPattern = attackerGod.getAttackPattern(opponentPlayer.GodTeam);
            int reducedDamage = opponentPlayer.GodTeam.getStriked(attackerGod.Power, attackPattern);
            opponentPlayer.getStriked(reducedDamage);
        }
        
    }
}