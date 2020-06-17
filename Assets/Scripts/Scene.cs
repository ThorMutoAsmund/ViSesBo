using Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VSB
{
    public class Scene : NetworkedScene
    {
        public static Scene Instance => NetworkedScene.NetworkSceneInstance as Scene;
        public Terrain Terrain { get; private set; }

        private RigidPlayer localPlayer;
        protected override void Awake()
        {
            base.Awake();

            this.Terrain = this.GetComponentInChildren<Terrain>();

            NetworkManager.StartMultiplayer();
        }

        protected override void Start()
        {
            base.Start();

            NetworkManager.Instance.WhenConnectedToMaster(this, () =>
            {
                var roomName = System.Guid.NewGuid().ToString().Substring(0, 8);
                PhotonNetwork.CreateRoom(roomName, new RoomOptions()
                {
                });
            });

            // Fade in
            Multiplayer.ScreenFade.FadeIn();
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

        private void JoinRandomRoom()
        {
            Multiplayer.ScreenFade.FadeOut(whenDone: () =>
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                PhotonNetwork.LeaveRoom();
                NetworkManager.Instance.WhenConnectedToMaster(this, () =>
                {
                    PhotonNetwork.JoinRandomRoom();
                });
            });
        }

        private void LoadScene(string sceneName)
        {
            Multiplayer.ScreenFade.FadeOut(whenDone: () =>
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

    }
}