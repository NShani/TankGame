using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TanksTheGame
{
    class AI
    {
        const int TREASURE = 1;
        const int ENEMY = 2;
        const int CRITICAL_LIFE = 50;   //critical life level (for searching life packs)

        static Node myself, start, current, temp;
        static Tank myTank;
        static int myDirection;

        static List<Node> shortClose = new List<Node>();
        static List<Node> closed = new List<Node>();
        static List<Node> open = new List<Node>();

        //controller method; synchronized
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void run()
        {
            Console.WriteLine("AI started");

            //Thread.Sleep(1000);
            //ComLink.send("DOWN#");
            //Thread.Sleep(1200);
            //ComLink.send("DOWN#");
            //Thread.Sleep(1200);
            //ComLink.send("RIGHT#");
            //Thread.Sleep(1200);
            //ComLink.send("RIGHT#");
            //Thread.Sleep(1000);
            //ComLink.send("DOWN#");
            //Thread.Sleep(1200);
            //ComLink.send("DOWN#");
            //while (true)
            //{
            //    Thread.Sleep(1100);
            //    ComLink.send("SHOOT#");
            //}

            //try to locate myself; quit if failed
            findMe();
            if (myself == null)
            {
                Console.WriteLine("Error: Could not find my tank!");
                return;
            }

            //try to get a coin pile
            findClosestCoinPile();
            if (shortClose.Count > 1)
            {
                //success! go get it
                move();
            }
            else
            {
                //try to get a life pack
                findClosestLifePack();
                if (shortClose.Count > 1)
                {
                    //success! go get it
                    move();
                }

                //if path not found to life pack
                if (shortClose.Count == 0)
                {
                    //fire closest enemy
                    fire();
                }
            }

            Console.WriteLine("AI finished");
        }

        //find and return my tank in Node form (for AI)
        static public void findMe()
        {
            Item item;
            myself = null;
            myTank = null;

            for (int y = 0; y < Board.SIZE; y++)
            {
                for (int x = 0; x < Board.SIZE; x++)
                {
                    item = Board.get(x, y);
                    if (item is Tank && ((Tank)item).name == Board.me)    //found!
                    {
                        myTank = (Tank)item;
                        myself = new Node();
                        myself.x = x;
                        myself.y = y;
                        myDirection = ((Tank)item).direction;
                        return;
                    }
                }
            }
        }

        //find shortest path to nearest coin pile
        public static void findClosestCoinPile()
        {
            Item item, tempItem;
            int pos, x, y;
            int tempMotion, motion = -1;

            //clean history before start
            shortClose.Clear();

            for (y = 0; y < Board.SIZE; y++)
            {
                for (x = 0; x < Board.SIZE; x++)
                {
                    item = Board.get(x, y);

                    if (item is CoinPile)
                    {
                        start = new Node();
                        start.f = 0;
                        start.g = 0;
                        start.h = 0;
                        start.x = x;
                        start.y = y;
                        start.motion = -1;
                        open.Add(start);

                        while (true)
                        {
                            //remove first element from open list and add it to close list
                            current = open[0];
                            closed.Add(current);
                            open.RemoveAt(0);

                            motion = current.motion;
                            tempMotion = -1;

                            for (int xT = current.x - 1; xT < current.x + 2; xT++)
                            {
                                if (xT < 0 || xT >= Board.SIZE) //out of bounds
                                    continue;

                                for (int yT = current.y - 1; yT < current.y + 2; yT++)
                                {
                                    //ignore current cell and diagonals
                                    if (!(xT == current.x ^ yT == current.y))
                                        continue;

                                    if (yT < 0 || yT >= Board.SIZE) //out of bounds
                                        continue;

                                    tempItem = Board.get(xT, yT);
                                    if (tempItem is Stone || tempItem is Wall || tempItem is Water ||
                                        (tempItem is Tank && ((Tank)tempItem).name != Board.me))
                                        //obstacle or another tank! ignore
                                        continue;

                                    temp = new Node();
                                    temp.x = xT;
                                    temp.y = yT;
                                    temp.g = current.g + Math.Abs(xT - current.x) + Math.Abs(yT - current.y);

                                    //determine motion direction
                                    //add extra cost if direction must be changed
                                    if (xT > current.x)
                                    {
                                        if (motion != -1 && motion != GameWindow.RIGHT)
                                            temp.g++;
                                        tempMotion = GameWindow.RIGHT;
                                    }
                                    if (xT < current.x)
                                    {
                                        if (motion != -1 && motion != GameWindow.LEFT)
                                            temp.g++;
                                        tempMotion = GameWindow.LEFT;
                                    }
                                    if (yT > current.y)
                                    {
                                        if (motion != -1 && motion != GameWindow.DOWN)
                                            temp.g++;
                                        tempMotion = GameWindow.DOWN;
                                    }
                                    if (yT < current.y)
                                    {
                                        if (motion != -1 && motion != GameWindow.UP)
                                            temp.g++;
                                        tempMotion = GameWindow.UP;
                                    }

                                    temp.h = Math.Abs(xT - myself.x) + Math.Abs(yT - myself.y);
                                    temp.f = temp.g + temp.h;

                                    pos = open.IndexOf(temp);
                                    if (pos >= 0 && open[pos].g > temp.g)
                                    {
                                        open[pos].g = temp.g;
                                        open[pos].f = open[pos].g + open[pos].h;
                                    }
                                    else if (closed.IndexOf(temp) < 0)
                                    {
                                        temp.motion = tempMotion;
                                        open.Add(temp);
                                    }
                                }
                            }

                            //quit if nothing found
                            if (open.Count == 0)
                                break;

                            //sort open list
                            open.Sort(CompareNodes);

                            //exit condition
                            if (closed[closed.Count - 1].x == myself.x && closed[closed.Count - 1].y == myself.y)
                                break;

                        }
                    }

                    //is current closed list the shortest?
                    if (closed.Count > 0 && (shortClose.Count > closed.Count || shortClose.Count == 0))
                    {
                        shortClose = new List<Node>(closed);
                    }

                    //clean up before next iteration
                    open.Clear();
                    closed.Clear();
                }
            }
        }

        //find shortest path to nearest life pack
        public static void findClosestLifePack()
        {
            Item item, tempItem;
            int pos, x, y;
            int tempMotion, motion = -1;

            //clean history before start
            shortClose.Clear();

            for (y = 0; y < Board.SIZE; y++)
            {
                for (x = 0; x < Board.SIZE; x++)
                {
                    item = Board.get(x, y);

                    if (item is LifePack)
                    {
                        start = new Node();
                        start.f = 0;
                        start.g = 0;
                        start.h = 0;
                        start.x = x;
                        start.y = y;
                        start.motion = -1;
                        open.Add(start);

                        while (true)
                        {
                            //remove first element from open list and add it to close list
                            current = open[0];
                            closed.Add(current);
                            open.RemoveAt(0);

                            motion = current.motion;
                            tempMotion = -1;

                            for (int xT = current.x - 1; xT < current.x + 2; xT++)
                            {
                                if (xT < 0 || xT >= Board.SIZE) //out of bounds
                                    continue;

                                for (int yT = current.y - 1; yT < current.y + 2; yT++)
                                {
                                    //ignore current cell and diagonals
                                    if (!(xT == current.x ^ yT == current.y))
                                        continue;

                                    if (yT < 0 || yT >= Board.SIZE) //out of bounds
                                        continue;

                                    tempItem = Board.get(xT, yT);
                                    if (tempItem is Stone || tempItem is Wall || tempItem is Water ||
                                        (tempItem is Tank && ((Tank)tempItem).name != Board.me))
                                        //obstacle or another tank! ignore
                                        continue;

                                    temp = new Node();
                                    temp.x = xT;
                                    temp.y = yT;
                                    temp.g = current.g + Math.Abs(xT - current.x) + Math.Abs(yT - current.y);

                                    //determine motion direction
                                    //add extra cost if direction must be changed
                                    if (xT > current.x)
                                    {
                                        if (motion != -1 && motion != GameWindow.RIGHT)
                                            temp.g++;
                                        tempMotion = GameWindow.RIGHT;
                                    }
                                    if (xT < current.x)
                                    {
                                        if (motion != -1 && motion != GameWindow.LEFT)
                                            temp.g++;
                                        tempMotion = GameWindow.LEFT;
                                    }
                                    if (yT > current.y)
                                    {
                                        if (motion != -1 && motion != GameWindow.DOWN)
                                            temp.g++;
                                        tempMotion = GameWindow.DOWN;
                                    }
                                    if (yT < current.y)
                                    {
                                        if (motion != -1 && motion != GameWindow.UP)
                                            temp.g++;
                                        tempMotion = GameWindow.UP;
                                    }

                                    temp.h = Math.Abs(xT - myself.x) + Math.Abs(yT - myself.y);
                                    temp.f = temp.g + temp.h;

                                    pos = open.IndexOf(temp);
                                    if (pos >= 0 && open[pos].g > temp.g)
                                    {
                                        open[pos].g = temp.g;
                                        open[pos].f = open[pos].g + open[pos].h;
                                    }
                                    else if (closed.IndexOf(temp) < 0)
                                    {
                                        temp.motion = tempMotion;
                                        open.Add(temp);
                                    }
                                }
                            }

                            //quit if nothing found
                            if (open.Count == 0)
                                break;

                            //sort open list
                            open.Sort(CompareNodes);

                            //exit condition
                            if (closed[closed.Count - 1].x == myself.x && closed[closed.Count - 1].y == myself.y)
                                break;

                        }
                    }

                    //is current closed list the shortest?
                    if (closed.Count > 0 && (shortClose.Count > closed.Count || shortClose.Count == 0))
                    {
                        shortClose = new List<Node>(closed);
                    }

                    //clean up before next iteration
                    open.Clear();
                    closed.Clear();
                }
            }
        }

        //comparator for non-increasing sort order
        private static int CompareNodes(Node n1, Node n2)
        {
            return n1.f - n2.f;
        }

        //generate move
        public static bool move()
        {
            switch(shortClose[shortClose.Count - 1].motion)
            {
                case GameWindow.LEFT:
                    ComLink.send("RIGHT#");
                    break;
                case GameWindow.RIGHT:
                    ComLink.send("LEFT#");
                    break;
                case GameWindow.UP:
                    ComLink.send("DOWN#");
                    break;
                case GameWindow.DOWN:
                    ComLink.send("UP#");
                    break;
                default:
                    return false;
            }

            //if we didn't hit the else, we have made a move
            return true;
        }

        //public static bool fire()
        //{
        //    Item target;

        //    switch (myDirection)
        //    {
        //        case GameWindow.RIGHT:   //shoot East
        //            for (int i = myself.x + 1; i < Board.SIZE; i++)
        //            {
        //                target = Board.get(i, myself.y);
        //                if (target is Tank)
        //                {
        //                    ComLink.send("SHOOT#");
        //                    return true;
        //                }
        //                else if (target is Stone)
        //                    return false;
        //            }
        //            break;

        //        case GameWindow.DOWN:   //shoot South
        //            for (int i = myself.y + 1; i < Board.SIZE; i++)
        //            {
        //                target = Board.get(myself.x, i);
        //                if (target is Tank)
        //                {
        //                    ComLink.send("SHOOT#");
        //                    return true;
        //                }
        //                else if (target is Stone)
        //                    return false;
        //            }
        //            break;

        //        case GameWindow.LEFT:   //shoot West
        //            for (int i = myself.x - 1; i >= 0; i--)
        //            {
        //                target = Board.get(i, myself.y);
        //                if (target is Tank)
        //                {
        //                    ComLink.send("SHOOT#");
        //                    return true;
        //                }
        //                else if (target is Stone)
        //                    return false;
        //            }
        //            break;

        //        case GameWindow.UP:   //shoot North
        //            for (int i = myself.y - 1; i >= 0; i++)
        //            {
        //                target = Board.get(myself.x, i);
        //                if (target is Tank)
        //                {
        //                    ComLink.send("SHOOT#");
        //                    return true;
        //                }
        //                else if (target is Stone)
        //                    return false;
        //            }
        //            break;

        //        default:    //can't shoot
        //            break;
        //    }

        //    //if we reach here, no shooting could be done
        //    return false;
        //}

        public static bool fire()
        {
            Item target;

            for (int i = myself.x + 1; i < Board.SIZE; i++)
            {
                target = Board.get(i, myself.y);
                if (target is Tank)
                {
                    //if already in that direction
                    if(myDirection == GameWindow.RIGHT)
                        ComLink.send("SHOOT#");
                    else
                        //turn to that direction
                        ComLink.send("RIGHT#");
                    return true;
                }
                else if (target is Stone)
                    return false;
            }

            for (int i = myself.y + 1; i < Board.SIZE; i++)
            {
                target = Board.get(myself.x, i);
                if (target is Tank)
                {
                    //if already in that direction
                    if(myDirection == GameWindow.DOWN)
                        ComLink.send("SHOOT#");
                    else
                        //turn to that direction
                        ComLink.send("DOWN#");
                    return true;
                }
                else if (target is Stone)
                    return false;
            }

            for (int i = myself.x - 1; i >= 0; i--)
            {
                target = Board.get(i, myself.y);
                if (target is Tank)
                {
                    //if already in that direction
                    if(myDirection == GameWindow.LEFT)
                        ComLink.send("SHOOT#");
                    else
                        //turn to that direction
                        ComLink.send("LEFT#");
                    return true;
                }
                else if (target is Stone)
                    return false;
            }

            for (int i = myself.y - 1; i >= 0; i++)
            {
                target = Board.get(myself.x, i);
                if (target is Tank)
                {
                    //if already in that direction
                    if(myDirection == GameWindow.UP)
                        ComLink.send("SHOOT#");
                    else
                        //turn to that direction
                        ComLink.send("UP#");
                    return true;
                }
                else if (target is Stone)
                    return false;
            }

            //if we reach here, no shooting could be done
            return false;
        }
    }
}
