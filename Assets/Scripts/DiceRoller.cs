using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class DiceRoller : NetworkBehaviour
{
    private Rigidbody rb;
    private bool rolling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // freeze at start
    }

    public void RequestRoll()
    {
        if (rolling) return;
        if (IsOwner)
        {
            RollDiceServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollDiceServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (NetworkObject.OwnerClientId == senderId)
        {
            RollDice();
        }
    }

    private void RollDice()
    {
        if (rolling) return;
        rolling = true;

        rb.isKinematic = false; // unfreeze when rolling
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
        rb.AddForce(dir * 6f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 15f, ForceMode.Impulse);

        Invoke(nameof(ResetRoll), 2.5f);
    }

    private void ResetRoll()
    {
        rolling = false;
        rb.isKinematic = true; // freeze again when done
    }

    public int GetValue()
    {
        float upY = transform.up.y;
        float downY = -transform.up.y;
        float forwardY = transform.forward.y;
        float backY = -transform.forward.y;
        float rightY = transform.right.y;
        float leftY = -transform.right.y;

        float max = Mathf.Max(upY, downY, forwardY, backY, rightY, leftY);

        if (max == upY) return 1;
        if (max == downY) return 6;
        if (max == forwardY) return 2;
        if (max == backY) return 5;
        if (max == rightY) return 3;
        return 4;
    }
}
