using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksTheGame
{
    class Node
    {
        public int x, y;    //location
        public int f, g, h; //for A*
        public int motion;  //motion direction

        public override bool Equals(Object o)
        {
            if(!(o is Node))
                return false;
            Node n = (Node)o;
            return n.x == this.x && n.y == this.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + x * 31 + y * 61 + f * 93 + g * 103 + h * 113;
        }
    }
}
