using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Networking;

namespace Multiplayer
{
    //public class MultiplayerSession : NetworkedObjectPunCallbacks
    //{
    //    protected const int SceneViewIDUpperBound = 999;

    //    protected Func<int, MultiplayerSessionScene> InstantiateMultiplayerSessionScene = GameObjectExtension.InstantiateWithView<MultiplayerSessionScene>;

    //    protected string CurrentSceneName
    //    {
    //        get => this.currentSceneName;
    //        private set
    //        {
    //            if (this.currentSceneName != value)
    //            {
    //                this.currentSceneName = value;

    //                this.CurrentSceneLoaded = false;
    //                this.CurrentSceneReady = false;

    //                OnLoadScene();
    //            }
    //        }
    //    }

    //    protected bool CurrentSceneLoaded
    //    {
    //        get => this.currentSceneLoaded;
    //        private set
    //        {
    //            this.currentSceneLoaded = value;

    //            PhotonNetwork.IsMessageQueueRunning = this.currentSceneLoaded;

    //            if (this.currentSceneLoaded)
    //            {
    //                if (TryGetSceneViewID(this.currentSceneName, out int viewId))
    //                {
    //                    this.currentSceneViewId = viewId;
    //                    this.CurrentScene = this.InstantiateMultiplayerSessionScene.Invoke(viewId);

    //                    Debug.Log($"Created multiplayer session scene with ID {viewId}");

    //                    if (this.IsHost)
    //                    {
    //                        this.CurrentScene.SendFullState(RpcTarget.Others);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    protected bool CurrentSceneReady { get; private set; }

    //    private MultiplayerSessionScene CurrentScene { get; set; }

    //    private int currentSceneViewId;
    //    private string currentSceneName;
    //    private bool currentSceneLoaded;

    //    protected override void Awake()
    //    {
    //        SceneManager.activeSceneChanged += this.OnActiveSceneChanged;

    //        MultiplayerSessionScene.OnMultiplayerSessionSceneReady += this.OnSceneReady;

    //        this.currentSceneName = SceneManager.GetActiveScene().name;
    //        this.CurrentSceneLoaded = true;

    //        base.Awake();
    //    }

    //    protected virtual void OnSceneReady()
    //    {
    //        this.CurrentSceneReady = true;
    //    }

    //    /// <summary>
    //    /// Override this to provide some kind of fade out before a new scene is loaded
    //    /// </summary>
    //    protected virtual void OnLoadScene()
    //    {
    //        SceneManager.LoadSceneAsync(this.currentSceneName);
    //    }

    //    public void ChangeScene(string scene)
    //    {
    //        Debug.Log("Sending RpcChangeScene");
    //        this.photonView.RPC(nameof(this.RpcChangeScene), RpcTarget.Others, scene);
    //        PhotonNetwork.SendAllOutgoingCommands();
    //        this.CurrentSceneName = scene;
    //    }

    //    public void ForceChangeScene(string scene)
    //    {
    //        Debug.Log("Sending RpcForceChangeScene");
    //        this.photonView.RPC(nameof(this.RpcForceChangeScene), RpcTarget.Others, scene);
    //        PhotonNetwork.SendAllOutgoingCommands();
    //        this.currentSceneName = null;
    //        this.CurrentSceneName = scene;
    //    }

    //    /// <summary>
    //    /// Call this to instantiate an object dynamically after the scene has been loaded
    //    /// </summary>
    //    /// <param name="viewID"></param>
    //    /// <param name="prefabName"></param>
    //    /// <param name="position"></param>
    //    /// <param name="rotation"></param>
    //    /// <param name="isRemote"></param>
    //    public void Instantiate(int viewID, string prefabName, Vector3 position, Quaternion rotation)
    //    {
    //        if (this.CurrentSceneLoaded)
    //        {
    //            this.CurrentScene.Instantiate(viewID, prefabName, position, rotation);
    //        }
    //    }

    //    [PunRPC]
    //    protected void RpcChangeScene(string scene)
    //    {
    //        Debug.Log("Recevied RpcChangeScene");
    //        this.CurrentSceneName = scene;
    //    }

    //    [PunRPC]
    //    protected void RpcForceChangeScene(string scene)
    //    {
    //        Debug.Log("Recevied RpcForcehangeScene");
    //        this.currentSceneName = null;
    //        this.CurrentSceneName = scene;
    //    }

    //    public override void OnPlayerEnteredRoom(Player newPlayer)
    //    {
    //        this.SendFullState(newPlayer);

    //        if (this.CurrentScene != null)
    //        {
    //            this.CurrentScene.SendFullState(newPlayer);
    //        }
    //    }

    //    protected override void FullStateSync(bool isWriting, Queue<object> data)
    //    {
    //        if (isWriting)
    //        {
    //            data.Enqueue(this.CurrentSceneName);
    //        }
    //        else
    //        {
    //            this.CurrentSceneName = data.Dequeue<string>();
    //        }
    //    }

    //    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    //    {
    //        if (newScene.name == this.CurrentSceneName)
    //        {
    //            this.CurrentSceneLoaded = true;
    //        }
    //    }


    //    protected override void OnDestroy()
    //    {
    //        base.OnDestroy();

    //        if (this.CurrentScene != null)
    //        {
    //            Destroy(this.CurrentScene.gameObject);
    //        }

    //        MultiplayerSessionScene.OnMultiplayerSessionSceneReady -= this.OnSceneReady;

    //        SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
    //    }

    //    protected virtual bool TryGetSceneViewID(string sceneName, out int viewId)
    //    {
    //        var scene = SceneManager.GetSceneByName(sceneName);
    //        if (scene != null)
    //        {
    //            viewId = SceneViewIDUpperBound - SceneManager.GetSceneByName(sceneName).buildIndex;
    //            return true;
    //        }

    //        viewId = 0;
    //        return false;
    //    }
    //}
}
