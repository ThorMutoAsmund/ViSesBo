using Networking;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class Ball : NetworkedObject
    {
        public ISyncedProperty<float> Size => this.size;

        [Synced] protected readonly SubscriptionField<float> size = SubscriptionField<float>.Create(nameof(size), 5f);

        [Synced] public Color BaseColor
        {
            get => this.baseColor;
            private set
            {
                if (value != this.baseColor)
                {
                    this.baseColor = value;
                    this.GetComponent<MeshRenderer>().material.color = this.BaseColor;
                    this.AutoSync.OnChange(nameof(this.BaseColor), this.BaseColor);
                }
            }
        }

        private float timeSinceCreation = 0f;
        private float lastSecond = 0f;
        private Color baseColor = Color.white;

        private void Start()
        {
            this.size.PropertyChanged += (sender, e) =>
            {
                this.transform.localScale = Vector3.one * this.size.Value;
            };
        }

        private void Update()
        {
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
                    this.BaseColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                }
            }
        }

        [PunRPC]
        public override void RpcFullStateSync(bool isWriting, Queue<object> dataQueue)
        {
            base.RpcFullStateSync(isWriting, dataQueue);
        }
    }
}