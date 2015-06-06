using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TanksTheGame
{
    class Board
    {
        public const int SIZE = 20;
        public const int UPDATE_INTERVAL = 50;          //board update interval
        private static Item[,] map = new Item[SIZE, SIZE];

        public static String me;                        //myself (my tank)

        static public void clearForUpdate()
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    //clear all tanks and brick walls
                    if(map[y, x] is Tank || map[y, x] is Wall)
                        map[y, x] = new Nothing();
                }
            }
        }

        static public void initialize()
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    if(map[y, x] == null)
                        map[y, x] = new Nothing();
                }
            }
        }

        static public Item get(int x, int y)
        {
            if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
                return map[x, y];
            else
                return null;
        }

        static public bool set(int x, int y, Item item)
        {
            if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            {
                map[x, y] = item;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// this method shows the current map status with names
        /// </summary>
        public static void show()
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    if (map[y, x] is Tank)
                        Console.Write(((Tank)map[y,x]).name);
                    else if (map[y, x] is Wall)
                        Console.Write(((Wall)map[y, x]).damage / 25);
                    else if (map[y, x] is LifePack)
                        Console.Write("L");
                    else if (map[y, x] is CoinPile)
                        Console.Write("C");
                    else if (map[y, x] is Stone)
                        Console.Write("S");
                    else if (map[y, x] is Water)
                        Console.Write("W");
                    else if (map[y, x] is Nothing)
                        Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public static void spendLife()
        {
            while (true)
            {
                TempItem item;

                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        if (map[y, x] is TempItem)
                        {
                            //decrease expiry times once in each update run
                            //(assuming updates happen every second)
                            item = (TempItem)map[y, x];
                            item.timeToExpire -= UPDATE_INTERVAL;

                            //remove if expired
                            if (item.timeToExpire <= 0)
                            {
                                map[y, x] = new Nothing();
                            }
                        }
                    }
                }

                //delay before next update
                Thread.Sleep(UPDATE_INTERVAL);
            }
        }
    }
}
