using Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VSB
{ 
    public class StartGameScene : NetworkedScene
    {
#pragma warning disable 0649
        [SerializeField] private Text statusText;
#pragma warning restore 0649

        private Coroutine ensureConnectedCoroutine;
        private bool reconnectWhenDisconnected;
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
                this.reconnectWhenDisconnected = true;
                this.roomName = PhotonNetwork.CurrentRoom.Name;
            }
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
            if (VSBApplication.Instance.ApplicationType == VSBApplicationType.Trainee)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadScene("GrassScene");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadScene("ConcreteScene");
                }
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            // If we tried a reconnect, but it failed, just create a new room
            if (this.reconnectWhenDisconnected)
            {
                this.reconnectWhenDisconnected = false;
                JoinRoomWhenConnected();
            }
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
                Debug.LogWarning($"== Cannot load scene. Reason: {sceneLoadNotPossibleReason}");
            }
        }

        private void JoinRoomWhenConnected()
        {
            NetworkManager.Instance.WhenConnectedToMaster(this, () =>
            {
                if (this.statusText)
                {
                    this.statusText.text = "Network: Connected";
                }

                if (this.reconnectWhenDisconnected)
                {
                    PhotonNetwork.RejoinRoom(this.roomName);
                }
                else
                {
                    this.reconnectWhenDisconnected = true;

                    this.roomName = System.Guid.NewGuid().ToString().Substring(0, 8);
                    VSBApplication.Instance.CreateRoom(this.roomName);
                }
            });
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

        protected override void SpawnPlayer(Player newPlayer = null) { }
    }
}