using System.Collections.Generic;

namespace Networking
{
    public interface INetworkedObject
    {
        void RpcFullStateSync(bool isWriting, Queue<object> dataQueue);
    }
}
