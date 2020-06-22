using Networking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    }
}
