﻿using System;

namespace Core.Model
{

    public interface IEntity
    {
        public EntId ID { get; }
    }

    public abstract class BaseEntity : IEntity
    {
        public EntId ID => id;
        private EntId id;

        protected BaseEntity()
        {
            id = EntityLifetimeManager.GenerateNewEntityID();
            EntityLifetimeManager.OnEntityCreated(this);
        }

        public void Destroy()
        {
            EntityLifetimeManager.OnDestroyEntity(this);
        }
    }
    
    [Serializable]
    public struct EntId : IEquatable<EntId>
    {
        /// <summary>
        /// This is an indicator of an invalid entity which has no entry in the WorldState.
        /// </summary>
        static public EntId Invalid = new(0);

        public readonly int Id;

        public EntId(int setId)
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
            return Id;
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
