using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public interface INetworkedObject
    {
        void RpcSyncState(bool isWriting, Queue<object> dataQueue);
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        // Serialize state
    //        stream.SendNext(this.IsCabinPressureIndicatorOn);
    //        stream.SendNext(this.IsExitSignOn);
    //    }
    //    else
    //    {
    //        // De-serialize state
    //        this.IsCabinPressureIndicatorOn = (bool)stream.ReceiveNext();
    //        this.IsExitSignOn = (bool)stream.ReceiveNext();
    //    }
    //}
}
