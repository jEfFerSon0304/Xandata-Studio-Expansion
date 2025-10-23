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
    }

    public void RollDice()
    {
        if (rolling) return; // Prevent double roll
        rolling = true;

        // Reset any previous motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Random force & torque
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
        rb.AddForce(randomDir * 6f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 15f, ForceMode.Impulse);

        // Reset roll state after some time
        Invoke(nameof(ResetRollState), 3f);
    }

    void ResetRollState()
    {
        rolling = false;
    }

    // Detect which side faces up — simple directional logic
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

    // Optional: visualize when owner rolls (only for local player)
    void OnDrawGizmosSelected()
    {
        if (IsOwner)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }
    }
}
