using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public abstract class NetworkedObject : MonoBehaviourPun, INetworkedObject
    {

        protected virtual void Awake()
        {
            var photonView = this.GetComponent<PhotonView>();
            if (photonView && photonView.ViewID > 0)
            {
                NetworkManager.Register(this);
            }
        }

        public abstract void RpcSyncState(bool isWriting, Queue<object> data);
    }
}
