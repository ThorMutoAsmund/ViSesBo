using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static NetworkedScene NetworkSceneInstance { get; private set; }

        protected virtual void Awake()
        {
            NetworkSceneInstance = this;

            var view = this.gameObject.AddComponent<PhotonView>();
            view.ViewID = NetworkManager.NetworkedSceneViewId;

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
                PhotonNetwork.IsMessageQueueRunning = true;
            }
        }

        protected virtual void Start()
        {
            NetworkManager.Instance.WhenJoinedRoom(this, () =>
            {
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
            Debug.Log($"== Player {newPlayer.ActorNumber} entered room");

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
            var gameObject = Object.Instantiate(Resources.Load<GameObject>(prefabName), position, rotation);
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
                    Debug.LogWarning("Instantiated object has no PhotonView and will not be networked");
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

        public (string[] resources, Vector3[] positions, Quaternion[] rotations, int[] viewIds) GetSpawnedObjects()
        {
            var spawnedObjects = FindObjectsOfType<MonoBehaviour>().OfType<SpawnedObject>();
            return (spawnedObjects.Select(obj => obj.ResourceName).ToArray(),
                    spawnedObjects.Select(obj => (obj as MonoBehaviour).transform.position).ToArray(),
                    spawnedObjects.Select(obj => (obj as MonoBehaviour).transform.rotation).ToArray(),
                    spawnedObjects.Select(obj => (obj as MonoBehaviour).GetComponent<PhotonView>()?.ViewID ?? 0).ToArray());
        }
    }
}
