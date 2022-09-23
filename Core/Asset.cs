using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public abstract class Asset
    {
        public abstract void LoadContent(ContentManager _content);
    }
}
