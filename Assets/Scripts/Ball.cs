using Networking;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class Ball : NetworkedObject
    {
        private float timeSinceCreation = 0f;

        private void Update()
        {
            this.timeSinceCreation += Time.deltaTime;
            if (this.transform.position.y < 0f || this.timeSinceCreation > 10f)
            {
                Object.Destroy(this.gameObject);
            }
        }
        [PunRPC]
        public override void RpcSyncState(bool isWriting, Queue<object> dataQueue)
        {
        }
    }
}