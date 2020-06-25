using Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VSB
{
    public class GameScene : NetworkedScene
    {
#pragma warning disable 0649
        [SerializeField] private Text statusText;
#pragma warning restore 0649

        public static GameScene Instance => NetworkedScene.NetworkSceneInstance as GameScene;
        public Terrain Terrain { get; private set; }

        private RigidPlayer localPlayer;
        private Coroutine ensureConnectedCoroutine;
        private bool reconnectWhenDisconnected;
        private bool createOfflineRoom = true;
        private string roomName;

        protected override void Awake()
        {
            base.Awake();

            VSBApplication.Start(VSBApplicationType.Trainee);

            PhotonNetwork.OfflineMode = false;

            NetworkManager.Connect();

            if (!PhotonNetwork.InRoom)
            {
                JoinRoomWhenConnected();
            }
            else
            {
                this.createOfflineRoom = false;
                this.reconnectWhenDisconnected = true;
                this.roomName = PhotonNetwork.CurrentRoom.Name;

                if (this.statusText)
                {
                    this.statusText.text = $"Network: Connected {(PhotonNetwork.OfflineMode ? " (offline mode)" : string.Empty)}";
                }
            }

            this.Terrain = this.GetComponentInChildren<Terrain>();
        }

        protected override void Start()
        {
            base.Start();

            // Fade in
            ScreenFade.FadeIn();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!PhotonNetwork.OfflineMode)
            {
                this.ensureConnectedCoroutine = StartCoroutine(EnsureConnected());
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (this.ensureConnectedCoroutine != null)
            {
                StopCoroutine(this.ensureConnectedCoroutine);
                this.ensureConnectedCoroutine = null;
            }
        }

        private void Update()
        {
            if (VSBApplication.Instance.ApplicationType == VSBApplicationType.Trainee)
            {
                if (Input.GetKeyDown(KeyCode.J))
                {
                    JoinRandomRoom();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadScene(Scenes.Scene1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadScene(Scenes.Scene2);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Networking.ScreenFade.FadeOut(whenDone: () =>
                {
                    if (VSBApplication.Instance.ApplicationType == VSBApplicationType.Instructor)
                    {            
                        PhotonNetwork.LeaveRoom();
                        SceneManager.LoadSceneAsync(Scenes.LobbyScene);
                    }
                    else
                    {
                        NetworkManager.Instance.LoadScene(Scenes.StartGameScene);
                    }
                });
            }
        }

        protected override void SpawnPlayer(Player newPlayer = null)
        {
            // Find spawn point
            var spawnPoint = FindObjectOfType<SpawnPoint>();

            // Instantiate and set name and position
            Prefabs.Instance.Player.SetActive(false);
            var playerGameObject = Instantiate(Prefabs.Instance.Player);
            playerGameObject.name = newPlayer == null ? "Local Player" : $"Player {newPlayer.ActorNumber}";
            playerGameObject.transform.position = spawnPoint.transform.position;

            // Set actor number
            var player = playerGameObject.GetComponent<RigidPlayer>();
            player.SetActorNumber(newPlayer?.ActorNumber ?? 0);

            // Activate
            playerGameObject.SetActive(true);

            // Make camera track player
            if (newPlayer == null)
            {
                var cameraRig = FindObjectOfType<CameraRig>();
                cameraRig.AttachTo(player.CameraAttachmentPoint);
                this.localPlayer = player;

                AdjustSpawnPosition(PhotonNetwork.LocalPlayer);
            }
        }

        protected override void PhotonPlayerAfterRoomJoined(Player newPlayer)
        {
            AdjustSpawnPosition(newPlayer);
        }

        private void AdjustSpawnPosition(Player player)
        { 
            if (this.localPlayer != null)
            {
                localPlayer.transform.position = localPlayer.transform.position.Add(z: player.ActorNumber * -5f);
            }
        }

        private void JoinRoomWhenConnected()
        {
            NetworkManager.Instance.WhenConnectedToMaster(this, () =>
            {
                if (this.statusText)
                {
                    this.statusText.text = $"Network: Connected {(PhotonNetwork.OfflineMode ? " (offline mode)" : string.Empty)}";
                }

                this.createOfflineRoom = false;

                if (this.reconnectWhenDisconnected)
                {
                    PhotonNetwork.RejoinRoom(this.roomName);
                }
                else
                {
                    this.reconnectWhenDisconnected = true;

                    this.roomName = PhotonNetwork.OfflineMode ? "offlineRoom" : System.Guid.NewGuid().ToString().Substring(0, 8);
                    VSBApplication.Instance.CreateRoom(this.roomName);
                }
            });
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("== Rejoining room failed! Session will be termnated.");
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                SceneManager.LoadSceneAsync(VSBApplication.Instance.ApplicationType == VSBApplicationType.Instructor ? Scenes.LobbyScene : Scenes.StartGameScene);
            });
        }

        public override void OnPlayerLeftRoom(Player newPlayer)
        {
            if (VSBApplication.Instance.ApplicationType == VSBApplicationType.Instructor && PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    Debug.Log("== Empty room! Leaving.");

                    PhotonNetwork.LeaveRoom();
                    SceneManager.LoadSceneAsync(Scenes.LobbyScene);
                }
            }
        }

        private void JoinRandomRoom()
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                PhotonNetwork.LeaveRoom();
                NetworkManager.Instance.WhenConnectedToMaster(this, () =>
                {
                    PhotonNetwork.JoinRandomRoom();
                });
            });
        }

        private void LoadScene(string sceneName)
        {
            if (NetworkManager.Instance.CanLoadScene(sceneName, out string sceneLoadNotPossibleReason))
            {
                Networking.ScreenFade.FadeOut(whenDone: () =>
                {
                    NetworkManager.Instance.LoadScene(sceneName);
                });
            }
            else
            {
                Debug.LogWarning($"== Cannot load scene. Reasone: {sceneLoadNotPossibleReason}");
            }
        }

        public void CorrectHeight(Transform transform, bool correctParent = false)
        {
            if (this.Terrain)
            {
                var terrainHeight = this.Terrain.SampleHeight(transform.position);

                if (correctParent)
                {
                    transform.parent.transform.position = transform.parent.transform.position.With(y: terrainHeight);
                }
                else
                {
                    transform.position = transform.position.With(y: terrainHeight);
                }
            }
        }

        private IEnumerator EnsureConnected()
        {
            for (; ; )
            {
                if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
                {
                    if (this.statusText)
                    {
                        this.statusText.text = "Network: Offline";
                    }

                    JoinRoomWhenConnected();

                    if (this.createOfflineRoom)
                    {
                        Debug.Log($"== Starting in offline mode");

                        StopCoroutine(this.ensureConnectedCoroutine);
                        this.ensureConnectedCoroutine = null;

                        PhotonNetwork.OfflineMode = true;
                    }
                    else
                    {
                        if (this.reconnectWhenDisconnected)
                        {
                            Debug.Log($"== Disconnected. Trying to reconnect...");
                            PhotonNetwork.Reconnect();
                        }
                        else
                        {
                            Debug.Log($"== Disconnected. Trying to connect...");
                            NetworkManager.Connect();
                        }
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }
}