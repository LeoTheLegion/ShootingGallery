using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public class EntityGroup
    {
        private List<Entity> _entities;

        public EntityGroup(List<Entity> entities)
        {
            _entities = entities;
        }

        public EntityGroup(Entity[] entities)
        {
            this._entities = new List<Entity>(entities);
        }

        public void Add(Entity e)
        {
            this._entities.Add(e);
        }

        public void Remove(Entity e)
        {
            this._entities.Remove(e);
        }

        public void SetActive(bool active)
        {
            foreach (var e in this._entities)
            {
                e.SetActive(active);
            }
        }

        public void Destroy()
        {
            foreach (var e in this._entities)
            {
                e.Destroy();
            }
        }
    }
}
