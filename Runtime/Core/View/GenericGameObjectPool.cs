using System;
using System.Collections.Generic;
using Core.Initialization;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Object = System.Object;

#nullable enable

namespace Core.View
{
    public sealed class GenericGameObjectPool : MonoBehaviour
    {
        [Inject] private ObjectBuilder objectBuilder = null!;
        
        public int instantiatedObjects = 0;

        private readonly HashSet<GameObject> managedGameObjects = new HashSet<GameObject>();
        public int ManagedGameObjectsCount => managedGameObjects.Count;

        private readonly Dictionary<GameObject, GameObject> prefabsFromObjects = new Dictionary<GameObject, GameObject>();
        public int PrefabsFromObjectsCount => prefabsFromObjects.Count;

        private readonly Dictionary<GameObject, List<GameObject>> gameobjectPools = new Dictionary<GameObject, List<GameObject>>();

        public int GameobjectPoolsCount
        {
            get
            {
                int cnt = 0;
                foreach (List<GameObject> listOfObject in gameobjectPools.Values)
                {
                    cnt += listOfObject.Count;
                }

                return cnt;
            }
        }

        private readonly Dictionary<GameObject, GameobjectProperties> propertiesbyGameObject = new();
        public int PropertiesbyGameObjectCount => propertiesbyGameObject.Count;


        private sealed class GameobjectProperties
        {
            public readonly Dictionary<Type, Object?> ComponentsCache = new();
            public readonly Dictionary<Type, Object[]?> MultipleComponentsCache = new();

            public IViewController? viewController;
        }
        
        public GameObject? GetGameObjectFromPrefab(GameObject prefab, Transform parent = null)
        {
            GameObject objectFromPrefab = GetOrCreateGameobject(prefab, true,  parent);
            return objectFromPrefab;
        }

        public GameObject? GetGameDeactivatedObjectFromPrefab(GameObject prefab, Transform parent = null)
        {
            GameObject objectFromPrefab = GetOrCreateGameobject(prefab, false, parent);
            return objectFromPrefab;
        }

        
        public void DestroyGameObject(GameObject targetGameObject)
        {
            if(targetGameObject == null)
            {
                return;
            }
            
            if (managedGameObjects.Contains(targetGameObject))
            {
                GameObject prefab = prefabsFromObjects[targetGameObject];
                ReturnObjectToPool(targetGameObject, prefab);
            }
            else
            {
                //Should we also remove it from prefabsFromObjects ?
                propertiesbyGameObject.Remove(targetGameObject);
                GameObject.Destroy(targetGameObject);
            }
        }
        public void Preload(GameObject prefab, int howMany, Transform parent = null)
        {
            using CachedList<UniTask> loadingTasks = ListCache<UniTask>.Get();
            List<GameObject> poolForPrefab = GetPoolForPrefab(prefab);
            int missingInstancesCount = howMany - poolForPrefab.Count;
            for (int i = 0; i < missingInstancesCount; i++)
            {
                instantiatedObjects++;
                GameObject newGameobject = objectBuilder.Instantiate(prefab, parent);
                DeactivateGameObject(newGameobject);
                poolForPrefab.Add(newGameobject);
            }
        }


        private void ReturnObjectToPool(GameObject targetGameObject, GameObject prefab)
        {
            List<GameObject> poolForPrefab = GetPoolForPrefab(prefab);
            poolForPrefab.Add(targetGameObject);
            targetGameObject.transform.SetParent(transform);
            managedGameObjects.Remove(targetGameObject);
            prefabsFromObjects.Remove(targetGameObject);

            DeactivateGameObject(targetGameObject);
        }

        private GameObject GetOrCreateGameobject(GameObject prefab, bool activate, Transform parent = null)
        {
            List<GameObject> pool = GetPoolForPrefab(prefab);

            if (pool.Count > 0)
            {
                GameObject pooledGameobject = pool[0];
                pool.RemoveAt(0);

                if (pooledGameobject != null)
                {
                    managedGameObjects.Add(pooledGameobject);
                    prefabsFromObjects.Add(pooledGameobject, prefab);
                    pooledGameobject.transform.SetParent(parent);


                    if (activate)
                    {
                        ActivateGameObject(pooledGameobject);
                    }

                    return pooledGameobject;
                }
                else
                {
                    Debug.LogError($"GenericPool: pooled object was null {prefab.gameObject.name}");
                }
                
            }
            
            
            instantiatedObjects++;
            GameObject newGameobject = objectBuilder.Instantiate(prefab, parent != null? parent : transform);
            managedGameObjects.Add(newGameobject);
            prefabsFromObjects.Add(newGameobject, prefab);
            AddPropertiesFor(newGameobject);
            if (!activate)
            {
                DeactivateGameObject(newGameobject);
            }
            return newGameobject;
        }

