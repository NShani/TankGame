using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace TanksTheGame
{
    class AI
    {
        static Node myself, start, current, temp;
        static int myDirection;

        static List<Node> shortClose = new List<Node>();
        static List<Node> closed = new List<Node>();
        static List<Node> open = new List<Node>();

        //find and return my tank in Node form (for AI)
        //[MethodImpl(MethodImplOptions.Synchronized)]
        static public void findMe()
        {
            Item item;
            myself = null;

            for (int y = 0; y < Board.SIZE; y++)
            {
                for (int x = 0; x < Board.SIZE; x++)
                {
                    item = Board.get(x, y);
                    if (item is Tank && ((Tank)item).name == Board.me)    //found!
                    {
                        myself = new Node();
                        myself.x = x;
                        myself.y = y;
                        myDirection = ((Tank)item).direction;
                        return;
                    }
                }
            }
        }

        //controller method
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void run()
        {
            Console.WriteLine("AI started");

            shortClose.Capacity = 100;
            open.Capacity = 100;
            closed.Capacity = 100;

            findMe();
            if (myself == null)
            {
                Console.WriteLine("Error: Could not find my tank!");
                return;
            }

            findClosest();
            if (shortClose.Count > 1)
                move();
            else
                fire();

            Console.WriteLine("AI finished");
        }

        //find shortest path to nearest valuable item (coin pile/life pack)
        public static void findClosest()
        {
            Item item, tempItem;
            int pos, x, y;

            for (y = 0; y < Board.SIZE; y++)
            {
                for (x = 0; x < Board.SIZE; x++)
                {
                    item = Board.get(x, y);

                    if (item is TempItem)
                    {
                        start = new Node();
                        start.f = 0;
                        start.g = 0;
                        start.h = 0;
                        start.x = x;
                        start.y = y;
                        open.Add(start);

                        while (true)
                        {
                            //remove first element from open list and add it to close list
                            current = open[0];
                            closed.Add(current);
                            open.RemoveAt(0);

                            for (int xT = current.x - 1; xT < current.x + 2; xT++)
                            {
                                if (xT < 0 || xT >= Board.SIZE) //out of bounds
                                    continue;

                                for (int yT = current.y - 1; yT < current.y + 2; yT++)
                                {
                                    //ignore current cell
                                    if (xT == current.x && yT == current.y)
                                        continue;

                                    //ignore diagonals
                                    if ((xT - current.x) * (yT - current.y) != 0)
                                        continue;

                                    if (yT < 0 || yT >= Board.SIZE) //out of bounds
                                        continue;

                                    tempItem = Board.get(xT, yT);
                                    if (tempItem is Stone || tempItem is Wall || tempItem is Water) //obstacle! ignore
                                        continue;

                                    temp = new Node();
                                    temp.x = xT;
                                    temp.y = yT;
                                    temp.g = current.g + Math.Abs(xT - current.x) + Math.Abs(yT - current.y);
                                    temp.h = Math.Abs(xT - myself.x) + Math.Abs(yT - myself.y);
                                    temp.f = temp.g + temp.h;

                                    pos = open.IndexOf(temp);
                                    if (pos >= 0 && open[pos].g > temp.g)
                                    {
                                        open[pos].g = temp.g;
                                        open[pos].f = open[pos].g + open[pos].h;
                                    }
                                    else if(closed.IndexOf(temp) < 0)
                                    {
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
        public static void move()
        {
            if (myself.x > shortClose[shortClose.Count - 2].x)
            {
                ComLink.send("LEFT#");
            }
            else if (myself.x < shortClose[shortClose.Count - 2].x)
            {
                ComLink.send("RIGHT#");
            }
            else if (myself.y > shortClose[shortClose.Count - 2].y)
            {
                ComLink.send("UP#");
            }
            else if (myself.y < shortClose[shortClose.Count - 2].y)
            {
                ComLink.send("DOWN#");
            }

            shortClose.Clear();
        }

        public static void fire()
        {
            Item target;

            switch(myDirection) {
                case GameWindow.EAST:   //shoot East
                    for (int i = myself.x + 1; i < Board.SIZE; i++)
                    {
                        target = Board.get(i, myself.y);
                        if (target is Tank)
                        {
                            ComLink.send("SHOOT#");
                            break;
                        }
                        else if (target is Stone)
                            break;
                    }
                    break;

                case GameWindow.SOUTH:   //shoot South
                    for (int i = myself.y + 1; i < Board.SIZE; i++)
                    {
                        target = Board.get(myself.x, i);
                        if (target is Tank)
                        {
                            ComLink.send("SHOOT#");
                            break;
                        }
                        else if (target is Stone)
                            break;
                    }
                    break;

                case GameWindow.WEST:   //shoot West
                    for (int i = myself.x - 1; i >= 0; i--)
                    {
                        target = Board.get(i, myself.y);
                        if (target is Tank)
                        {
                            ComLink.send("SHOOT#");
                            break;
                        }
                        else if (target is Stone)
                            break;
                    }
                    break;

                case GameWindow.NORTH:   //shoot North
                    for (int i = myself.y - 1; i >= 0; i++)
                    {
                        target = Board.get(myself.x, i);
                        if (target is Tank)
                        {
                            ComLink.send("SHOOT#");
                            break;
                        }
                        else if (target is Stone)
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
