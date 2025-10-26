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
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public override void OnNetworkSpawn()
    {
        // Reference the active DiceManager in scene
        manager = FindFirstObjectByType<DiceManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollDiceServerRpc()
    {
        if (!IsServer) return; // Prevent clients from forcing roll

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(Vector3.up * 6f, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * 15f, ForceMode.Impulse);

        StartCoroutine(CheckStoppedRoutine());
    }

    IEnumerator CheckStoppedRoutine()
    {
        yield return new WaitForSeconds(1f);

        // Wait until physics fully stops
        while (rb.linearVelocity.magnitude > 0.12f ||
               rb.angularVelocity.magnitude > 0.12f)
        {
            yield return null;
        }

        DetectValue();
    }

    void DetectValue()
    {
        // first attempt raycast detection
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            int val = DecodeFromCollider(hit.collider.name);
            if (val != -1)
            {
                manager.ReportResultServerRpc(val, OwnerClientId);
                return;
            }
        }

        // backup method using rotation
        int fallback = DecodeFromUpVector();
        manager.ReportResultServerRpc(fallback, OwnerClientId);
    }

    int DecodeFromCollider(string name)
    {
        if (name.Contains("1")) return 6;
        if (name.Contains("2")) return 5;
        if (name.Contains("3")) return 4;
        if (name.Contains("4")) return 3;
        if (name.Contains("5")) return 2;
        if (name.Contains("6")) return 1;
        return -1;
    }

    int DecodeFromUpVector()
    {
        Vector3 up = Vector3.up;
        float[] dots = {
            Vector3.Dot(transform.up, up),            // 1 face
            Vector3.Dot(-transform.up, up),           // 6 face
            Vector3.Dot(transform.forward, up),       // 2 face
            Vector3.Dot(-transform.forward, up),      // 5 face
            Vector3.Dot(transform.right, up),         // 3 face
            Vector3.Dot(-transform.right, up)         // 4 face
        };

        int idx = System.Array.IndexOf(dots, Mathf.Max(dots));
        return new int[] { 1, 6, 2, 5, 3, 4 }[idx];
    }
}
