using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class HostGameController : MonoBehaviour
{
    public void StartGame()
    {
        // Only host can start
        if (!NetworkManager.Singleton.IsHost) return;

        foreach (var player in CharacterSelectionStore.Instance.players.Values)
        {
            if (!player.isReady)
            {
                Debug.Log("Not all players are ready!");
                return;
            }
        }

        // All players ready → start game scene
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
