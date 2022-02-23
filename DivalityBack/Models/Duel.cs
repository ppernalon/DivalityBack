using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using DivalityBack.Models.Gods;
using DivalityBack.Services;

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
        public int TurnCount { set; get; }
        public Dictionary<GenericGod, Player> GodsBySpeed { set; get; }

        private Dictionary<GenericGod, Player> sortGodsBySpeed()
        {
            List<GenericGod> godsBySpeed1 = Player1.GodTeam.AllGods.ToList();
            godsBySpeed1.Sort(GenericGod.compareSpeed);
            List<GenericGod> godsBySpeed2 = Player2.GodTeam.AllGods.ToList();
            godsBySpeed2.Sort(GenericGod.compareSpeed);
            Dictionary<GenericGod, Player> sortedGods = new Dictionary<GenericGod, Player>();

            int index1 = 0;
            int index2 = 0;
            
            while (index1 + index2 < 12)
            {
                if (index1 == 6 && index2 < 6)
                {
                    sortedGods.Add(godsBySpeed2[index2], Player2);
                    index2++;
                }
                else if (index1 < 6 && index2 == 6)
                {
                    sortedGods.Add(godsBySpeed1[index2], Player1);
                    index1++;
                }
                else
                {
                    if (godsBySpeed1[index1].Speed >= godsBySpeed2[index2].Speed)
                    {
                        sortedGods.Add(godsBySpeed1[index1], Player1);
                        index1++;
                    }
                    else
                    {
                        sortedGods.Add(godsBySpeed2[index2], Player2);
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

        public Player looser()
        {
            if (Player2.isAlive()) return Player1;
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

        public void initDuel()
        {
            foreach (var god in Player1.GodTeam.AllGods)
            {
                Player1.GodTeam.addGlobalPositiveEffect(god.GlobalAllyEffect);
                Player2.GodTeam.addGlobalNegativeEffect(god.GlobalEnnemyEffect);
            }

            foreach (var god in Player2.GodTeam.AllGods)
            {
                Player2.GodTeam.addGlobalPositiveEffect(god.GlobalAllyEffect);
                Player1.GodTeam.addGlobalNegativeEffect(god.GlobalEnnemyEffect);
            }
        }

        public Boolean play(WebSocketReceiveResult result)
        {
            foreach (var GodAndPlayer in GodsBySpeed)
            {
                GenericGod attacker = GodAndPlayer.Key;
                Player offensivePlayer = GodAndPlayer.Value;

                if (attacker.isAlive()) // a dead god can't attack
                {
                    TurnCount++;

                    int attackerPosition;
                    
                    int[][] attackPattern;
                    GenericGod[] attackedGods;
                    
                    if (offensivePlayer.isEqual(Player1)) // player 2 is attacked
                    {
                        attackedGods = Player2.GodTeam.AllGods;
                        attackPattern = godAttack(attacker, Player2);
                        attackerPosition = Array.IndexOf(Player1.GodTeam.AllGods, attacker);
                    }
                    else // player 1 is attacked
                    {
                        attackedGods = Player1.GodTeam.AllGods;
                        attackPattern = godAttack(attacker, Player1);
                        attackerPosition = Array.IndexOf(Player2.GodTeam.AllGods, attacker);
                    }

                    string updateJson = UtilServices.UpdateDuelJson(attackedGods.ToList(), offensivePlayer.Username, attackPattern, TurnCount, attackerPosition);
                    byte[] updateBytes = Encoding.UTF8.GetBytes(updateJson);
                    
                    Player1.PlayerWebSocket.SendAsync(updateBytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
                    Player2.PlayerWebSocket.SendAsync(updateBytes, result.MessageType, result.EndOfMessage, CancellationToken.None);
                }

                if (!(Player1.isAlive() && Player2.isAlive())) // the game end when one player is dead
                {
                    return true;
                }
            }
            
            return false;
        }

        private int[][] godAttack(GenericGod attackerGod, Player opponentPlayer)
        {
            int[][] attackPattern = attackerGod.getAttackPattern(opponentPlayer.GodTeam);
            opponentPlayer.GodTeam.getStriked(attackerGod.Power, attackPattern);
            return attackPattern;
        }
        
    }
}