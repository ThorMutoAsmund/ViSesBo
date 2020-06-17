using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Multiplayer
{
    //public class UIDController : MonoBehaviour
    //{
    //    public static UIDController Instance { get; private set; }

    //    private Dictionary<int, Transform> transformFromUID = new Dictionary<int, Transform>();
    //    private Dictionary<Transform, int> uidFromTransform = new Dictionary<Transform, int>();

    //    private void Awake()
    //    {
    //        if (Instance != null)
    //        {
    //            Destroy(this);
    //            return;
    //        }

    //        Instance = this;

    //        SceneManager.sceneLoaded += (scene, loadSceneMode) =>
    //        {
    //            var id = 0;

    //            var toProcess = new Queue<Transform>(SceneManager.GetActiveScene().GetRootGameObjects().Select(go => go.transform));
    //            this.transformFromUID.Clear();
    //            this.uidFromTransform.Clear();

    //            while (toProcess.Count != 0)
    //            {
    //                var t = toProcess.Dequeue();
    //                this.transformFromUID[++id] = t;
    //                this.uidFromTransform[t] = id;
    //                for (int c = 0; c < t.childCount; ++c)
    //                {
    //                    toProcess.Enqueue(t.GetChild(c));
    //                }
    //            }
    //        };
    //    }

    //    private void OnDestroy()
    //    {
    //        if (Instance == this)
    //        {
    //            Instance = null;
    //        }
    //    }

    //    public Transform TransformFromUID(int uid)
    //    {
    //        return this.transformFromUID.ContainsKey(uid) ? this.transformFromUID[uid] : default;
    //    }
    //    public int UIDFromTransform(Transform transform)
    //    {
    //        return !transform ? 0 : (this.uidFromTransform.ContainsKey(transform) ? this.uidFromTransform[transform] : 0);
    //    }
    //}
}
