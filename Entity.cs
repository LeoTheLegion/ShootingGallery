using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery
{
    public abstract class Entity
    {
        public abstract void LoadContent(ContentManager content);
        public abstract void Update(ref GameTime gameTime);
        public abstract void Render(ref SpriteBatch _spriteBatch);
        public abstract void Delete();

    }
}
