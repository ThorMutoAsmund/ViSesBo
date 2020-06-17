using UnityEngine;

namespace VSB
{
    public class LookAtPlayer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private bool shouldFollowPlayer;
#pragma warning restore 0649

        void Update()
        {
            if (this.shouldFollowPlayer && Camera.main)
            {
                //this.transform.LookAt(this.mainCam);
                this.transform.LookAt(2 * this.transform.position - Camera.main.transform.position);
            }
        }
    }
}