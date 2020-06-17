using Photon.Pun;
using UnityEngine;

namespace VSB
{
    public class PhotonViewOwnerDebug : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private PhotonView photonView;
#pragma warning restore 0649

        private TextMesh textMesh;
        private void Start()
        {
            this.textMesh = GetComponent<TextMesh>();
            this.enabled = this.textMesh && this.photonView;
        }
        private void Update()
        {
            this.textMesh.text = $"Owner {(this.photonView.OwnerActorNr == 0 ? "none" : $"{this.photonView.OwnerActorNr}")}";
        }
    }
}
