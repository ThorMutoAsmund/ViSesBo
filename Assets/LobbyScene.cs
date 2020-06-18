﻿using Networking;
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
#pragma warning restore 0649

        public RoomInfo[] RoomList { get; private set; } = new RoomInfo[0];
        private Dictionary<string, RoomInfo> CachedRoomList { get; } = new Dictionary<string, RoomInfo>();

        private void Awake()
        {
            VSBApplication.Start();
        }


        private void Start()
        {
            NetworkManager.Instance.WhenConnectedToMaster(this, () =>
            {
                PhotonNetwork.JoinLobby();
            });
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

            this.RoomList = this.CachedRoomList.Values.ToArray();
        }


        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            // Update internal cached room list
            UpdateCachedRoomList(roomList);

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

            Debug.Log("== Room list updated");
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


        public override void OnJoinedLobby()
        {
            Debug.Log("== Joined lobby");
        }
    }
}