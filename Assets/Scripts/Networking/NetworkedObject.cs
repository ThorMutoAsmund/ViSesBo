using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public abstract class NetworkedObject : MonoBehaviourPun, INetworkedObject
    {
        protected MultiplayerAutoSync AutoSync { get; private set; }

        private Queue<object> partialDataQueue = new Queue<object>();

        protected virtual void Awake()
        {
            var photonView = this.GetComponent<PhotonView>();
            if (photonView && photonView.ViewID > 0)
            {
                NetworkManager.Register(this);
            }

            this.AutoSync = MultiplayerAutoSync.Create(this);
        }

        protected virtual void Update()
        {
            // Send updates every frame, if there are changes
            if (this.photonView.IsMine && this.AutoSync.IsChanged)
            {
                var keys = this.AutoSync.PartialStateSync(true, this.partialDataQueue);
                this.AutoSync.AcceptChanges();

                var dataArray = this.partialDataQueue.ToArray();
                this.partialDataQueue.Clear();

                this.photonView.RPC(nameof(this.RpcSendPartialState), RpcTarget.Others, (object)dataArray);

                AfterSync(keys);
            }
        }

        [PunRPC]
        protected virtual void RpcSendPartialState(object[] dataArray)
        {
            var dataQueue = new Queue<object>(dataArray);

            var keys = this.AutoSync.PartialStateSync(false, dataQueue);
            this.AutoSync.AcceptChanges();

            AfterSync(keys);
        }

        [PunRPC]
        public void RpcDestroyObject()
        {
            this.photonView.RPC(nameof(RpcDestroyObject), RpcTarget.Others);

            UnityEngine.Object.Destroy(this.gameObject);
        }


        public virtual void RpcSyncState(bool isWriting, Queue<object> dataQueue)
        {
            this.AutoSync.FullStateSync(isWriting, dataQueue);
        }

        protected virtual void AfterSync(string[] keys)
        {
        }
    }
}
