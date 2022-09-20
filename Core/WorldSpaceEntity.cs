using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public abstract class WorldSpaceEntity : Entity
    {
        protected Texture2D _sprite;

        protected WorldSpaceEntity() : base()
        {
        }
    }
}
