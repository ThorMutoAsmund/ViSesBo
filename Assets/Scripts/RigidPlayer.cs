using Networking;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSB
{
    public class RigidPlayer : MonoBehaviourPun
    {
#pragma warning disable 0649
        [SerializeField] private GameObject cameraAttachmentPoint;
#pragma warning restore 0649

        public GameObject CameraAttachmentPoint => this.cameraAttachmentPoint;

        private readonly float startHeightOverTerrain = 4f;
        private readonly float lookMargin = 10f;
        private readonly float rotationSpeed = 5f;
        private readonly float lookSpeed = 3f;
        private readonly float maxForwardVelocity = 30f;
        private readonly float maxBackwardsVelocity = 30f;
        private readonly float maxSidewaysVelocity = 20f;
        private readonly float forwardForceMultiplier = 5000f;
        private readonly float backwardsForceMultiplier = 5000f;
        private readonly float sidewaysForceMultiplier = 5000f;
        private readonly float jumpForceMultiplier = 1000f;
        private readonly float shootForceMultiplier = 15f;
        private readonly float terrainYMargin = 0.01f;

        private int actorNumber;
        private float halfHeight;
        private float lookDownMax;
        private float lookUpMax;
        private Vector3 euler;
        private new CapsuleCollider collider;
        private Rigidbody body;
        private bool isgrounded;

        private void Start()
        {
            this.lookDownMax = 90f - this.lookMargin;
            this.lookUpMax = 270f + this.lookMargin;

            this.body = GetComponent<Rigidbody>();
            this.collider = GetComponent<CapsuleCollider>();

            if (this.collider)
            {
                this.halfHeight = this.collider.height / 2f;
            }

            if (Scene.Instance?.Terrain)
            {
                var terrainHeight = Scene.Instance?.Terrain.SampleHeight(this.transform.position);
                this.transform.position = this.transform.position.With(y: terrainHeight + this.startHeightOverTerrain);
            }

            NetworkManager.Instance.WhenJoinedRoom(this, () =>
            {
                // Local player?
                if (this.actorNumber == 0)
                {
                    this.actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                }

                this.photonView.ViewID = this.actorNumber * PhotonNetwork.MAX_VIEW_IDS;
            });
        }

        private void Update()
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            float mouse = Input.GetAxis("Mouse X");
            if (mouse != 0f)
            {
                this.transform.RotateAround(this.transform.position, Vector3.up, mouse * this.rotationSpeed);
            }

            mouse = Input.GetAxis("Mouse Y");
            if (mouse != 0f)
            {
                this.cameraAttachmentPoint.transform.RotateAround(this.cameraAttachmentPoint.transform.position, this.cameraAttachmentPoint.transform.right, -mouse * this.lookSpeed);
                this.euler = this.cameraAttachmentPoint.transform.rotation.eulerAngles;
                if (this.euler.x > this.lookDownMax && this.euler.x < 180f)
                {
                    this.euler.x = this.lookDownMax;
                    this.cameraAttachmentPoint.transform.rotation = Quaternion.Euler(this.euler);
                }
                else if (this.euler.x < this.lookUpMax && this.euler.x > 180f)
                {
                    this.euler.x = this.lookUpMax;
                    this.cameraAttachmentPoint.transform.rotation = Quaternion.Euler(this.euler);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                SpawnBall();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                SpawnTree();
            }
        }

        private void OnCollisionEnter(Collision theCollision)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (theCollision.gameObject.GetComponent<Surface>())
            {
                this.isgrounded = true;
            }
        }

        private void OnCollisionExit(Collision theCollision)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (theCollision.gameObject.GetComponent<Surface>())
            {
                this.isgrounded = false;
            }
        }

        private void FixedUpdate()
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            var localVel = this.transform.InverseTransformDirection(this.body.velocity);

            if (Input.GetKey(KeyCode.W))
            {
                if (localVel.z < this.maxForwardVelocity)
                {
                    this.body.AddForce(this.cameraAttachmentPoint.transform.forward * this.forwardForceMultiplier);
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (localVel.z > -this.maxBackwardsVelocity)
                {
                    this.body.AddForce(this.cameraAttachmentPoint.transform.forward * -this.backwardsForceMultiplier);
                }
            }

            if (Input.GetKey(KeyCode.A))
            {
                if (localVel.x > -this.maxSidewaysVelocity)
                {
                    this.body.AddForce(this.cameraAttachmentPoint.transform.right * -this.sidewaysForceMultiplier);
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (localVel.x < this.maxSidewaysVelocity)
                {
                    this.body.AddForce(this.cameraAttachmentPoint.transform.right * this.sidewaysForceMultiplier);
                }
            }
            if (this.isgrounded)
            {

                if (Input.GetKey(KeyCode.Space))
                {
                    this.isgrounded = false;
                    this.body.AddForce(new Vector3(0, this.jumpForceMultiplier, 0), ForceMode.Impulse);
                }
            }

            if (Scene.Instance?.Terrain)
            {
                var terrainHeight = Scene.Instance?.Terrain.SampleHeight(this.transform.position);
                if (this.transform.position.y - this.halfHeight < terrainHeight - this.terrainYMargin)
                {
                    this.transform.position = this.transform.position.With(y: terrainHeight + this.halfHeight);
                }
            }
        }

        public void SetActorNumber(int actorNumber)
        {
            this.actorNumber = actorNumber;
        }

        private void SpawnBall()
        {
            if (Scene.Instance)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    Scene.Instance.InstantiateSceneObject("SoccerBall",                            
                        position: this.transform.position + this.cameraAttachmentPoint.transform.forward);
                }
                else
                {
                    var gameObject = Scene.Instance.InstantiateLocalObject("SoccerBall",
                            actorNumber: this.actorNumber,
                            position: this.transform.position + this.cameraAttachmentPoint.transform.forward);

                    var rigidBody = gameObject?.GetComponent<Rigidbody>();
                    if (rigidBody)
                    {
                        rigidBody.AddForce((this.cameraAttachmentPoint.transform.forward + this.cameraAttachmentPoint.transform.up * 0.5f
                            + this.cameraAttachmentPoint.transform.up * Random.Range(-0.1f, 0.1f) + this.cameraAttachmentPoint.transform.right * Random.Range(-0.1f, 0.1f)) * this.shootForceMultiplier, ForceMode.Impulse);
                        rigidBody.AddTorque(new Vector3(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f), 0f));
                    }
                }

            }
        }

        private void SpawnTree()
        {
            if (Scene.Instance)
            {
                Scene.Instance.InstantiateStatic("Tree", position: this.transform.position + this.cameraAttachmentPoint.transform.forward);
            }
        }
    }
}