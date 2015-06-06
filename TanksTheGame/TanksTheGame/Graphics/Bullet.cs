using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace TanksTheGame
{
    class Bullet
    {
        const int SPEED_REL_TANK = 3;       //bullet speed relative to tank

        public bool alive;          //to determine wherther bullet is fired or not
        public Texture2D bulletImage;
        public Vector2 position, velocity;
        public int x, y, deltaX, deltaY;

        private int count = 0;      //counter to keep track of updates

        public Bullet(Texture2D loadedImage, Vector2 origin, int x, int y, int direction)    //bullet constructor.
        {
            this.bulletImage = loadedImage;
            this.position = origin;
            this.alive = true;
            this.x = x;
            this.y = y;

            //detect direction
            switch (direction)
            {
                case GameWindow.UP:
                    deltaX = 0;
                    deltaY = -SPEED_REL_TANK;
                    break;
                case GameWindow.DOWN:
                    deltaX = 0;
                    deltaY = SPEED_REL_TANK;
                    break;
                case GameWindow.LEFT:
                    deltaX = -SPEED_REL_TANK;
                    deltaY = 0;
                    break;
                case GameWindow.RIGHT:
                    deltaX = SPEED_REL_TANK;
                    deltaY = 0;
                    break;
                default:
                    Console.WriteLine("Error: Invalid tank direction while shooting");
                    break;
            }
            this.velocity = GameWindow.TANK_SPEED * new Vector2(deltaX, deltaY);
        }

        public void update(Rectangle gameArea) // update bullet details
        {
            if (alive) {
                //update board indices each second
                if (count == GameWindow.REFRESH_RATE)
                {
                    x += deltaX;
                    y += deltaY;
                    count = 0;
                }
                else
                    count++;

                position += velocity;   //update position

                //die if outside of board
                if (gameArea.Contains((int)position.X + bulletImage.Width * Math.Sign(deltaX), (int)position.Y + bulletImage.Height * Math.Sign(deltaY)))
                {
                    //check if bullet has collided with something
                    checkCollision();
                }
                else
                {
                    alive = false;    // if bullet not in the grid space bullet live is false
                }
            }
        }

        private void checkCollision()
        {
            Item item = Board.get(x, y);
            if (item is Stone || (item is Wall && ((Wall)item).damage < 100) ||    //obstacle
                (item is Tank && ((Tank)item).name != Board.me))  //different tank
            {
                alive = false;
            }
        }

        public void draw(SpriteBatch spritebatch) 
        {
            spritebatch.Draw(bulletImage, position, Color.White);  //position to be calculated
        }
    }
}
