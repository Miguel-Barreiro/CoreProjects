using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Initialization
{
    public class ObjectBuilder
    {
        [Inject] private DiContainer diContainer;

        /// <summary>
        /// this will return all instances of the given type that are registered in the container ( T can be an interface or a class )
        /// </summary>
        /// <param name="result"> the result will be added here </param>
        /// <typeparam name="T"></typeparam>
        public void ResolveAll<T>(List<T> result)
        {
            result.AddRange(diContainer.ResolveAll<T>());
        }

        public void Inject(object target)
        {
            diContainer.Inject(target);
        }

        /// <summary>
        /// this will instantiate a gameobject from the given prefab and will inject all the necessary
        /// instances
        /// </summary>
        /// <param name="prefab"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Instantiate<T>(T prefab) where T : MonoBehaviour
        {
            GameObject newGameObject = diContainer.InstantiatePrefab(prefab);
            return newGameObject.GetComponent<T>();
        }
        /// <summary>
        /// this will instantiate a gameobject from the given prefab and will inject all the necessary
        /// instances
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Instantiate<T>(T prefab, Transform parent) where T : MonoBehaviour
        {
            GameObject newGameObject = diContainer.InstantiatePrefab(prefab, parent);
            return newGameObject.GetComponent<T>();
        }
        
        /// <summary>
        /// this will instantiate a gameobject from the given prefab and will inject all the necessary
        /// instances
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject prefab)
        {
            return diContainer.InstantiatePrefab(prefab);
        }
        /// <summary>
        /// this will instantiate a gameobject from the given prefab and will inject all the necessary
        /// instances
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return diContainer.InstantiatePrefab(prefab, parent);
        }
    }
}