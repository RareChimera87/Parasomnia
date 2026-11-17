using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] DeathCause cause = DeathCause.General;
    public DeathCause Cause => cause;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.PlayerDied(cause);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
            GameManager.Instance.PlayerDied(cause);
    }
}