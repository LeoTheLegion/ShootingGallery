using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace ShootingGallery
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static Game1 instance;
        public static int WIDTH => instance._graphics.PreferredBackBufferWidth;
        public static int HEIGHT => instance._graphics.PreferredBackBufferHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            instance = this;
        }

        protected override void Initialize()
        {
            Resources.Init(new Dictionary<string, Asset>(){
                {"crosshairs", new Sprite("crosshairs") },
                {"sky", new Sprite("sky") },
                {"target", new Sprite("target") },
                {"button", new Sprite("button") },
                {"button_hover", new Sprite("button_hover") },
                {"galleryFont", new Font("galleryFont") }
            });
            // TODO: Add your initialization logic here
            Entity _target, _crossHair, _background;

             _target = new Target(new Vector2(300, 300))
                .SetSort(1);

            _crossHair = new Crosshair()
                .SetSort(3);

            _background = new Decorative("sky", new Vector2(0, 0))
                .SetSort(-1);

            Text _score, _timer, _gameoverMessage,_startMessage;
            Button _restartButton, _startButton;

            _score = new Text("galleryFont", "Score: Null", new Vector2(3, 3));
            _score.SetSort(0);

            _timer = new Text("galleryFont", "Time: Null", new Vector2(3, 40));
            _timer.SetSort(0);

            _gameoverMessage = new Text("galleryFont", "Game Over - Press Button To Play Again", new Vector2(120, 150));
            _startMessage = new Text("galleryFont", "Welcome to Michael's Shooting Gallery!", new Vector2(120, 150));

            _restartButton = new Button("button", "button_hover", "galleryFont", new Vector2(300, 250), "Restart");
            _startButton = new Button("button", "button_hover", "galleryFont", new Vector2(300, 250), "Play");

            GameManager.Init(ref _score, ref _timer,
                ref _gameoverMessage,ref _restartButton,
                ref _startMessage, ref _startButton,
                ref _target);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Resources.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            EntityManagementSystem.Update(ref gameTime);

            GameManager.Update(ref gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            EntityManagementSystem.Render(ref _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}