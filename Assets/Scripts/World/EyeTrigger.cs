using UnityEngine;

public class EyeTrigger : MonoBehaviour
{
    [SerializeField] EyeWatcher[] eyes;
    [Tooltip("Opcional: un sonido cuando los ojos se activan")]
    [SerializeField] AudioSource sfx;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        foreach (EyeWatcher eye in eyes)
            if (eye != null) eye.Activate();

        if (sfx != null) sfx.Play();
    }
}