        private void AddPropertiesFor(GameObject newGameobject)
        {
            GameobjectProperties gameobjectProperties = new GameobjectProperties
            {
                viewController = newGameobject.GetComponent<IViewController>(),
            };

            propertiesbyGameObject.Add(newGameobject, gameobjectProperties);
        }
        

        public T? GetComponent<T>(GameObject gameObject) where T : class
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);
        
            Type componentType = typeof(T);
            if (gameobjectProperties.ComponentsCache.TryGetValue(componentType, out var value))
            {
                return value as T;
            }
            
            T newComponent = gameObject.GetComponent<T>();
            if (newComponent == null)
            {
                newComponent = gameObject.GetComponentInChildren<T>();
            }
            if (newComponent == null)
            {
                Debug.LogError($"component {componentType} not found in {gameObject.name} in generic pool");
                return null;
            }

            gameobjectProperties.ComponentsCache.Add(componentType, newComponent);
            return newComponent;
        }
        
        public T[]? GetComponents<T>(GameObject gameObject) where T : class
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);

            Type componentType = typeof(T);
            if (gameobjectProperties.MultipleComponentsCache.TryGetValue(componentType, out var value))
            {
                return value as T[];
            }
            
            T[] components = gameObject.GetComponents<T>();
            if (components == null || components.Length == 0)
            {
                components = gameObject.GetComponentsInChildren<T>();
            }
            if (components == null || components.Length == 0)
            {
                Debug.LogError($"components {componentType} not found in {gameObject.name} in generic pool");
                return null;
            }
            
            gameobjectProperties.MultipleComponentsCache.Add(componentType, components);
            return components as T[];
        }

        public T? GetComponentInChildren<T>(GameObject gameObject) where T : class
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);

            Type componentType = typeof(T);
            if (gameobjectProperties.ComponentsCache.TryGetValue(componentType, out var value))
            {
                return value as T;
            }

            T component = gameObject.GetComponentInChildren<T>();
            gameobjectProperties.ComponentsCache.Add(componentType, component);
            return component;
        }

        public T[]? GetComponentsInChildren<T>(GameObject gameObject) where T : class
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);

            Type componentType = typeof(T);
            if (gameobjectProperties.MultipleComponentsCache.TryGetValue(componentType, out var value))
            {
                return value as T[];
            }
            
            T[]? components = gameObject.GetComponentsInChildren<T>();
            gameobjectProperties.MultipleComponentsCache.Add(componentType, components);
            return components as T[];
        }

        

        private List<GameObject> GetPoolForPrefab(GameObject prefab)
        {
            List<GameObject> pool;
            if (!gameobjectPools.ContainsKey(prefab))
            {
                pool = new List<GameObject>();
                gameobjectPools.Add(prefab, pool);
            }
            else
            {
                pool = gameobjectPools[prefab];
            }

            return pool;
        }
        
        
        private void DeactivateGameObject(GameObject gameObject)
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);
            IViewController? viewController = gameobjectProperties.viewController;
            if (viewController != null)
            {
                viewController.DeactivateGameObject();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private GameobjectProperties GetGameobjectProperties(GameObject gameObject)
        {
            if (!propertiesbyGameObject.ContainsKey(gameObject))
            {
                AddPropertiesFor(gameObject);
            }
            return propertiesbyGameObject[gameObject];
        }

        private void ActivateGameObject(GameObject gameObject)
        {
            GameobjectProperties gameobjectProperties = GetGameobjectProperties(gameObject);
            if (gameobjectProperties.viewController != null)
            {
                gameobjectProperties.viewController.ActivateGameObject();
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void ClearPools()
        {
            foreach ((GameObject prefab, List<GameObject> pool) in gameobjectPools)
            {
                foreach (GameObject managedGameObject in pool)
                {
                    propertiesbyGameObject.Remove(managedGameObject);
                    GameObject.Destroy(managedGameObject);
                    instantiatedObjects--;
                }
            }
            gameobjectPools.Clear();
        }

        public void ClearAll()
        {
            ClearPools();

            foreach (GameObject managedGameObject in managedGameObjects)
            {
                propertiesbyGameObject.Remove(managedGameObject);
                prefabsFromObjects.Remove(managedGameObject);
                GameObject.Destroy(managedGameObject);
                instantiatedObjects--;
            }

            managedGameObjects.Clear();
        }
    }
}
