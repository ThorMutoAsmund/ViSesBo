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
        protected AutoSync AutoSync { get; private set; }

        protected virtual void Awake()
        {
            var photonView = this.GetComponent<PhotonView>();
            if (photonView && photonView.ViewID > 0)
            {
                NetworkManager.Register(this);
            }

            this.AutoSync = this.gameObject.AddComponent<AutoSync>();
            this.AutoSync.ObjectToSync = this;
            this.AutoSync.SyncCompleted += (sender, keys) => AfterSync(keys);

            this.photonView.Group = 1;
        }

        [PunRPC]
        public void RpcDestroyObject()
        {
            if (this.photonView.IsMine)
            {
                this.photonView.RPC(nameof(RpcDestroyObject), RpcTarget.Others);
                PhotonNetwork.SendAllOutgoingCommands();
            }

            UnityEngine.Object.Destroy(this.gameObject);
        }


        [PunRPC]
        public virtual void RpcFullStateSync(bool isWriting, Queue<object> dataQueue)
        {
            this.AutoSync.FullStateSync(isWriting, dataQueue);
        }

        protected virtual void AfterSync(string[] keys) { }
    }
}
