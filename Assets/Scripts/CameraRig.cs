using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VSB
{
    public class CameraRig : MonoBehaviour
    {
        private GameObject attachedTo;
        public void AttachTo(GameObject objectToAttachTo)
        {
            this.attachedTo = objectToAttachTo;
        }

        private void Update()
        {
            if (this.attachedTo)
            {
                this.transform.position = this.attachedTo.transform.position;
                this.transform.rotation = this.attachedTo.transform.rotation;
            }
        }
    }
}
