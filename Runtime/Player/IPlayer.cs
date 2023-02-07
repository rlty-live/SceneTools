using System;
using System.Collections.Generic;
using UnityEngine;

public static class AllPlayers
{
    public static IPlayer Me { get; internal set; }
    public static Dictionary<int, IPlayer> List;
    public static event Action<IPlayer> OnPlayerJoined, OnPlayerLeft;

    public static void NotifyPlayerJoined(IPlayer player, bool me=false)
    {
        JLogBase.Log("Creating player: " + player.ClientId, typeof(AllPlayers));
        if (me)
            Me = player;
        List[player.ClientId] = player;
        OnPlayerJoined?.Invoke(player);
    }

    public static void NotifyPlayerLeft(IPlayer player)
    {
        JLogBase.Log("Destroying player: " + player.ClientId, typeof(AllPlayers));
        List.Remove(player.ClientId);
        if (Me == player)
            Me = null;
        OnPlayerLeft?.Invoke(player);
    }
}

public interface IPlayer
{
    Transform Transform { get; }

    int ClientId { get; }

    bool IsAdmin { get; }
    uint AgoraUserId { get; set; }
    
    bool IsVoiceBoosted {get;set;}

    string Username { get; set; }
    string SkinDesc { get; set; }

    event Action<bool> OnTalkChanged;

    event Action<string> OnNameChanged;

    event Action<string> OnSkinRebuild;

    void Teleport(Vector3 position, Quaternion rotation);

    /// <summary>
    /// Modify speed of player (used by jumpers)
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    void SetAdditionalSpeed(Vector3 worldSpaceSpeedVector);

    /// <summary>
    /// Force vertical velocity (used by jumpers)
    /// </summary>
    /// <param name="verticalVelocity"></param>
    void SetVerticalVelocity(float verticalVelocity);
}

