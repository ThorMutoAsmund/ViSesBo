using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace VSB
{
    public enum VSBApplicationType
    {
        Instructor,
        Trainee
    }

    public class VSBApplication : MonoBehaviour
    {
        public static VSBApplication Instance { get; private set; }

        public VSBApplicationType ApplicationType { get; private set; }

        public static void Start(VSBApplicationType applicationType)
        {
            if (Instance == null)
            {
                var gameObjcet = new GameObject(typeof(VSBApplication).Name);
                UnityEngine.Object.DontDestroyOnLoad(gameObjcet);
                Instance = gameObjcet.AddComponent<VSBApplication>(instance =>
                {
                    instance.ApplicationType = applicationType;
                });
            }
        }

        public void CreateRoom(string roomName)
        {
            PhotonNetwork.CreateRoom(roomName, roomOptions: new RoomOptions()
            {
                MaxPlayers = 0,
                PlayerTtl = 30000,
                EmptyRoomTtl = 30000,
                PublishUserId = true,
                CleanupCacheOnLeave = true
            });
        }
    }
}
