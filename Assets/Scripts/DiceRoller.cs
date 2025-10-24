using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class DiceRoller : NetworkBehaviour
{
    private Rigidbody rb;
    private DiceManager manager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        manager = FindFirstObjectByType<DiceManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollDiceServerRpc(ServerRpcParams rpc = default)
    {
        if (!IsServer) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Add random physical impulse
        rb.AddForce(Vector3.up * 6f + Random.insideUnitSphere * 2f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.Impulse);

        StartCoroutine(CheckDiceStopped());
    }

    private IEnumerator CheckDiceStopped()
    {
        yield return new WaitForSeconds(2.5f);
        int value = GetDiceValue();
        manager.ReportDiceResultServerRpc(value, OwnerClientId);
    }

    private int GetDiceValue()
    {
        Vector3 up = transform.up;

        if (Vector3.Dot(up, Vector3.up) > 0.9f) return 1;
        if (Vector3.Dot(-transform.up, Vector3.up) > 0.9f) return 6;
        if (Vector3.Dot(transform.forward, Vector3.up) > 0.9f) return 5;
        if (Vector3.Dot(-transform.forward, Vector3.up) > 0.9f) return 2;
        if (Vector3.Dot(transform.right, Vector3.up) > 0.9f) return 3;
        return 4;
    }
}
