using UnityEngine;

public class NotePickup : MonoBehaviour
{
    [SerializeField] Sprite noteSprite;
    [Tooltip("Marcar SOLO en la última nota: al cerrarla se gana el juego")]
    [SerializeField] bool winOnClose = false;

    [Header("Efecto para que se note en el mapa")]
    [SerializeField] float bobHeight = 0.05f;
    [SerializeField] float bobSpeed = 2f;

    Vector3 startPos;
    bool taken;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (taken) return;
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (taken || !other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (NoteUI.Instance == null || NoteUI.Instance.IsOpen) return;

        taken = true;
        NoteUI.Instance.Show(noteSprite, OnClosed);
        gameObject.SetActive(false);
    }

    void OnClosed()
    {
        if (winOnClose && GameManager.Instance != null)
            GameManager.Instance.PlayerWon();
    }
}
