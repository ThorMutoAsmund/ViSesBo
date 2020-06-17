using Photon.Pun;
using UnityEngine;

namespace Multiplayer
{
    public static class GameObjectExtension
    {
        public static T InstantiateWithView<T>(int viewID) where T : Component
        {
            var instance = new GameObject(typeof(T).Name);

            var view = instance.AddComponent<PhotonView>();
            view.ViewID = viewID;

            return instance.AddComponent<T>();
        }

        /// <summary>
        /// Create a GameObject and attach an instance of the given component type to it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dontDestroyOnLoad">Set this true to mark this component as not to be destroyed on load</param>
        /// <returns></returns>
        public static T Instantiate<T>(bool dontDestroyOnLoad = false) where T : Component
        {
            var instance = new GameObject(typeof(T).Name);

            if (dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(instance);
            }

            return instance.AddComponent<T>();
        }
    }
}
