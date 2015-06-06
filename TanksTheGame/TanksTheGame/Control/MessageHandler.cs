using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TanksTheGame
{
    class MessageHandler
    {
        public static void handle(String message)
        {
            if (message.StartsWith("S:"))
            {
                doAccept(message);
            }
            else if (message.StartsWith("I:"))
            {
                doInit(message);
            }
            else if (message.StartsWith("G:"))
            {
                doGlobal(message);
            }
            else if (message.StartsWith("C:"))
            {
                addCoinPile(message);
            }
            else if (message.StartsWith("L:"))
            {
                addLifePack(message);
            }
            else if(message.Equals("GAME_ALREADY_STARTED"))
            {
                Program.showMessage("Sorry, the game has already started!");

                //add delay to ensure message display
                Thread.Sleep(500);

                //quit
                Program.exit();
            }
            else if (message.Equals("NOT_A_VALID_CONTESTANT"))
            {
                Program.showMessage("Sorry, you're not eligible to participate in this game.");

                //add delay to ensure message display
                Thread.Sleep(500);

                //quit
                Program.exit();
            }
            else if (message.Equals("PITFALL"))
            {
                Program.showMessage("Oops... Pitfall! You're dead!");
            }
            else if (message.Equals("GAME_FINISHED"))
            {
                Program.showMessage("Game over!");

                //add delay to ensure message display
                Thread.Sleep(500);

                //quit
                Program.exit();
            }
            else
            {
                Console.WriteLine("Error: " + message);
            }
        }

        public static void doAccept(String message)
        {
            //clean board
            Board.initialize();

            //start board update thread
            new Thread(Board.spendLife).Start();

            string[] tanks = message.Split(':');

            foreach (string tankStr in tanks)
            {
                if (tankStr.StartsWith("P"))
                {
                    string[] tank_info = tankStr.Split(';');
                    string[] position = tank_info[1].Split(',');

                    Tank tank = new Tank();
                    tank.name = tankStr.Substring(1, 1);
                    tank.life = 100;
                    tank.points = 0;
                    tank.direction = Convert.ToInt32(tank_info[2]);
                    Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), tank);
                }
            }

            //launch interface
            new Thread(Program.runGame).Start();
        }

        static void doInit(String message)
        {
            string[] ImsgArray = message.Split(':'); //divides initial msg by ':' sign

            Board.me = ImsgArray[1];

            string[] walls = ImsgArray[2].Split(';');
            foreach (string wallStr in walls)
            {
                string[] position = wallStr.Split(',');
                Wall wall = new Wall();
                wall.damage = 0;
                Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), wall);
            }

            // create stones
            string[] stones = ImsgArray[3].Split(';');
            foreach (string stone in stones)
            {
                string[] position = stone.Split(',');
                Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), new Stone());
            }

            // create water
            string[] waters = ImsgArray[4].Split(';');
            foreach (string water in waters)
            {
                string[] position = water.Split(',');
                Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), new Water());
            }
        }

        static void doGlobal(String message)
        {
            Board.clearForUpdate();

            string[] global_info = message.Split(':');
            foreach (string info in global_info)
            {
                if (info.StartsWith("P"))
                {
                    string[] tank_info = info.Split(';');
                    string[] position = tank_info[1].Split(',');

                    Tank tank = new Tank();
                    tank.name = tank_info[0];
                    tank.direction = Convert.ToInt32(tank_info[2]);
                    tank.shot = (Convert.ToInt32(tank_info[3]) == 1);
                    tank.life = Convert.ToInt32(tank_info[4]);
                    tank.coins = Convert.ToInt32(tank_info[5]);
                    tank.points = Convert.ToInt32(tank_info[6]);
                    Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), tank);
                }
                else if (!info.StartsWith("G"))
                {
                    // select wall info
                    string[] walls = info.Split(';');
                    foreach (string wallStr in walls)
                    {
                        string[] status = wallStr.Split(',');
                        Wall wall = new Wall();
                        wall.damage = Convert.ToInt32(status[2]) * 25;
                        Board.set(Convert.ToInt32(status[0]), Convert.ToInt32(status[1]), wall);
                    }
                }
            }

            //now graphics thread will automatically update the board

            //launch AI for calculation & sending command
            new Thread(AI.run).Start();
        }

        static void addCoinPile(String message)
        {
            string[] coin = message.Split(':');
            string[] position = coin[1].Split(',');

            CoinPile pile = new CoinPile();
            pile.timeToExpire = Convert.ToInt32(coin[2]);
            pile.value = Convert.ToInt32(coin[3]);
            Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), pile);

            //run AI if last update was more than 1 second ago-- for making advantage of browser 'stuck' glitch
            if(DateTime.UtcNow.Subtract(ComLink.lastSend).TotalMilliseconds > ComLink.MIN_COMM_GAP)
                new Thread(AI.run).Start();
        }

        static void addLifePack(String message)
        {
            string[] lifepack = message.Split(':');
            string[] position = lifepack[1].Split(',');

            LifePack pack = new LifePack();
            pack.timeToExpire = Convert.ToInt32(lifepack[2]);
            Board.set(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]), pack);

            //run AI if last update was more than 1 second ago-- for making advantage of browser 'stuck' glitch
            if (DateTime.UtcNow.Subtract(ComLink.lastSend).TotalMilliseconds > ComLink.MIN_COMM_GAP)
                new Thread(AI.run).Start();
        }
    }
}
