using Networking;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSB
{
    public class Car : NetworkedObject
    {
#pragma warning disable 0649
        [SerializeField] private TextMesh numberPlateText;
#pragma warning restore 0649

        private Vector3 center;
        private float radius = 40f;
        private float t = 0f;
        private bool textSet;

        [Synced] protected readonly SubscriptionField<string> plateText = SubscriptionField<string>.Create(nameof(plateText));

        protected override void Awake()
        {
            base.Awake();

            this.plateText.PropertyChanged += (sender, e) =>
            {
                this.numberPlateText.text = plateText.Value;
            };
        }

        private void Start()
        {
            GameScene.Instance?.CorrectHeight(this.transform);
            this.center = this.transform.position.Add(x: - this.radius);
        }

        // Update is called once per frame
        private void Update()
        {
            if (this.photonView.IsMine)
            {
                this.t += Time.deltaTime * 0.2f;
                this.transform.position = center.Add(x: Mathf.Cos(t) * this.radius, z: Mathf.Sin(t) * this.radius);
                var e = this.transform.rotation.eulerAngles;
                this.transform.rotation = Quaternion.Euler(e.With(y: -t / (2 * Mathf.PI) * 360f));
                GameScene.Instance?.CorrectHeight(this.transform);

                if (!this.textSet)
                {
                    this.plateText.Value = $"{(char)Random.Range('A', 'Z')}{(char)Random.Range('A', 'Z')} {(char)Random.Range('0', '9')}{(char)Random.Range('0', '9')} {(char)Random.Range('0', '9')}{(char)Random.Range('0', '9')}{(char)Random.Range('0', '9')}";
                    this.textSet = true;
                }
            }
        }

        [PunRPC]
        public override void RpcFullStateSync(bool isWriting, Queue<object> dataQueue)
        {
            base.RpcFullStateSync(isWriting, dataQueue);
            //if (isWriting)
            //{
            //    Debug.Log("+++ Writing to queue");
            //    dataQueue.Enqueue(this.plateText.Value);
            //}
            //else
            //{
            //    Debug.Log("+++ Reading from queue");
            //    this.plateText.Value = dataQueue.Dequeue<string>();
            //}
        }
    }
}