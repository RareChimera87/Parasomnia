using System.Collections;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [SerializeField] Rigidbody fallingObject;
    [SerializeField] float delay = 0.2f;
    [SerializeField] float resetTime = 4f;

    Vector3 startPos;
    Quaternion startRot;
    bool triggered;

    void Start()
    {
        startPos = fallingObject.transform.position;
        startRot = fallingObject.transform.rotation;
        fallingObject.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        triggered = true;

        yield return new WaitForSeconds(delay);
        fallingObject.isKinematic = false;

        yield return new WaitForSeconds(resetTime);
        fallingObject.linearVelocity = Vector3.zero;
        fallingObject.angularVelocity = Vector3.zero;
        fallingObject.isKinematic = true;
        fallingObject.transform.SetPositionAndRotation(startPos, startRot);

        triggered = false;
    }
}