using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public abstract class Entity
    {
        protected Vector2 _position;
        protected int sort = -1;
        protected bool _active;

        protected Entity()
        {
            _active = true;
            EntityManagementSystem.Register(this);
        }

        public abstract void Update(ref GameTime gameTime);
        public abstract void Render(ref SpriteBatch _spriteBatch);
        public virtual void Destroy()
        {
            EntityManagementSystem.Unregister(this);
        }

        public virtual Entity SetSort(int x) {
            sort = x;
            return this;
        }

        public virtual int GetSort() { return sort; } 
        public virtual void SetActive(bool active) => _active = active;
        public virtual bool GetActive() { return _active; }
    }
}
