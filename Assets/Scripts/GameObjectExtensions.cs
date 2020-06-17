using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VSB
{
    public static class GameObjectExtensions
    {

        /// <summary>
        /// Add a component with the given type T and call initialize before the component gets activated
        /// Activation happens when the disabled gameobject is made active again
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="initialize"></param>
        /// <returns></returns>
        public static T AddComponent<T>(this GameObject gameObject, Action<T> initialize) where T : Component
        {
            gameObject.SetActive(false);
            var component = gameObject.AddComponent<T>();
            initialize?.Invoke(component);
            gameObject.SetActive(true);
            return component;
        }
    }
}
