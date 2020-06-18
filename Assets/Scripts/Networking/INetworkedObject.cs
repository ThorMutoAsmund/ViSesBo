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
}
