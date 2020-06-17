using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class StationaryObject : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private bool correctParent;
#pragma warning restore 0649

        private void Start()
        {
            Scene.Instance?.CorrectHeight(this.transform, this.correctParent);

            this.gameObject.isStatic = true;
        }
    }
}