using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public static class EntityManagementSystem
    {
        private static List<Entity> _entities = new List<Entity>();

        public static void Register(Entity e)
        {
            _entities.Add(e);
        }
        public static void Unregister(Entity e)
        {
            _entities.Remove(e);
        }

        public static void Update(ref GameTime _gameTime)
        {
            _entities.Sort(
                (x, y) => x.GetSort().CompareTo(y.GetSort())
                );

            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].GetActive())
                    _entities[i].Update(ref _gameTime);
            }
        }

        public static void Render(ref SpriteBatch _spriteBatch)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].GetActive())
                    _entities[i].Render(ref _spriteBatch);
            }
        }


    }
}
