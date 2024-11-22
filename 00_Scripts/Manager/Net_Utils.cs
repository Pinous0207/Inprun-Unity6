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
}
