using System;
using Photon.Realtime;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Multiplayer
{
    public class MultiplayerSessionScene : MultiplayerGameObjectPunCallbacks
    {
        public delegate void MultiplayerSessionSceneReady();

        public static event MultiplayerSessionSceneReady OnMultiplayerSessionSceneReady;

        private Dictionary<int, MultiplayerGameObject> sceneMultiplayerGameObjects = new Dictionary<int, MultiplayerGameObject>();

        private Dictionary<int, DynamicMultiplayerGameObject> dynamicMultiplayerGameObjects = new Dictionary<int, DynamicMultiplayerGameObject>();

        private Action onSceneReady;

        private Dictionary<string, Transform> dynamicTransformRoots = new Dictionary<string, Transform>();
        private Dictionary<Int64, Transform> transformFromHash = new Dictionary<Int64, Transform>();
        private Dictionary<Transform, Int64> hashFromTransform = new Dictionary<Transform, Int64>();

        public static MultiplayerSessionScene Instance { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            MultiplayerGameObject.OnDestroyed += this.OnMultiplayerGameObjectDestroyed;

            CreateTransformHashes();

            MultiplayerSceneObjects multiplayerSceneObjects = FindObjectOfType<MultiplayerSceneObjects>();

            if (multiplayerSceneObjects != null)
            {
                foreach (var multiplayerGameObject in multiplayerSceneObjects.SceneMultiplayerGameObjects)
                {
                    this.RegisterMultiplayerGameObject(multiplayerGameObject);
                }
            }

            base.Awake();

            if (this.IsHost)
            {
                this.OnAllSynced();
            }
        }

        private void ScanTransforms(Transform transform, string rootPath)
        {
            string NameWithPath(GameObject gameObject, string divider = "/")
            {
                if (!gameObject)
                {
                    return String.Empty;
                }
                var name = gameObject.name;
                var parent = gameObject.transform.parent;
                while (parent)
                {
                    name = $"{parent.name}{divider}{name}";
                    parent = parent.parent;
                }

                return name;
            }

            void ScanRecursive(Transform t, string path)
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(path));

                    Int64 code0 = BitConverter.ToInt64(bytes, 0);
                    Int64 code1 = BitConverter.ToInt64(bytes, 8);
                    Int64 code2 = BitConverter.ToInt64(bytes, 16);
                    Int64 code3 = BitConverter.ToInt64(bytes, 24);
                    var hash = code0 ^ code1 ^ code2 ^ code3;

                    if (this.transformFromHash.ContainsKey(hash))
                    {
                        Debug.LogError($"HASHCLASH!! {NameWithPath(this.transformFromHash[hash].gameObject)} {path}");
                    }
                    this.transformFromHash[hash] = t;
                    this.hashFromTransform[t] = hash;
                     
                    for (int c = 0; c < t.childCount; ++c)
                    {
                        ScanRecursive(t.GetChild(c), $"{path}/{c}");
                    }
                }
            }

            ScanRecursive(transform, rootPath);
        }

        private void CreateTransformHashes()
        {
            this.transformFromHash.Clear();
            this.hashFromTransform.Clear();

            foreach (var t in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().OrderBy(go => go.name).Select(go => go.transform))
            {
                ScanTransforms(t, $"STATIC/{t.name}");
            }
        }

        public Transform TransformFromHash(Int64 hash)
        {
            return this.transformFromHash.ContainsKey(hash) ? this.transformFromHash[hash] : default;
        }

        public Int64 HashFromTransform(Transform transform)
        {
            return !transform ? 0 : (this.hashFromTransform.ContainsKey(transform) ? this.hashFromTransform[transform] : 0);
        }
        
        public void AddInstantiation(Transform transform, string id)
        {
            ScanTransforms(transform, rootPath: id);
            this.dynamicTransformRoots[id] = transform;
        }

        public void RemoveInstantiation(string id)
        {
            if (this.dynamicTransformRoots.ContainsKey(id))
            {
                RemoveInstantiation(this.dynamicTransformRoots[id]);
                this.dynamicTransformRoots.Remove(id);
            }
        }

        public void RemoveInstantiation(Transform transform)
        {
            if (this.hashFromTransform.ContainsKey(transform))
            {
                var hash = this.hashFromTransform[transform];
                this.hashFromTransform.Remove(transform);
                this.transformFromHash.Remove(hash);
            }

            for (int c = 0; c < transform.childCount; ++c)
            {
                RemoveInstantiation(transform.GetChild(c));
            }
        }

        public void RegisterMultiplayerGameObject(MultiplayerGameObject multiplayerGameObject)
        {
            this.sceneMultiplayerGameObjects[multiplayerGameObject.photonView.ViewID] = multiplayerGameObject;
        }

        public void DestroyAllDynamicMultiplayerGameObjects()
        {
            foreach (var dynamicMultiplayerGameObject in this.dynamicMultiplayerGameObjects)
            {
                Destroy(dynamicMultiplayerGameObject.Value.MultiplayerGameObject.gameObject);
            }

            this.dynamicMultiplayerGameObjects = new Dictionary<int, DynamicMultiplayerGameObject>();
        }

        public GameObject Instantiate(int viewID, string prefabName, Vector3 position, Quaternion rotation)
        {
            return InstantiateInternal(viewID, prefabName, position, rotation, false);
        }
        
        private GameObject InstantiateInternal(int viewID, string prefabName, Vector3 position, Quaternion rotation, bool isRemote = false)
        {
            var multiplayerGameObject = Instantiate(Resources.Load<MultiplayerGameObject>(prefabName), position, rotation);
            multiplayerGameObject.photonView.ViewID = viewID;
            multiplayerGameObject.OnViewIDSet();

            multiplayerGameObject.name = prefabName;

            var dynamicMultiplayerGameObject = new DynamicMultiplayerGameObject(prefabName, multiplayerGameObject);

            this.dynamicMultiplayerGameObjects.Add(viewID, dynamicMultiplayerGameObject);

            if (isRemote == false)
            {
                this.photonView.RPC(nameof(RpcInstantiate), RpcTarget.Others, viewID, prefabName, position, rotation);
            }

            return multiplayerGameObject.gameObject;
        }

        [PunRPC]
        protected void RpcInstantiate(int viewID, string prefabName, Vector3 position, Quaternion rotation)
        {
            //this.InstantiateInternal(viewID, prefabName, position, rotation, true);
        }

        protected virtual void OnAllSynced()
        {
            OnMultiplayerSessionSceneReady?.Invoke();
        }

        public override void SendFullState(RpcTarget target)
        {
            base.SendFullState(target);

            foreach (var multiplayerGameObject in this.sceneMultiplayerGameObjects)
            {
                multiplayerGameObject.Value.SendFullState(target);
            }

            foreach (var multiplayerGameObject in this.dynamicMultiplayerGameObjects)
            {
                multiplayerGameObject.Value.MultiplayerGameObject.SendFullState(target);
            }

            this.photonView.RPC(nameof(this.RpcOnAllSynced), target);
        }

        public override void SendFullState(Player player)
        {
            base.SendFullState(player);

            foreach (var multiplayerGameObject in this.sceneMultiplayerGameObjects)
            {
                multiplayerGameObject.Value.SendFullState(player);
            }

            foreach (var multiplayerGameObject in this.dynamicMultiplayerGameObjects)
            {
                multiplayerGameObject.Value.MultiplayerGameObject.SendFullState(player);
            }

            this.photonView.RPC(nameof(this.RpcOnAllSynced), player);
        }

        protected override void FullStateSync(bool isWriting, Queue<object> data)
        {
            if (isWriting)
            {
                var dynamicMultiplayerGameObjectViewIDs = new List<int>();
                var dynamicMultiplayerGameObjectPrefabs = new List<string>();
                var dynamicMultiplayerGameObjectPositions = new List<Vector3>();
                var dynamicMultiplayerGameObjectRotations = new List<Quaternion>();

                foreach (var dynamicMultiplayerGameObject in this.dynamicMultiplayerGameObjects)
                {
                    dynamicMultiplayerGameObjectViewIDs.Add(dynamicMultiplayerGameObject.Key);
                    dynamicMultiplayerGameObjectPrefabs.Add(dynamicMultiplayerGameObject.Value.PrefabName);
                    dynamicMultiplayerGameObjectPositions.Add(dynamicMultiplayerGameObject.Value.MultiplayerGameObject.transform.position);
                    dynamicMultiplayerGameObjectRotations.Add(dynamicMultiplayerGameObject.Value.MultiplayerGameObject.transform.rotation);
                }

                data.Enqueue(dynamicMultiplayerGameObjectViewIDs.ToArray());
                data.Enqueue(dynamicMultiplayerGameObjectPrefabs.ToArray());
                data.Enqueue(dynamicMultiplayerGameObjectPositions.ToArray());
                data.Enqueue(dynamicMultiplayerGameObjectRotations.ToArray());
            }
            else
            {
                var dynamicMultiplayerGameObjectViewIDs = data.Dequeue<int[]>();
                var dynamicMultiplayerGameObjectPrefabs = data.Dequeue<string[]>();
                var dynamicMultiplayerGameObjectPositions = data.Dequeue<Vector3[]>();
                var dynamicMultiplayerGameObjectRotations = data.Dequeue<Quaternion[]>();

                for (int i = 0; i < dynamicMultiplayerGameObjectViewIDs.Length; i++)
                {
                    this.InstantiateInternal(dynamicMultiplayerGameObjectViewIDs[i], dynamicMultiplayerGameObjectPrefabs[i], dynamicMultiplayerGameObjectPositions[i], dynamicMultiplayerGameObjectRotations[i], true);
                }
            }
        }

        [PunRPC]
        protected void RpcOnAllSynced()
        {
            this.OnAllSynced();
        }

        private class DynamicMultiplayerGameObject
        {
            public readonly string PrefabName;
            public readonly MultiplayerGameObject MultiplayerGameObject;

            public DynamicMultiplayerGameObject(string prefabName, MultiplayerGameObject multiplayerGameObject)
            {
                this.PrefabName = prefabName;
                this.MultiplayerGameObject = multiplayerGameObject;
            }
        }

        private void OnMultiplayerGameObjectDestroyed(MultiplayerGameObject multiplayerGameObject)
        {
            var viewID = multiplayerGameObject.photonView.ViewID;

            if (this.sceneMultiplayerGameObjects.ContainsKey(viewID))
            {
                this.sceneMultiplayerGameObjects.Remove(viewID);
            }

            if (this.dynamicMultiplayerGameObjects.ContainsKey(viewID))
            {
                this.dynamicMultiplayerGameObjects.Remove(viewID);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            MultiplayerGameObject.OnDestroyed -= this.OnMultiplayerGameObjectDestroyed;

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
