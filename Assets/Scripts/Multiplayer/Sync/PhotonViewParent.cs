using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Multiplayer
{
    //[RequireComponent(typeof(PhotonView))]
    public class PhotonViewParent : MonoBehaviour, IPunObservable
    {
#pragma warning disable 0649        
        [Header("References")]
        public List<GameObject> children;
#pragma warning restore 0649

        private List<IPunObservable> observedComponents = new List<IPunObservable>();

        private void Awake()
        {
            if (this.children != null)
            {
                foreach (var gameObject in this.children)
                {
                    if (TryFindComponentsThatImplement<IPunObservable>(gameObject, out var componentsImplementingIPunObservable))
                    {
                        this.observedComponents.AddRange(componentsImplementingIPunObservable.Where(o => o != (IPunObservable)this));
                    }
                }
            }
        }

        private bool TryFindComponentsThatImplement<TInterface>(GameObject gameObject, out IEnumerable<TInterface> componentsToLookFor) where TInterface : class
        {
            if (!gameObject)
            {
                componentsToLookFor = default;
                return false;
            }

            var components = gameObject.GetComponents<Component>();
            //componentsToLookFor = components.Where(c => c is TInterface && ((c as Behaviour)?.enabled ?? false)).Select(c => c as TInterface);
            componentsToLookFor = components.Where(c => c is TInterface).Select(c => c as TInterface);

            return componentsToLookFor.Count() > 0;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            foreach (var component in this.observedComponents)
            {
                component.OnPhotonSerializeView(stream, info);
            }
        }
    }
}