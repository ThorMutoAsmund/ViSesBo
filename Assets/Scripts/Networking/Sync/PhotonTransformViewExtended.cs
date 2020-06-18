using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public enum TransformGlobality
    {
        Local,
        Global,
        Parent
    }

    public enum SerializationRateCompensation
    {
        None,
        Behind,
        Predictive,
        Photon,
        PhotonSmooth
    }

    public class PhotonTransformViewExtended : MonoBehaviour, IPunObservable
    {
#pragma warning disable 0649
        [Header("Synchronize Options")]
        [SerializeField] private TransformGlobality syncMode;
        [SerializeField] private SerializationRateCompensation serializationRateCompensation = SerializationRateCompensation.Behind;
        [SerializeField] private bool m_SynchronizePosition = true;
        [SerializeField] private bool m_SynchronizeRotation = true;
        [SerializeField] private bool m_SynchronizeScale = false;
#pragma warning restore 0649

        private float m_Distance;
        private float m_Angle;
        private bool m_firstTake = false;
        private PhotonView m_PhotonView;
        private bool receivedInitialValue;
        private float m_TimeSinceSync;
        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_LastPosition;
        private Vector3 m_StoredPosition;
        private Quaternion m_NetworkRotation;
        private Quaternion m_LastRotation;

        private Int64 currentParentHash = 0;
        private Transform currentParent = null;

        private TransformGlobality ActualSyncMode => this.syncMode == TransformGlobality.Parent ?
            TransformGlobality.Local :
            this.syncMode;

        public TransformGlobality SyncMode => this.syncMode;
        private Vector3 LocalOrGlobalPosition
        {
            get => this.ActualSyncMode == TransformGlobality.Local ? this.transform.localPosition : this.transform.position;
            set
            {
                if (this.ActualSyncMode == TransformGlobality.Local)
                {
                    this.transform.localPosition = value;
                }
                else
                {
                    this.transform.position = value;
                }
            }
        }

        private Quaternion LocalOrGlobalRotation
        {
            get => this.ActualSyncMode == TransformGlobality.Local ? this.transform.localRotation : this.transform.rotation;
            set
            {
                if (this.ActualSyncMode == TransformGlobality.Local)
                {
                    this.transform.localRotation = value;
                }
                else
                {
                    this.transform.rotation = value;
                }
            }
        }

        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        public void Start()
        {
            this.m_PhotonView = GetComponentInParent<PhotonView>();
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (this.m_PhotonView && !this.m_PhotonView.IsMine && this.receivedInitialValue)
            {
                this.m_TimeSinceSync += Time.deltaTime;
                Vector3 position;
                Quaternion rotation;
                float t = this.m_TimeSinceSync * PhotonNetwork.SerializationRate * 0.9F;
                switch (this.serializationRateCompensation)
                {
                    default:
                    case SerializationRateCompensation.None:
                        {
                            position = this.m_NetworkPosition;
                            rotation = this.m_NetworkRotation;
                            break;
                        }
                    case SerializationRateCompensation.Behind:
                        {

                            position = Vector3.Lerp(this.m_LastPosition, this.m_NetworkPosition, t);
                            rotation = Quaternion.Lerp(this.m_LastRotation, this.m_NetworkRotation, t);
                            break;
                        }
                    case SerializationRateCompensation.Predictive:
                        {
                            position = Vector3.Lerp(this.m_LastPosition, this.m_NetworkPosition + this.m_Direction, t);
                            rotation = Quaternion.Lerp(this.m_LastRotation, this.m_NetworkRotation, t);

                            break;
                        }
                    case SerializationRateCompensation.Photon:
                        {
                            position = Vector3.MoveTowards(this.LocalOrGlobalPosition, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                            rotation = Quaternion.RotateTowards(this.LocalOrGlobalRotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));

                            break;
                        }
                    case SerializationRateCompensation.PhotonSmooth:
                        {
                            float smoothingDelay = 5F;
                            position = Vector3.Lerp(this.LocalOrGlobalPosition, this.m_NetworkPosition, Time.deltaTime * smoothingDelay);
                            rotation = Quaternion.Lerp(this.LocalOrGlobalRotation, this.m_NetworkRotation, Time.deltaTime * smoothingDelay);
                            break;
                        }
                }

                if (this.ActualSyncMode == TransformGlobality.Local)
                {
                    if (this.m_SynchronizePosition)
                    {
                        transform.localPosition = position;
                    }
                    if (this.m_SynchronizeRotation)
                    {
                        transform.localRotation = rotation;
                    }
                }
                else
                {

                    if (this.m_SynchronizePosition)
                    {
                        transform.position = position;
                    }
                    if (this.m_SynchronizeRotation)
                    {
                        transform.rotation = rotation;
                    }
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //string NameWithPath(GameObject gameObject, string divider = "/")
            //{
            //    if (!gameObject)
            //    {
            //        return String.Empty;
            //    }
            //    var name = gameObject.name;
            //    var parent = gameObject.transform.parent;
            //    while (parent)
            //    {
            //        name = $"{parent.name}{divider}{name}";
            //        parent = parent.parent;
            //    }

            //    return name;
            //}

            if (stream.IsWriting)
            {
                if (this.syncMode == TransformGlobality.Parent)
                {
                    if (this.currentParent != this.transform.parent)
                    {
                        this.currentParent = this.transform.parent;
                        this.currentParentHash = NetworkedScene.NetworkSceneInstance.HashFromTransform(this.currentParent);
                        //Debug.Log($"New parent {(this.currentParent ? NameWithPath(this.currentParent.gameObject) : "none")} with hash {this.currentParentHash}");
                    }
                    stream.SendNext(this.currentParentHash);
                }
                if (this.m_SynchronizePosition)
                {
                    this.m_Direction = this.LocalOrGlobalPosition - this.m_StoredPosition;
                    this.m_StoredPosition = this.LocalOrGlobalPosition;

                    stream.SendNext(this.m_StoredPosition);
                    stream.SendNext(this.m_Direction);
                }

                if (this.m_SynchronizeRotation)
                {
                    stream.SendNext(this.LocalOrGlobalRotation);
                }

                if (this.m_SynchronizeScale)
                {
                    stream.SendNext(this.transform.localScale);
                }
            }
            else
            {
                this.receivedInitialValue = true;

                this.m_TimeSinceSync = 0F;

                if (this.syncMode == TransformGlobality.Parent)
                {
                    var parentHash = (Int64)stream.ReceiveNext();
                    if (this.currentParentHash != parentHash)
                    {
                        this.currentParent = NetworkedScene.NetworkSceneInstance.TransformFromHash(parentHash);
                        this.transform.SetParent(this.currentParent);
                        this.currentParentHash = parentHash;
                        //Debug.Log($"{NameWithPath(this.gameObject)} Parented to {(this.currentParent ? NameWithPath(this.currentParent.gameObject) : "none")} using hash {parentHash}");
                    }
                }
                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    if (this.m_firstTake)
                    {
                        this.LocalOrGlobalPosition = this.m_NetworkPosition;
                        this.m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        this.m_Distance = Vector3.Distance(this.LocalOrGlobalPosition, this.m_NetworkPosition);
                    }
                    this.m_LastPosition = this.LocalOrGlobalPosition;
                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (this.m_firstTake)
                    {
                        this.m_Angle = 0f;
                        this.LocalOrGlobalRotation = this.m_NetworkRotation;
                    }
                    else
                    {
                        this.m_Angle = Quaternion.Angle(this.LocalOrGlobalRotation, this.m_NetworkRotation);
                    }
                    this.m_LastRotation = this.LocalOrGlobalRotation;
                }

                if (this.m_SynchronizeScale)
                {
                    this.transform.localScale = (Vector3)stream.ReceiveNext();
                }

                if (this.m_firstTake)
                {
                    this.m_firstTake = false;
                }
            }
        }
    }
}