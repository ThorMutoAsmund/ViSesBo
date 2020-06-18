using Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    //public delegate void MultiplayerGameObjectHandler(MultiplayerGameObject multiplayerGameObject);

    //[RequireComponent(typeof(PhotonView))]
    //public abstract class MultiplayerGameObject : MonoBehaviourPun
    //{
    //    /// <summary>
    //    /// Save cpu time by caching this photon view
    //    /// TBD
    //    /// </summary>
    //    private Queue<object> partialDataQueue = new Queue<object>();

    //    public static event MultiplayerGameObjectHandler OnDestroyed;

    //    protected MultiplayerAutoSync AutoSync { get; private set; }
    //    protected bool IsHost => this.photonView.IsMine;

    //    public bool IsStateSynced { get; private set; }

    //    protected abstract void FullStateSync(bool isWriting, Queue<object> data);

    //    protected virtual void OnSynced()
    //    {
    //    }
    //    public virtual void OnViewIDSet()
    //    {
    //    }

    //    protected virtual void Awake()
    //    {
    //        this.AutoSync = MultiplayerAutoSync.Create(this);

    //        if (this.IsHost)
    //        {
    //            this.OnSynced();
    //        }
    //    }

    //    protected virtual void Update()
    //    {
    //        // Send updates every frame, if there are changes
    //        if (this.IsHost && this.AutoSync.IsChanged)
    //        {
    //            var keys = this.AutoSync.PartialStateSync(true, this.partialDataQueue);
    //            this.AutoSync.AcceptChanges();

    //            var dataArray = this.partialDataQueue.ToArray();
    //            this.partialDataQueue.Clear();

    //            this.photonView.RPC(nameof(this.RpcSendPartialState), RpcTarget.Others, (object)dataArray);

    //            AfterSync(keys);
    //        }
    //    }

    //    //[PunRPC]
    //    protected void RpcSendPartialState(object[] data)
    //    {
    //        var dataQueue = new Queue<object>(data);

    //        var keys = this.AutoSync.PartialStateSync(false, dataQueue);
    //        this.AutoSync.AcceptChanges();

    //        AfterSync(keys);
    //    }

    //    protected virtual void AfterSync(string[] keys)
    //    {
    //    }

    //    public virtual void SendFullState(Player player)
    //    {
    //        var dataQueue = new Queue<object>();

    //        this.FullStateSync(true, dataQueue);
    //        this.AutoSync.FullStateSync(true, dataQueue);

    //        var dataArray = dataQueue.ToArray();

    //        this.photonView.RPC(nameof(this.RpcSendFullState), player, (object)dataArray);
    //    }

    //    public virtual void SendFullState(RpcTarget target)
    //    {
    //        var dataQueue = new Queue<object>();

    //        this.FullStateSync(true, dataQueue);
    //        this.AutoSync.FullStateSync(true, dataQueue);

    //        var dataArray = dataQueue.ToArray();

    //        this.photonView.RPC(nameof(this.RpcSendFullState), target, (object)dataArray);
    //    }

    //    //[PunRPC]
    //    protected void RpcSendFullState(object[] data)
    //    {
    //        var dataQueue = new Queue<object>(data);

    //        this.FullStateSync(false, dataQueue);
    //        this.AutoSync.FullStateSync(false, dataQueue);
            

    //        this.IsStateSynced = true;

    //        this.OnSynced();
    //    }

    //    protected virtual void OnDestroy()
    //    {
    //        MultiplayerGameObject.OnDestroyed?.Invoke(this);
    //    }

    //    /// <summary>
    //    /// This helper method removes null-components from the observed component list before adding new objects.
    //    /// This often happens if you have an empty list in the editor and add components runtime
    //    /// </summary>
    //    /// <param name="components"></param>
    //    protected void AddObservedComponents(params Component[] components)
    //    {
    //        if (this.photonView.ObservedComponents.Count == 1 && !this.photonView.ObservedComponents[0])
    //        {
    //            this.photonView.ObservedComponents.Clear();
    //        }
    //        foreach (var component in components)
    //        {
    //            this.photonView.ObservedComponents.Add(component);
    //        }
    //    }
    //}
}
