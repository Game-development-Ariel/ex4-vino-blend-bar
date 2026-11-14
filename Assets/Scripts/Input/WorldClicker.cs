using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Turns a mouse click or a touch into a click on a world object:
/// it finds the 2D collider under the pointer and calls its IClickable.
/// </summary>
public class WorldClicker : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;

    private void Update()
    {
        // Pointer is the mouse on a computer and the finger on a phone.
        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame)
        {
            return;
        }

        // A click on a UI button should not also click the world behind it.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 worldPoint = worldCamera.ScreenToWorldPoint(pointer.position.ReadValue());
        foreach (Collider2D hit in Physics2D.OverlapPointAll(worldPoint))
        {
            IClickable clickable = hit.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.OnClicked();
                return;
            }
        }
    }
}
