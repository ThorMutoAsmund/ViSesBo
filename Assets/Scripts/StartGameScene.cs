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
    public class StartGameScene : MonoBehaviourPunCallbacks
    {
    #pragma warning disable 0649
        [SerializeField] private Text menuText;
#pragma warning restore 0649

        private RoomInfo[] RoomList => this.CachedRoomList.Values.ToArray();
        private Dictionary<string, RoomInfo> CachedRoomList { get; } = new Dictionary<string, RoomInfo>();
        private Coroutine ensureConnectedCoroutine;
        private bool reconnectWhenDisconnected;
        private string roomName;

        private void Awake()
        {
            VSBApplication.Start(VSBApplicationType.Trainee);

            NetworkManager.Connect();

            if (!PhotonNetwork.InRoom)
            {
                CreateRoomWhenConnected();
            }
            else 
            {
                this.reconnectWhenDisconnected = true;
                this.roomName = PhotonNetwork.CurrentRoom.Name;
            }
        }

        private void Start()
        {
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
            if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadScene("GrassScene");
            }
            else if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadScene("ConcreteScene");
            }
        }

        private void LoadScene(string sceneName)
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                NetworkManager.Instance.LoadScene(sceneName);
            });
        }

        private void CreateRoomWhenConnected()
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

        private IEnumerator EnsureConnected()
        {
            for (; ; )
            {
                if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
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

                yield return new WaitForSeconds(1f);
            }
        }
    }
}