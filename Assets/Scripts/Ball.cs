using Networking;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class Ball : NetworkedObject
    {
        private float timeSinceCreation = 0f;
        private float lastSecond = 0f;
        public ISyncedProperty<float> Size => this.size;

        [Synced] protected readonly SubscriptionField<float> size = SubscriptionField<float>.Create(nameof(size), 5f);

        private void Start()
        {
            this.size.PropertyChanged += (sender, e) =>
            {
                this.transform.localScale = Vector3.one * this.size.Value;
            };
        }

        protected override void Update()
        {
            base.Update();

            this.timeSinceCreation += Time.deltaTime;
            if (this.transform.position.y < 0f || this.timeSinceCreation > 10f)
            {
                if (this.photonView.IsMine)
                {
                    RpcDestroyObject();
                }
            }

            if (this.photonView.IsMine)
            {
                if (this.timeSinceCreation > this.lastSecond)
                {
                    this.lastSecond = (int)(this.timeSinceCreation) + 1f;
                    this.size.Value = Random.Range(3f, 7f);
                }
            }
        }

        [PunRPC]
        public override void RpcSyncState(bool isWriting, Queue<object> dataQueue)
        {
            base.RpcSyncState(isWriting, dataQueue);
        }
    }
}