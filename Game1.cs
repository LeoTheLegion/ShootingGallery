using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShootingGallery
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D targetSprite, crosshairsSprite, backgroundSprite;
        private SpriteFont gameFont;

        private Vector2 targetPosition = new Vector2(300, 300);
        private const int targetRadius = 45;

        private MouseState mState;
        bool mReleased = true;
        private int score = 0;

        private const int crosshairRadius = 25;

        double timer = 10;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            targetSprite = Content.Load<Texture2D>("target");
            crosshairsSprite = Content.Load<Texture2D>("crosshairs");
            backgroundSprite = Content.Load<Texture2D>("sky");
            gameFont = Content.Load<SpriteFont>("galleryFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if(timer > 0)
            {
                timer -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            if(timer < 0)
            {
                timer = 0;
            } 

            mState = Mouse.GetState();

            if(mState.LeftButton == ButtonState.Pressed && mReleased == true)
            {
                float mouseTargetDist = Vector2.Distance(targetPosition, mState.Position.ToVector2());

                if(mouseTargetDist < targetRadius && timer > 0)
                {
                    score++;

                    Random rand = new Random();

                    targetPosition.X = rand.Next(0, _graphics.PreferredBackBufferWidth);
                    targetPosition.Y = rand.Next(0, _graphics.PreferredBackBufferHeight);
                }

                mReleased = false;
            }

            if(mState.LeftButton == ButtonState.Released)
            {
                mReleased = true;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();  
            
            _spriteBatch.Draw(backgroundSprite, new Vector2(0, 0), Color.White);

            _spriteBatch.DrawString(gameFont, "Score: " + score.ToString(), new Vector2(3, 3), Color.White);
            _spriteBatch.DrawString(gameFont, "Time: " + Math.Ceiling(timer).ToString(), new Vector2(3, 40), Color.White);

            if(timer > 0)
            {
                _spriteBatch.Draw(targetSprite,
                targetPosition - new Vector2(targetRadius, targetRadius)
                , Color.White);
            }

            _spriteBatch.Draw(crosshairsSprite, 
                mState.Position.ToVector2() - new Vector2(crosshairRadius,crosshairRadius),
                Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}