using Networking;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class Car : NetworkedObject
    {
        private Vector3 center;
        private float radius = 40f;
        private float t = 0f;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Scene.Instance?.CorrectHeight(this.transform);
            this.center = this.transform.position.Add(x: - this.radius);
        }

        // Update is called once per frame
        void Update()
        {
            if (this.photonView.IsMine)
            {
                this.t += Time.deltaTime;
                this.transform.position = center.Add(x: Mathf.Cos(t) * this.radius, z: Mathf.Sin(t) * this.radius);
                var e = this.transform.rotation.eulerAngles;
                this.transform.rotation = Quaternion.Euler(e.With(y: -t / (2 * Mathf.PI) * 360f));
                Scene.Instance?.CorrectHeight(this.transform);
            }
        }
        //public override void SyncState(bool isWriting, Queue<object> data)
        //{
        //}
    }
}