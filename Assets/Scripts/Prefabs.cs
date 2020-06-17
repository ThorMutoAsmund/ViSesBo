using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VSB
{
    public class Prefabs : MonoBehaviour
    {
        public GameObject player;
        public GameObject soccerBall;
        public GameObject Player => this.player;
        public GameObject SoccerBall => this.soccerBall;

        public static Prefabs Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
