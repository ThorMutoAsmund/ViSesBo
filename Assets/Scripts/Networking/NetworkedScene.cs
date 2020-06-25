using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public enum ViewIdAllocationMethod
    {
        Specific,
        Static,
        Local,
        Scene
    };

    [RequireComponent(typeof(PhotonView))]
    public abstract class NetworkedScene : MonoBehaviourPunCallbacks
    {
        public class SpawnedObject : MonoBehaviourPun
        {
            public string ResourceName { get; set; }
            public Vector3 Position => this.transform.position;
            public Quaternion Rotation => this.transform.rotation;
            public int ViewID => this.photonView?.ViewID ?? 0;
        }

        public static NetworkedScene NetworkSceneInstance { get; private set; }

        private Dictionary<string, Transform> dynamicTransformRoots = new Dictionary<string, Transform>();
        private Dictionary<Int64, Transform> transformFromHash = new Dictionary<Int64, Transform>();
        private Dictionary<Transform, Int64> hashFromTransform = new Dictionary<Transform, Int64>();

        protected virtual void Awake()
        {
            NetworkSceneInstance = this;

            var view = this.gameObject.AddComponent<PhotonView>();
            view.ViewID = NetworkManager.NetworkedSceneViewId;

            CreateTransformHashes();

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDestroy()
        {
            if (NetworkSceneInstance == this)
            {
                NetworkSceneInstance = null;
            }
            
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            {
                // Report "loaded" to master
                NetworkManager.Instance.ReportSceneLoaded();
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                //PhotonNetwork.IsMessageQueueRunning = true;
            }
        }

        protected virtual void Start()
        {
            NetworkManager.Instance.WhenJoinedRoom(this, () =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // Start receiving messages
                    PhotonNetwork.SetInterestGroups(1, true);
                }

                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (!player.IsLocal)
                    {
                        SpawnPlayer(player);
                    }
                    else
                    {
                        PhotonPlayerAfterRoomJoined(player);
                    }
                }
            });

            // Create local player
            SpawnPlayer();
        }

        protected abstract void SpawnPlayer(Player newPlayer = null);
        protected virtual void PhotonPlayerAfterRoomJoined(Player newPlayer) { }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            // Create the joining player locally
            SpawnPlayer(newPlayer); // TBD hide until reports "ready"

            if (PhotonNetwork.IsMasterClient)
            {
                // Force joining player to load scene
                NetworkManager.Instance.ForceLoadScene(newPlayer, SceneManager.GetActiveScene().name);
            }
        }

        [PunRPC]
        public (GameObject gameObject, int viewId) RpcInstantiate(string prefabName, Vector3 position, Quaternion rotation, ViewIdAllocationMethod method, int existingViewId)
        {            
            var gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(prefabName), position, rotation);
            gameObject.AddComponent<SpawnedObject>().ResourceName = prefabName;

            if (method != ViewIdAllocationMethod.Static)
            {
                var photonView = gameObject.GetComponent<PhotonView>();
                if (photonView)
                {
                    switch (method)
                    {
                        case ViewIdAllocationMethod.Specific:
                            photonView.ViewID = existingViewId;
                            break;
                        case ViewIdAllocationMethod.Local:
                            PhotonNetwork.AllocateViewID(photonView);
                            existingViewId = photonView.ViewID;
                            break;
                        case ViewIdAllocationMethod.Scene:
                            PhotonNetwork.AllocateSceneViewID(photonView);
                            existingViewId = photonView.ViewID;
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("== Instantiated object has no PhotonView and will not be networked.");
                }

                var networkedObject = gameObject.GetComponent<INetworkedObject>();
                if (!(networkedObject == null || networkedObject.Equals(null)))
                {
                    NetworkManager.Register(networkedObject);
                }
            }

            return (gameObject, existingViewId);
        }

        [PunRPC]
        public void RpcInstantiateOnMaster(string prefabName, Vector3 position, Quaternion rotation)
        {
            var (gameObject, viewId) = RpcInstantiate(prefabName, position, rotation, ViewIdAllocationMethod.Scene, 0);
            this.photonView.RPC(nameof(RpcInstantiate), RpcTarget.Others, prefabName, position, rotation, ViewIdAllocationMethod.Specific, viewId);
        }

        public void InstantiateSceneObject(string prefabName, Vector3? position = null, Quaternion? rotation = null)
        {
            var actualPosition = position ?? Vector3.zero;
            var actualRotation = rotation ?? Quaternion.identity;

            if (PhotonNetwork.IsMasterClient)
            {
                RpcInstantiateOnMaster(prefabName, actualPosition, actualRotation);
            }
            else
            {
                this.photonView.RPC(nameof(RpcInstantiateOnMaster), RpcTarget.MasterClient, prefabName, position, rotation);
            }
        }

        public GameObject InstantiateLocalObject(string prefabName, int actorNumber, Vector3? position = null, Quaternion? rotation = null)
        {
            var actualPosition = position ?? Vector3.zero;
            var actualRotation = rotation ?? Quaternion.identity;
            var (gameObject, viewId) = RpcInstantiate(prefabName, actualPosition, actualRotation, ViewIdAllocationMethod.Local, 0);

            this.photonView.RPC(nameof(RpcInstantiate), RpcTarget.Others, prefabName, position, rotation, ViewIdAllocationMethod.Specific, viewId);

            return gameObject;
        }

        public GameObject InstantiateStatic(string prefabName, Vector3? position = null, Quaternion? rotation = null)
        {            
            var actualPosition = position ?? Vector3.zero;
            var actualRotation = rotation ?? Quaternion.identity;
            var (gameObject, viewId) = RpcInstantiate(prefabName, actualPosition, actualRotation, ViewIdAllocationMethod.Static, 0);

            this.photonView.RPC(nameof(RpcInstantiate), RpcTarget.Others, prefabName, position, rotation, ViewIdAllocationMethod.Static, 0);

            return gameObject;
        }

        public IEnumerable<SpawnedObject> GetSpawnedObjects()
        {
            return FindObjectsOfType<MonoBehaviour>().OfType<SpawnedObject>();
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
                        Debug.LogError($"== HASHCLASH!! {NameWithPath(this.transformFromHash[hash].gameObject)} {path}");
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


    }
}
