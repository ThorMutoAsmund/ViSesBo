using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Multiplayer
{
    //public sealed class MultiplayerRoomManager : MonoBehaviourPunCallbacks
    //{
    //    private const string CurrentSceneProperty = "_sceneName";
    //    private const string JoinWhenCreatedProperty = "_joinWhenCreated";

    //    public static MultiplayerRoomManager Instance { get; private set; }

    //    public Action RoomListUpdated;

    //    public Func<RoomOptions> GetRoomOptions;

    //    public Func<TypedLobby> GetLobby;

    //    private Dictionary<string, RoomInfo> CachedRoomList { get; } = new Dictionary<string, RoomInfo>();

    //    public RoomInfo[] RoomList { get; private set; } = new RoomInfo[0];

    //    public static void StartRoomManager()
    //    {
    //        if (Instance == null)
    //        {
    //            Instance = GameObjectExtension.Instantiate<MultiplayerRoomManager>(dontDestroyOnLoad: true);
    //        }
    //    }
    //    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    //    {
    //        foreach (RoomInfo info in roomList)
    //        {
    //            // Remove room from cached room list if it got closed, became invisible or was marked as removed
    //            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
    //            {
    //                if (this.CachedRoomList.ContainsKey(info.Name))
    //                {
    //                    this.CachedRoomList.Remove(info.Name);
    //                }

    //                continue;
    //            }

    //            // Update cached room info
    //            if (this.CachedRoomList.ContainsKey(info.Name))
    //            {
    //                this.CachedRoomList[info.Name] = info;
    //            }
    //            // Add new room info to cache
    //            else
    //            {
    //                this.CachedRoomList.Add(info.Name, info);
    //            }
    //        }

    //        this.RoomList = this.CachedRoomList.Values.ToArray();
    //    }

    //    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //    {
    //        UpdateCachedRoomList(roomList);

    //        this.RoomListUpdated?.Invoke();
    //    }

    //    public override void OnLeftLobby()
    //    {
    //        this.CachedRoomList.Clear();
    //        this.RoomList = new RoomInfo[0];
    //    }

    //    public void CreateRoom(string roomName = null, string loadScene = null)
    //    {
    //        var roomOptions = this.GetRoomOptions?.Invoke() ?? new RoomOptions() { };

    //        // If user specifies a scene to load when room is joined, add that to the room properties
    //        if (!String.IsNullOrEmpty(loadScene))
    //        {
    //            roomOptions.CustomRoomProperties.Add(CurrentSceneProperty, loadScene);
    //            roomOptions.CustomRoomProperties.Add(JoinWhenCreatedProperty, true);
    //        }

    //        PhotonNetwork.CreateRoom(roomName, roomOptions, this.GetLobby?.Invoke());
    //    }

    //    public void JoinOrCreateRoom(string roomName, string loadScene = null)
    //    {
    //        var roomOptions = this.GetRoomOptions?.Invoke() ?? new RoomOptions() { };

    //        // If user specifies a scene to load when room is joined, add that to the room properties
    //        if (!String.IsNullOrEmpty(loadScene))
    //        {
    //            roomOptions.CustomRoomProperties.Add(CurrentSceneProperty, loadScene);
    //            roomOptions.CustomRoomProperties.Add(JoinWhenCreatedProperty, true);
    //        }

    //        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, this.GetLobby?.Invoke());
    //    }

    //    public override void OnCreatedRoom()
    //    {
    //        // Auto join?
    //        var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
    //        if (customProperties.ContainsKey(JoinWhenCreatedProperty) && (bool)customProperties[JoinWhenCreatedProperty] && customProperties.ContainsKey(CurrentSceneProperty))
    //        {
    //            var sceneName = (string)customProperties[CurrentSceneProperty];
    //            MultiplayerManager.ChangeScene(sceneName, force: true);
    //        }
    //    }
    //}
}