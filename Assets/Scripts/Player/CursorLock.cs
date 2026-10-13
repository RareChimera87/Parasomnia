using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UnlockCursor();
        else if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}