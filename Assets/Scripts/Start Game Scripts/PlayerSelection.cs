using Unity.Netcode;
using UnityEngine;

public class PlayerSelection : NetworkBehaviour
{
    public NetworkVariable<int> characterIndex = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    // Called by UI button
    public void ChangeCharacter()
    {
        if (!IsOwner) return;
        characterIndex.Value = (characterIndex.Value + 1) % CharacterSprites.Length;
    }

    public void SetReady()
    {
        if (!IsOwner) return;
        isReady.Value = true;
    }

    // Reference to character sprites (assign in inspector)
    public Sprite[] CharacterSprites;

    // UI references (assign in inspector)
    public UnityEngine.UI.Image characterImage;
    public UnityEngine.UI.Button changeButton, readyButton;

    void Start()
    {
        if (IsOwner)
        {
            changeButton.onClick.AddListener(ChangeCharacter);
            readyButton.onClick.AddListener(SetReady);
        }

        characterIndex.OnValueChanged += OnCharacterChanged;
        isReady.OnValueChanged += OnReadyChanged;
        OnCharacterChanged(0, characterIndex.Value); // Set initial image
    }

    void OnCharacterChanged(int oldIdx, int newIdx)
    {
        characterImage.sprite = CharacterSprites[newIdx];
    }

    void OnReadyChanged(bool oldReady, bool newReady)
    {
        // Update ready indicator (e.g. change button color)
        readyButton.GetComponentInChildren<UnityEngine.UI.Text>().text = newReady ? "Ready!" : "Ready";
    }
}
