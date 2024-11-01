using Unity.Netcode;
using UnityEngine;

public partial class Net_Mng : MonoBehaviour
{
    private void OnPlayerJoined()
    {
        if(NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    private void ChangeSceneForAllPlayers()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
