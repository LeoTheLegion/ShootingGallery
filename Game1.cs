using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;

namespace ShootingGallery
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        
        private List<Entity> _entities;

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
            Entity _target, _crossHair, _background;

             _target = new Target(new Vector2(300, 300));
            _target.SetSort(1);

            _crossHair = new Crosshair();
            _crossHair.SetSort(2);

            _background = new Decorative("sky", new Vector2(0, 0));
            _background.SetSort(-1);

            Text _score, _timer;

            _score = new Text("galleryFont", "Score: Null", new Vector2(3, 3));
            _score.SetSort(0);

            _timer = new Text("galleryFont", "Time: Null", new Vector2(3, 40));
            _timer.SetSort(0);

            _entities = new List<Entity>();
            _entities.Add(_target);
            _entities.Add(_crossHair);
            _entities.Add(_background);
            _entities.Add(_score);
            _entities.Add(_timer);

            GameManager.Init(ref _score, ref _timer, ref _target);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].LoadContent(Content);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _entities.Sort(
                (x, y) => x.GetSort().CompareTo(y.GetSort())
                );

            GameManager.Update(ref gameTime);

            for (int i = 0; i < _entities.Count; i++)
            {
                if(_entities[i].GetActive())
                    _entities[i].Update(ref gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].GetActive())
                    _entities[i].Render(ref _spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}