using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [SerializeField] float amount = 40f;
    [SerializeField] float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Flashlight flashlight = other.GetComponent<Flashlight>();
        if (flashlight == null) return;

        flashlight.Recharge(amount);
        Destroy(gameObject);
    }
}
