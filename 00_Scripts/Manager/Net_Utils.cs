using System;
using Unity.Netcode;
using UnityEngine;

public class Net_Utils
{ 
    public static ulong LocalID()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public static void HostAndClientMethod(Action clientAction, Action HostAction)
    {
        if (NetworkManager.Singleton.IsClient) clientAction?.Invoke();
        else if (NetworkManager.Singleton.IsServer) HostAction?.Invoke();
    }

    public static bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject spawnedObject)
    {
        return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out spawnedObject);
    }

    public static bool IsClientCheck(ulong cleintID)
    {
        if (LocalID() == cleintID) return true;
        return false;
    }

    public static string RarityColor(Rarity rarity)
    {
        switch(rarity)
        {
            case Rarity.Common: return "<color=#A4A4A4>";
            case Rarity.UnCommon: return "<color=#79FF73>";
            case Rarity.Rare: return "<color=#6EE5FF>";
            case Rarity.Hero: return "<color=#FF9EF5>";
            case Rarity.Legendary: return "<color=#FFBA13>";
        }
        return "";
    }
}
