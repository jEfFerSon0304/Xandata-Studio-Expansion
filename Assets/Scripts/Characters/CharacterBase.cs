using Unity.Netcode;
using UnityEngine;

public class CharacterBase : NetworkBehaviour
{
    public CharacterDataSO characterData;
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        if (characterData != null)
        {
            animator = GetComponent<Animator>();
            if (animator && characterData.animatorController)
                animator.runtimeAnimatorController = characterData.animatorController;
        }
    }

    public void LoadData(CharacterDataSO data)
    {
        characterData = data;
    }
}
