using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Multiplayer
{
    //public class MultiplayerPlayerManager : MultiplayerGameObjectPunCallbacks
    //{
    //    private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

    //    public static Action<int> CustomSpawnPlayer = actorNumber => { };
    //    public static Action<int> CustomDespawnPlayer = actorNumber => { };

    //    protected override void Awake()
    //    {
    //        base.Awake();

    //        MultiplayerSessionScene.OnMultiplayerSessionSceneReady += this.OnSceneReady;
    //    }

    //    private void OnSceneReady()
    //    {
    //        foreach (var pair in PhotonNetwork.CurrentRoom.Players)
    //        {
    //            this.SpawnPlayer(pair.Value);
    //        }
    //    }

    //    public override void OnPlayerEnteredRoom(Player newPlayer)
    //    {
    //        this.SpawnPlayer(newPlayer);
    //    }

    //    public override void OnPlayerLeftRoom(Player otherPlayer)
    //    {
    //        this.DespawnPlayer(otherPlayer);
    //    }

    //    private void SpawnPlayer(Player player)
    //    {
    //        if (this.players.ContainsKey(player.ActorNumber) == false)
    //        {
    //            this.players.Add(player.ActorNumber, player);

    //            // Custom spawn player
    //            MultiplayerPlayerManager.CustomSpawnPlayer(player.ActorNumber);
    //        }
    //    }

    //    //Photon auto destroyes the players Prefab
    //    private void DespawnPlayer(Player otherPlayer)
    //    {
    //        if (this.players.ContainsKey(otherPlayer.ActorNumber))
    //        {
    //            this.players.Remove(otherPlayer.ActorNumber);

    //            // Custom despawn player
    //            MultiplayerPlayerManager.CustomDespawnPlayer(otherPlayer.ActorNumber);
    //        }
    //    }

    //    protected override void FullStateSync(bool isWriting, Queue<object> data)
    //    {
    //    }

    //    protected override void OnDestroy()
    //    {
    //        base.OnDestroy();

    //        MultiplayerSessionScene.OnMultiplayerSessionSceneReady -= this.OnSceneReady;
    //    }
    //}
}