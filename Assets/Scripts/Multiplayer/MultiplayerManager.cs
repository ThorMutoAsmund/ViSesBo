// ----------------------------------------------------------------------------
// <copyright file="MultiplayerManager.cs" company="Bolverk XR / Bolverk Games">
//   Framework for using Photon in a multiplayer scenario - Copyright (C) 2020 Bolverk XR / Bolverk Games
// </copyright>
// <version>2.1</version>
// <author>mark@bolverkgames.com  thor@bolverkxr.com</author>
// ----------------------------------------------------------------------------

using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace Multiplayer
{
    public sealed class MultiplayerManager : MonoBehaviourPunCallbacks
    {
        private const int MultiplayerSessionViewID = 1;

        public static MultiplayerManager Instance { get; private set; }

        private MultiplayerSession CurrentSession { get; set; }

        /// <summary>
        /// Override this to create a customized MultiplayerSession
        /// </summary>
        public static Func<int, MultiplayerSession> InstantiateMultiplayerSession = viewID => GameObjectExtension.InstantiateWithView<MultiplayerSession>(viewID);

        public static void StartMultiplayer(int sendRate = 40, int serializationRate = 40, float minimalTimeScaleToDispatchInFixedUpdate = -1f)
        {
            if (Instance == null)
            {
                Instance = GameObjectExtension.Instantiate<MultiplayerManager>(dontDestroyOnLoad: true);

                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.SendRate = sendRate;
                PhotonNetwork.SerializationRate = serializationRate;
                PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = minimalTimeScaleToDispatchInFixedUpdate;

                //PhotonNetwork.PrecisionForFloatSynchronization = 0.002f;
                //PhotonNetwork.PrecisionForQuaternionSynchronization = 0.2f;
                //PhotonNetwork.PrecisionForVectorSynchronization = 0.0000099f;                       
            }
        }

        public static void EndMultiplayer()
        {
            if (Instance != null)
            {
                PhotonNetwork.Disconnect();
            }
        }

        private void Start()
        {
            MultiplayerCustomTypes.Register();
        }

        public static void ChangeScene(string scene, bool force = false)
        {
            if (Instance)
            {
                if (Instance.CurrentSession != null)
                {
                    if (force)
                    {
                        Instance.CurrentSession.ForceChangeScene(scene);
                    }
                    else
                    {
                        Instance.CurrentSession.ChangeScene(scene);
                    }
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
            }
        }


        public override void OnCreatedRoom()
        {
            this.StartSession();
        }

        public override void OnJoinedRoom()
        {
            this.StartSession();
        }

        public override void OnLeftRoom()
        {
            this.EndSession();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            this.EndSession();

            Destroy(Instance.gameObject);
            
            Instance = null;
        }

        private void StartSession()
        {
            if (this.CurrentSession == null)
            {
                var viewID = MultiplayerSessionViewID;

                this.CurrentSession = InstantiateMultiplayerSession(viewID);

                DontDestroyOnLoad(this.CurrentSession.gameObject);
            }
        }

        private void EndSession()
        {
            if (this.CurrentSession != null)
            {
                Destroy(this.CurrentSession.gameObject);
                this.CurrentSession = null;
            }
        }
    }
}