using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksTheGame
{
    public class Item
    {
        //public int x, y;
    }

    public class Nothing : Item
    {
    }

    public class Stone : Item
    {
    }

    public class Water : Item
    {
    }

    public class Wall : Item
    {
        public int damage;
    }

    public class Tank : Item
    {
        public string name;
        public bool shot;
        public int life, coins, points, direction;
    }

    public class TempItem : Item
    {
        public int timeToExpire;
    }

    public class LifePack : TempItem
    {
    }

    public class CoinPile : TempItem
    {
        public int value;
    }
}
