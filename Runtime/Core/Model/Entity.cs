using System;
using Core.Initialization;
using Core.Model.ModelSystems;
using Core.Systems;

namespace Core.Model
{

    public interface IEntity
    {
        public EntId ID { get; }

        public void Destroy();
    }

    public abstract class Entity : IEntity
    {
        public EntId ID { get; }

        protected Entity()
        {
            ID = EntitiesContainer.GenerateNewEntityID();
            EntitiesContainer.OnEntityCreated(this);
        }
    
        protected ref TComponentData GetComponent<TComponentData>() where TComponentData : struct, IComponentData
        {
            object componentContainer = DataContainersControllerImplementation.GetInstance().GetComponentContainer(typeof(TComponentData));
            return ref ((ComponentContainer<TComponentData>) componentContainer).GetComponent(ID);
        }
        
        protected TSystem GetSystem<TSystem>() 
        {
            return ObjectBuilder.GetInstance().Resolve<TSystem>();
        }
        
        public void Destroy()
        {
            EntitiesContainer.DestroyEntity(this.ID);
        }
    }
    
    
    [Serializable]
    public struct EntId : IEquatable<EntId>
    {
        /// <summary>
        /// This is an indicator of an invalid entity which has no entry in the WorldState.
        /// </summary>
        public static EntId Invalid = new(uint.MinValue);
    
        public readonly uint Id;
    
        public EntId(uint setId)
        {
            Id = setId;
        }
    
        public static bool operator ==(EntId obj1, EntId obj2)
        {
            return obj1.Id == obj2.Id;
        }
    
        public static bool operator !=(EntId obj1, EntId obj2) => !(obj1 == obj2);
    
        public bool Equals(EntId other)
        {
            return Id == other.Id;
        }
    
        public override bool Equals(object obj)
        {
            return obj is EntId otherEntId && otherEntId.Id == Id;
        }
    
        public override int GetHashCode()
        {
            return (int) Id;
        }
    
        public override string ToString()
        {
            if (IsValid())
            {
                return $"EntId({Id})";
            }
            else
            {
                return "EntId(Invalid)";
            }
        }
    
        public bool IsValid()
        {
            return Id != Invalid.Id;
        }
    }

}
