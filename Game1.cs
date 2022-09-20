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

        private Entity _target,_crossHair,_background;
        private SpriteFont gameFont;

        public static Game1 instance;
        public static int WIDTH => instance._graphics.PreferredBackBufferWidth;
        public static int HEIGHT => instance._graphics.PreferredBackBufferHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            instance = this;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _target = new Target(new Vector2(300, 300));
            _crossHair = new Crosshair();
            _background = new Decorative("sky", new Vector2(0, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            _target.LoadContent(Content);
            _crossHair.LoadContent(Content);
            _background.LoadContent(Content);

            gameFont = Content.Load<SpriteFont>("galleryFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _crossHair.Update(ref gameTime);
            _background.Update(ref gameTime);

            if (!GameManager.isGameOver)
            {
                GameManager.ReduceGameTime(gameTime.ElapsedGameTime.TotalSeconds);

                _target.Update(ref gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _background.Render(ref _spriteBatch);

            _spriteBatch.DrawString(gameFont, "Score: " + GameManager.GetScore().ToString(), new Vector2(3, 3), Color.White);
            _spriteBatch.DrawString(gameFont, "Time: " + Math.Ceiling(GameManager.GetGameTime()).ToString(), new Vector2(3, 40), Color.White);

            if(!GameManager.isGameOver)
            {
                _target.Render(ref _spriteBatch);
            }

            _crossHair.Render(ref _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}