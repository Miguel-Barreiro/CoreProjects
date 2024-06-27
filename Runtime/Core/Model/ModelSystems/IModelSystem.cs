using System;
using Core.Systems;

namespace Core.Model
{
    public interface IModelSystem<T> : ISystem where T : IEntity 
    {
        public void OnNew(T newEnt);
        public void OnDestroy(T destroyedEnt);
        public void Update(T entity, float deltaTime);
        
        public Type EntityType { get; }

    }
}