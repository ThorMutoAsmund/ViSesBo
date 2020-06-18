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
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        public static readonly int NetworkManagerViewId = 999;
        public static readonly int NetworkedSceneViewId = 998;

        private static Dictionary<int, INetworkedObject> networkedObjectList = new Dictionary<int, INetworkedObject>();
        private Dictionary<Object, System.Action> connectionHandlers = new Dictionary<Object, System.Action>();
        private Dictionary<Object, System.Action> joinedRoomHandlers = new Dictionary<Object, System.Action>();

        public static void StartMultiplayer(int sendRate = 40, int serializationRate = 40, float minimalTimeScaleToDispatchInFixedUpdate = -1f)
        {
            if (Instance == null)
            {
                var gameObjcet = new GameObject(typeof(NetworkManager).Name);
                Object.DontDestroyOnLoad(gameObjcet);
                Instance = gameObjcet.AddComponent<NetworkManager>();

                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.SendRate = sendRate;
                PhotonNetwork.SerializationRate = serializationRate;
                PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = minimalTimeScaleToDispatchInFixedUpdate;
                //PhotonNetwork.PrecisionForFloatSynchronization = 0.002f;
                //PhotonNetwork.PrecisionForQuaternionSynchronization = 0.2f;
                //PhotonNetwork.PrecisionForVectorSynchronization = 0.0000099f;                       
            } 
        }

        public static void EndMultiplayer()
        {
            if (Instance != null)
            {
                PhotonNetwork.Disconnect();
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        private void Awake()
        {
            var view = this.gameObject.AddComponent<PhotonView>();
            view.ViewID = NetworkManagerViewId;
            NetworkCustomTypes.Register();
        }

        private void Start()
        {
            SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                NetworkManager.NewSceneLoaded();
            };
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("== Connected to master");

            foreach (var caller in this.connectionHandlers.Keys)
            {
                if (caller)
                {
                    this.connectionHandlers[caller]?.Invoke();
                }
            }
            this.connectionHandlers.Clear();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("== Disconnected");
        }

        public void WhenConnectedToMaster(Object caller, System.Action callBack)
        {
            this.connectionHandlers.Remove(caller);

            if (callBack == null)
            {
                return;
            }

            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                callBack.Invoke();
                return;
            }

            this.connectionHandlers[caller] = callBack;
        }

        public void WhenJoinedRoom(Object caller, System.Action callBack)
        {
            this.joinedRoomHandlers.Remove(caller);

            if (callBack == null)
            {
                return;
            }

            if (PhotonNetwork.InRoom)
            {
                callBack.Invoke();
                return;
            }

            this.joinedRoomHandlers[caller] = callBack;
        }

        [PunRPC]
        private void RpcSpawnObjects(string[] resources, Vector3[] positions, Quaternion[] rotations, int[] viewIds)
        {
            if (NetworkedScene.NetworkSceneInstance)
            {
                Debug.Log($"== Spawning {resources.Length} objects");
                for (int i = 0; i < resources.Length; ++i)
                {
                    var gameObject = NetworkedScene.NetworkSceneInstance.RpcInstantiate(resources[i], positions[i], rotations[i],  ViewIdAllocationMethod.Specific, viewIds[i]);
                }
            }
            else
            {
                Debug.LogWarning("No scene. Cannot spawn objects.");
            }
        }

        [PunRPC]
        private void RpcTurnOn()
        {
            Debug.Log($"== Turning on");
        }

        [PunRPC]
        private void RpcForceLoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        public void ForceLoadScene(Player newPlayer, string sceneName)
        {
            this.photonView.RPC(nameof(RpcForceLoadScene), newPlayer, sceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                this.photonView.RPC(nameof(RpcForceLoadScene), RpcTarget.Others, sceneName);
                PhotonNetwork.SendAllOutgoingCommands();
                PhotonNetwork.IsMessageQueueRunning = false;
                SceneManager.LoadSceneAsync(sceneName);
            }
        }
        
        [PunRPC]
        public void RpcSceneLoaded(int actorNumber)
        {
            Debug.Log($"== Actor {actorNumber} has loaded scene");
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null && NetworkedScene.NetworkSceneInstance)
            {
                // Instantiate all spawned objects
                var (resources, positions, rotations, viewIds) = NetworkedScene.NetworkSceneInstance.GetSpawnedObjects();
                Debug.Log($"== Requesting actor {actorNumber} to spawn {resources.Count()} networked objects");
                this.photonView.RPC(nameof(RpcSpawnObjects), player, resources, positions, rotations, viewIds);

                // Sync all networked objects
                foreach (var kvp in networkedObjectList)
                {
                    var networkedObject = kvp.Value;
                    if (networkedObject == null || networkedObject.Equals(null) || !(networkedObject is MonoBehaviour monoBehaviour))
                    {
                        continue;
                    }

                    var photonView = monoBehaviour.GetComponent<PhotonView>();

                    if (!photonView)
                    {
                        continue;
                    }

                    var dataQueue = new Queue<object>();
                    networkedObject.RpcSyncState(true, dataQueue);
                    photonView.RPC(nameof(INetworkedObject.RpcSyncState), player, false, dataQueue);
                }

                // Tell player to enable network communication
                this.photonView.RPC(nameof(RpcTurnOn), player);
            }
        }

        public void ReportSceneLoaded()
        {
            this.photonView.RPC(nameof(RpcSceneLoaded), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log($"== Created room {PhotonNetwork.CurrentRoom?.Name}");
        }
        public override void OnJoinedRoom()
        {
            Debug.Log($"== Joined room {PhotonNetwork.CurrentRoom?.Name}");

            foreach (var caller in this.joinedRoomHandlers.Keys)
            {
                if (caller)
                {
                    this.joinedRoomHandlers[caller]?.Invoke();
                }
            }
            this.joinedRoomHandlers.Clear();
        }
        public override void OnLeftRoom()
        {
            Debug.Log("== Left room");
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"== Master client switched. New master is {newMasterClient?.ActorNumber}");
        }

        public static void Register(INetworkedObject networkedObject)
        {
            if (!Application.isPlaying)
            {
                networkedObjectList = new Dictionary<int, INetworkedObject>();
                return;
            }

            if (!(networkedObject is MonoBehaviour monoBehaviour))
            {
                Debug.LogWarning("Cannot register networked object that is not a monobehaviour.");
                return;
            }

            var photonView = monoBehaviour.GetComponent<PhotonView>();

            if (!photonView)
            {
                Debug.LogWarning("Cannot register networked object without a PhotonView.");
                return;
            }

            networkedObjectList[photonView.ViewID] = networkedObject;

            Debug.Log($"There are now {networkedObjectList.Count()} items networked");
        }

        internal static void NewSceneLoaded()
        {
            var removeKeys = new List<int>();
            foreach (var kvp in networkedObjectList)
            {
                var networkedObject = kvp.Value;
                if (networkedObject == null || networkedObject.Equals(null))
                {
                    removeKeys.Add(kvp.Key);
                }
            }

            for (int index = 0; index < removeKeys.Count; index++)
            {
                int key = removeKeys[index];
                networkedObjectList.Remove(key);
            }

            Debug.Log($"Cleared {removeKeys.Count} items");
        }
    }
}
