using System;
using System.Reflection;
using Core.Systems;
using Core.Utils.Reflection;

namespace Core.Model.ModelSystems.ComponentSystems
{
	


    public sealed class UpdateComponentSystemCache : BaseSystemCache
    {
        public readonly MethodInfo CachedUpdateMethod;
        public readonly SystemPriority SystemUpdatePriority = SystemPriority.Default;
        
        private static readonly Type UPDATE_SYSTEM_TYPE = typeof(UpdateComponents<>); 
        
        private UpdateComponentSystemCache(object system, Type desiredComponentType) : 
            base(system)
        {
            Type systemType = system.GetType();

            Attribute[] attributes = Attribute.GetCustomAttributes(systemType);
            UpdateComponentPropertiesAttribute systemProperties = GetAttributesOfType<UpdateComponentPropertiesAttribute>(attributes);
            if (systemProperties != null)
            {
                SystemUpdatePriority = systemProperties.Priority;
            }
            
            Type generic = UPDATE_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            CachedUpdateMethod = generic.GetMethod(nameof(UpdateComponents<MockComponentData>.UpdateComponents));
        }
        
        internal static UpdateComponentSystemCache CreateIfPossible(object system, Type desiredComponentType)
        {
            Type systemType = system.GetType();
            if (!systemType.IsAssignableToGenericType(UPDATE_SYSTEM_TYPE)) return null;

            Type fullGeneric = UPDATE_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            if (!systemType.ImplementsGenericFullTypeDefinition(fullGeneric)) return null;

            return new UpdateComponentSystemCache(system, desiredComponentType);
        }
        
        internal void Call(object[] args)
        {
            
#if !UNITY_EDITOR
            try
            {
#endif   
            CachedUpdateMethod?.Invoke(System, args);
            
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError($"Error in a componentSystem({system.GetType()}) .UpdateComponents:\n {e.GetType()}");
                Debug.LogException(e);
            }
#endif            
        }
    }

    public sealed class OnCreateComponentSystemCache : BaseSystemCache
    {
        private static readonly Type ON_CREATE_SYSTEM_TYPE = typeof(OnCreateComponent<>);

        public readonly MethodInfo CachedOnCreateMethod;
        public readonly SystemPriority SystemLifetimePriority = SystemPriority.Default;

        private OnCreateComponentSystemCache(object system, Type desiredComponentType) : 
            base(system)
        {
            Type systemType = system.GetType();

            Attribute[] attributes = Attribute.GetCustomAttributes(systemType);
            OnCreateComponentPropertiesAttribute systemProperties = GetAttributesOfType<OnCreateComponentPropertiesAttribute>(attributes);
            if (systemProperties != null)
            {
                SystemLifetimePriority = systemProperties.Priority;
            }
            
            Type generic = ON_CREATE_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            
            CachedOnCreateMethod = generic.GetMethod(nameof(OnCreateComponent<MockComponentData>.OnCreateComponent));
        }
        
        internal static OnCreateComponentSystemCache CreateIfPossible(object system, Type desiredComponentType)
        {
            Type systemType = system.GetType();
            if (!systemType.IsAssignableToGenericType(ON_CREATE_SYSTEM_TYPE)) return null;

            Type fullGeneric = ON_CREATE_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            if (!systemType.ImplementsGenericFullTypeDefinition(fullGeneric)) return null;

            return new OnCreateComponentSystemCache(system, desiredComponentType);
        }
        
        internal void Call(object[] args)
        {
            
#if !UNITY_EDITOR
            try
            {
#endif   
            CachedOnCreateMethod?.Invoke(System, args);
            
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError($"Error in a system({system.GetType()}) .OnCreateComponent:\n {e.GetType()}");
                Debug.LogException(e);
            }
#endif            
        }
    }

    public sealed class OnDestroyComponentSystemCache : BaseSystemCache
    {
        private static readonly Type DESTROY_SYSTEM_TYPE = typeof(OnDestroyComponent<>);

        public readonly MethodInfo CachedOnDestroyedMethod;
        public readonly SystemPriority SystemLifetimePriority = SystemPriority.Default;

        private OnDestroyComponentSystemCache(object system, Type desiredComponentType) : 
            base(system)
        {
            Type systemType = system.GetType();

            Attribute[] attributes = Attribute.GetCustomAttributes(systemType);
            OnDestroyComponentPropertiesAttribute systemProperties = GetAttributesOfType<OnDestroyComponentPropertiesAttribute>(attributes);
            if (systemProperties != null)
            {
                SystemLifetimePriority = systemProperties.Priority;
            }
            
            Type generic = DESTROY_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            
            CachedOnDestroyedMethod = generic.GetMethod(nameof(OnDestroyComponent<MockComponentData>.OnDestroyComponent));
        }
        
        internal static OnDestroyComponentSystemCache CreateIfPossible(object system, Type desiredComponentType)
        {
            Type systemType = system.GetType();
            if (!systemType.IsAssignableToGenericType(DESTROY_SYSTEM_TYPE)) return null;

            Type fullGeneric = DESTROY_SYSTEM_TYPE.MakeGenericType( desiredComponentType );
            if (!systemType.ImplementsGenericFullTypeDefinition(fullGeneric)) return null;

            return new OnDestroyComponentSystemCache(system, desiredComponentType);
        }
        
        internal void Call(object[] args)
        {
            
#if !UNITY_EDITOR
            try
            {
#endif   
            CachedOnDestroyedMethod?.Invoke(System, args);
            
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError($"Error in a system({system.GetType()}) .OnDestroyComponent:\n {e.GetType()}");
                Debug.LogException(e);
            }
#endif            
        }
    }

    
    
    public class BaseSystemCache
    {
        public readonly object System;
        public readonly Type CachedType;
        
        protected BaseSystemCache(object system)
        {
            Type systemType = system.GetType();

            System = system;
            CachedType = systemType;
        }
        
        protected static T GetAttributesOfType<T>(Attribute[] attributes) where T : Attribute
        {
            foreach (Attribute attribute in attributes)
            {
                if (attribute.GetType() == typeof(T))
                {
                    return attribute as T;
                }
            }

            return default;
        }

    }
	
}