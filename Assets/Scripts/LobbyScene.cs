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
    public class LobbyScene : MonoBehaviourPunCallbacks
    {
    #pragma warning disable 0649
        [SerializeField] private Text menuText;
        [SerializeField] private Text statusText;
#pragma warning restore 0649

        private RoomInfo[] RoomList => this.CachedRoomList.Values.ToArray();
        private Dictionary<string, RoomInfo> CachedRoomList { get; } = new Dictionary<string, RoomInfo>();
        private Coroutine ensureConnectedCoroutine;
        private bool reconnectWhenDisconnected;

        private void Awake()
        {
            VSBApplication.Start(VSBApplicationType.Instructor);

            NetworkManager.Connect();

            if (!PhotonNetwork.InLobby)
            {
                JoinLobbyWhenConnected();
            }
            else
            {
                if (this.statusText)
                {
                    this.statusText.text = $"Network: Connected";
                }
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

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"== Join room failed! {message}");

            ReloadLobby();
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("== Empty room! Leaving.");

                ReloadLobby();
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            // Update internal cached room list
            UpdateCachedRoomList(roomList);
            CreateGui();

            Debug.Log("== Room list updated");
        }

        private void CreateGui()
        { 
            if (this.RoomList.Length == 0)
            {
                this.menuText.text = "No rooms";
            }
            else
            {
                int i = 0;

                this.menuText.text = $"Press a key to join a room\n{i++}: random\n";
                foreach (var roomInfo in this.RoomList)
                {
                    this.menuText.text += $"{i++}: {roomInfo.Name}\n";
                }
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("== Joined lobby");
        }

        private void Update()
        {
            if (PhotonNetwork.InLobby)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    JoinRandomRoom();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1) && this.RoomList.Length > 0)
                {
                    JoinRoom(this.RoomList[0].Name);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) && this.RoomList.Length > 1)
                {
                    JoinRoom(this.RoomList[1].Name);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) && this.RoomList.Length > 2)
                {
                    JoinRoom(this.RoomList[2].Name);
                }
            }
            else if (PhotonNetwork.InRoom)
            { 
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ReloadLobby();
                }
            }
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (this.CachedRoomList.ContainsKey(info.Name))
                    {
                        this.CachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (this.CachedRoomList.ContainsKey(info.Name))
                {
                    this.CachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    this.CachedRoomList.Add(info.Name, info);
                }
            }
        }

        private void ReloadLobby()
        {
            this.CachedRoomList.Clear();
            CreateGui();

            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                
                NetworkManager.Instance.WhenConnectedToMaster(this, () =>
                {
                    PhotonNetwork.JoinLobby();
                    ScreenFade.FadeIn();
                });
            }
            else
            {
                PhotonNetwork.JoinLobby();
                ScreenFade.FadeIn();
            }
        }

        private void JoinRandomRoom()
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                PhotonNetwork.LeaveLobby();
                PhotonNetwork.JoinRandomRoom();
            });
        }

        private void JoinRoom(string roomName)
        {
            Networking.ScreenFade.FadeOut(whenDone: () =>
            {
                PhotonNetwork.LeaveLobby();
                PhotonNetwork.JoinRoom(roomName);
            });
        }

        private void JoinLobbyWhenConnected()
        {
            NetworkManager.Instance.WhenConnectedToMaster(this, () =>
            {
                if (this.statusText)
                {
                    this.statusText.text = $"Network: Connected";
                }

                this.reconnectWhenDisconnected = true;
                PhotonNetwork.JoinLobby();
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

                    this.CachedRoomList.Clear();
                    CreateGui();

                    JoinLobbyWhenConnected();

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