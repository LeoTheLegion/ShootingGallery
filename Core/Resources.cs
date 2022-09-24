using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public static class Resources
    {
        private static Dictionary<string, Asset> _assets;

        public static void Init(Dictionary<string, Asset> dictionary)
        {
            _assets = dictionary;
        }

        public static void LoadContent(ContentManager _content)
        {
            foreach(var key in _assets.Keys)
            {
                _assets[key].LoadContent(_content);
            }
        }

        public static Asset Load(string t) {
            return _assets[t]; 
        }

        
    }
}
