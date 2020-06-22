using Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VSB
{
    public class GameScene : NetworkedScene
    {
        public static GameScene Instance => NetworkedScene.NetworkSceneInstance as GameScene;
        public Terrain Terrain { get; private set; }

        private RigidPlayer localPlayer;
        private Coroutine ensureConnectedCoroutine;
        private bool reconnectWhenDisconnected;
        private string roomName;

        protected override void Awake()
        {
            base.Awake();

            VSBApplication.Start(VSBApplicationType.Trainee);

            NetworkManager.Connect();

            if (!PhotonNetwork.InRoom)
            {
                JoinRoomWhenConnected();
            }
            else
            {
                this.reconnectWhenDisconnected = true;
                this.roomName = PhotonNetwork.CurrentRoom.Name;
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
            this.ensureConnectedCoroutine = StartCoroutine(EnsureConnected());
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
            if (Input.GetKeyDown(KeyCode.J))
            {
                JoinRandomRoom();
            }
            else if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadScene("GrassScene");
            }
            else if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadScene("ConcreteScene");
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Networking.ScreenFade.FadeOut(whenDone: () =>
                {
                    if (VSBApplication.Instance.ApplicationType == VSBApplicationType.Instructor)
                    {            
                        PhotonNetwork.LeaveRoom();
                        SceneManager.LoadSceneAsync("LobbyScene");
                    }
                    else
                    {
                        NetworkManager.Instance.LoadScene("StartGameScene");
                        //SceneManager.LoadSceneAsync("StartGameScene");
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
                if (this.reconnectWhenDisconnected)
                {
                    PhotonNetwork.RejoinRoom(this.roomName);
                }
                else
                {
                    this.reconnectWhenDisconnected = true;

                    this.roomName = System.Guid.NewGuid().ToString().Substring(0, 8);
                    PhotonNetwork.CreateRoom(this.roomName, roomOptions: new RoomOptions()
                    {
                        MaxPlayers = 0,
                        PlayerTtl = 3000,
                        EmptyRoomTtl = 3000,
                        PublishUserId = true,
                        CleanupCacheOnLeave = true
                    });
                }
            });
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                SceneManager.LoadSceneAsync(VSBApplication.Instance.ApplicationType == VSBApplicationType.Instructor ? "LobbyScene" : "StartGameScene");
            });
        }

        private void JoinRandomRoom()
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                PhotonNetwork.LeaveRoom();
                NetworkManager.Instance.WhenConnectedToMaster(this, () =>
                {
                    PhotonNetwork.JoinRandomRoom();
                });
            });
        }

        private void LoadScene(string sceneName)
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                NetworkManager.Instance.LoadScene(sceneName);
            });
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
                    JoinRoomWhenConnected();

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

                yield return new WaitForSeconds(1f);
            }
        }
    }
}