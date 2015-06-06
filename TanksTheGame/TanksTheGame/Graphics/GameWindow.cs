using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TanksTheGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameWindow : Microsoft.Xna.Framework.Game
    {
        //graphics update parameters
        public const int REFRESH_RATE = 60;
        public const int CELL_SIZE = 35;
        public const float TANK_SPEED = 1.0f * CELL_SIZE / REFRESH_RATE;     //distance travelled per screen update

        //direction representation
        public const int NORTH = 0;
        public const int UP = 0;
        public const int EAST = 1;
        public const int RIGHT = 1;
        public const int SOUTH = 2;
        public const int DOWN = 2;
        public const int WEST = 3;
        public const int LEFT = 3;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle viewportRect;    //background
        Rectangle gameArea;        //game region

        //margins & sizes
        const int vertMargin = 5;
        const int leftMargin = 5;

        //offsets for score display
        const int scoreArea = 300;
        const int scoreOffsetTop = 250;
        const int nameOffset = 10;
        const int coinsOffset = 80;
        const int pointsOffset = 150;
        const int healthOffset = 220;

        //offsets for logo
        const int logoLeft = 15;
        const int logoTop = 650;

        //each image is 35*35; must add offset to centre in cell
        Vector2 normOffset = new Vector2(leftMargin + (CELL_SIZE - 35) / 2, vertMargin + (CELL_SIZE - 35) / 2);

        //location of rotational origin of tank
        Vector2 tankOffset = new Vector2(leftMargin + CELL_SIZE / 2, vertMargin + CELL_SIZE / 2);

        //rotation origin of tank (25*30 image)
        Vector2 origin = new Vector2(17.5f, 17.5f);

        //holds shot bullets
        List<Bullet> shots = new List<Bullet>();

        // for images
        Texture2D stoneImage;
        Texture2D waterImage;
        Texture2D brick100;
        Texture2D brick75;
        Texture2D brick50;
        Texture2D brick25;
        Texture2D coinImage;
        Texture2D lifePackImage;
        Texture2D emptyImage;
        Texture2D myImage;
        Texture2D enemyImage;
        Texture2D bulletImage;
        Texture2D logo;

        //to store fonts
        SpriteFont scoreFonts;

        public GameWindow()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            Program.exit();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //loading images
            stoneImage = Content.Load<Texture2D>("image\\stone");
            waterImage = Content.Load<Texture2D>("image\\water");
            brick100 = Content.Load<Texture2D>("image\\brickLife100");
            brick75 = Content.Load<Texture2D>("image\\brickLife75");
            brick50 = Content.Load<Texture2D>("image\\brickLife50");
            brick25 = Content.Load<Texture2D>("image\\brickLife25");
            coinImage = Content.Load<Texture2D>("image\\coinPile");
            lifePackImage = Content.Load<Texture2D>("image\\lifePack");
            emptyImage = Content.Load<Texture2D>("image\\blank");
            myImage = Content.Load<Texture2D>("image\\me");
            enemyImage = Content.Load<Texture2D>("image\\enemy");
            bulletImage = Content.Load<Texture2D>("image\\bullet");
            logo = Content.Load<Texture2D>("image\\logo");

            //loading fonts
            scoreFonts = Content.Load<SpriteFont>("fonts\\ScoreFont");

            // change the size of the game console
            graphics.PreferredBackBufferHeight = 2 * vertMargin + CELL_SIZE * Board.SIZE;          //10 is footer
            graphics.PreferredBackBufferWidth = leftMargin + CELL_SIZE * Board.SIZE + scoreArea;   //300 is for scoring
            graphics.ApplyChanges();

            //set the size of the rectangle
            viewportRect = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
            gameArea = new Rectangle(leftMargin, vertMargin, CELL_SIZE * Board.SIZE, CELL_SIZE * Board.SIZE);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Program.exit();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Program.exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.Draw(emptyImage, gameArea, Color.White);

            Item item;
            Tank tank;
            Texture2D image;
            Vector2 position;
            List<Tank> tanks = new List<Tank>();    //for showing scores

            for (int x = 0; x < Board.SIZE; x++)
            {
                for (int y = 0; y < Board.SIZE; y++)
                {
                    item = Board.get(x, y);
                    position = new Vector2(CELL_SIZE * x, CELL_SIZE * y);

                    if (item is Tank)
                    {
                        position += tankOffset;
                        tank = (Tank)item;
                        tanks.Add(tank);

                        if (tank.name == Board.me)   //my tank
                            image = myImage;
                        else                         //enemy tank
                            image = enemyImage;

                        //launch bullet if tank has shot one
                        if (tank.shot)
                        {
                            //avoid multiple bullets
                            tank.shot = false;

                            Bullet bullet = new Bullet(bulletImage, position - new Vector2(bulletImage.Width / 2, bulletImage.Height / 2), x, y, tank.direction);

                            //find next empty/dead position in shots list, and add bullet
                            int i = 0;
                            for (; i < shots.Count; i++)
                            {
                                if (shots[i] == null || shots[i].alive == false)    //empty/dead
                                {
                                    shots[i] = bullet;
                                    break;
                                }
                            }
                            if (i == shots.Count)   //no empty/dead positions found; add to end of list
                                shots.Add(bullet);
                        }

                        //draw tank over bullet
                        spriteBatch.Draw(image, position, null, Color.White, rotate(tank.direction), origin, 1.0f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        position += normOffset;   //regular offset for other objects
                        if (item is Wall)
                            switch (((Wall)item).damage)
                            {
                                case 100: //no need to draw
                                    break;
                                case 75:
                                    spriteBatch.Draw(brick25, position, Color.White);
                                    break;
                                case 50:
                                    spriteBatch.Draw(brick50, position, Color.White);
                                    break;
                                case 25:
                                    spriteBatch.Draw(brick75, position, Color.White);
                                    break;
                                case 0:
                                    spriteBatch.Draw(brick100, position, Color.White);
                                    break;
                                default:
                                    Console.Write("Error: Invalid wall damage level");
                                    break;
                            }
                        else if (item is LifePack)
                            spriteBatch.Draw(lifePackImage, position, Color.White);
                        else if (item is CoinPile)
                            spriteBatch.Draw(coinImage, position, Color.White);
                        else if (item is Stone)
                            spriteBatch.Draw(stoneImage, position, Color.White);
                        else if (item is Water)
                            spriteBatch.Draw(waterImage, position, Color.White);
                        else if (!(item is Nothing))
                            Console.Write("Error: Invalid item found on board");
                    }
                }
            }

            //draw bullets
            for (int i = 0; i < shots.Count; i++)
            {
                if (shots[i].alive) //live bullet
                {
                    shots[i].update(gameArea);
                    shots[i].draw(spriteBatch);
                }
            }

            //show scores
            spriteBatch.DrawString(scoreFonts, "Coins", new Vector2(2 * leftMargin + CELL_SIZE * Board.SIZE + coinsOffset, vertMargin + scoreOffsetTop), Color.White);
            spriteBatch.DrawString(scoreFonts, "Points", new Vector2(2 * leftMargin + CELL_SIZE * Board.SIZE + pointsOffset, vertMargin + scoreOffsetTop), Color.White);
            spriteBatch.DrawString(scoreFonts, "Health", new Vector2(2 * leftMargin + CELL_SIZE * Board.SIZE + healthOffset, vertMargin + scoreOffsetTop), Color.White);

            tanks.Sort(CompareTanks);

            //draw my info in white, the rest in gray
            for (int i = 0; i < tanks.Count; i++)
            {
                spriteBatch.DrawString(scoreFonts, tanks[i].name, new Vector2(leftMargin + CELL_SIZE * Board.SIZE + nameOffset, vertMargin + scoreOffsetTop + CELL_SIZE * (i + 1)),
                    (tanks[i].name == Board.me ? Color.White : Color.LightGray));
                spriteBatch.DrawString(scoreFonts, Convert.ToString(tanks[i].coins), new Vector2(leftMargin + CELL_SIZE * Board.SIZE + coinsOffset, vertMargin + scoreOffsetTop + CELL_SIZE * (i + 1)),
                    (tanks[i].name == Board.me ? Color.White : Color.LightGray));
                spriteBatch.DrawString(scoreFonts, Convert.ToString(tanks[i].points), new Vector2(leftMargin + CELL_SIZE * Board.SIZE + pointsOffset, vertMargin + scoreOffsetTop + CELL_SIZE * (i + 1)),
                    (tanks[i].name == Board.me ? Color.White : Color.LightGray));
                spriteBatch.DrawString(scoreFonts, Convert.ToString(tanks[i].life), new Vector2(leftMargin + CELL_SIZE * Board.SIZE + healthOffset, vertMargin + scoreOffsetTop + CELL_SIZE * (i + 1)),
                    (tanks[i].name == Board.me ? Color.White : Color.LightGray));
            }

            //show logo
            spriteBatch.Draw(logo, new Vector2(2 * leftMargin + CELL_SIZE * Board.SIZE + logoLeft, vertMargin + logoTop), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        //comparator for sorting tank list by name
        private static int CompareTanks(Tank t1, Tank t2)
        {
            return t1.name.CompareTo(t2.name);
        }

        public float rotate(int direction) //generate rotation return
        {
            float rotation = 0.0f;

            if (direction == NORTH)
            {
                rotation = 0.0f;
            }
            else if (direction == EAST)
            {
                rotation = MathHelper.PiOver2;
            }
            else if (direction == SOUTH)
            {
                rotation = MathHelper.Pi;
            }
            else if (direction == WEST)
            {
                rotation = -MathHelper.PiOver2;
            }

            return rotation;
        }
    }
}
