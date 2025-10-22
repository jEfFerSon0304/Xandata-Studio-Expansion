using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class LobbyUIController : NetworkBehaviour
{
    public TMP_Text playersStatusText;
    public CharacterDataSO[] allCharacters;

    void Start()
    {
        InvokeRepeating(nameof(UpdateLobbyStatus), 0.5f, 0.5f);
    }

    void UpdateLobbyStatus()
    {
        var players = FindObjectsOfType<PlayerNetwork>();

        string display = "";
        foreach (var player in players.OrderBy(p => p.OwnerClientId))
        {
            string name = $"Player {player.OwnerClientId}";
            string charName = "None";

            if (player.SelectedCharacterIndex.Value >= 0 && player.SelectedCharacterIndex.Value < allCharacters.Length)
                charName = allCharacters[player.SelectedCharacterIndex.Value].characterName;

            string ready = player.IsReady.Value ? "✅ Ready" : "❌ Not Ready";
            display += $"{name}: {charName} - {ready}\n";
        }

        playersStatusText.text = display;
    }
}
