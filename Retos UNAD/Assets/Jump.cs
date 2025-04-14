using UnityEngine;

public class JumpingObject : MonoBehaviour, IInteractable
{
    public float jumpForce = 5f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact()
    {
        if (rb != null && Mathf.Approximately(rb.linearVelocity.y, 0))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}