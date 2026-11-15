using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gives a button its feel under the pointer: it grows a little under the mouse,
/// shrinks and clicks when pressed. Unity calls these methods by itself, because the class implements the pointer interfaces.
/// </summary>
public class ButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const float ClickVolume = 0.6f;

    private const float HoverScale = 1.04f;
    private const float PressedScale = 0.97f;

    // How fast the size follows the goal. Higher = snappier.
    private const float Speed = 18f;

    private float goalScale = 1f;

    private void Update()
    {
        float scale = Mathf.Lerp(transform.localScale.x, goalScale, Time.unscaledDeltaTime * Speed);
        transform.localScale = Vector3.one * scale;
    }

    // A button hidden while the mouse is on it (like Serve) would come back still big.
    private void OnDisable()
    {
        goalScale = 1f;
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        goalScale = HoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        goalScale = 1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        goalScale = PressedScale;
        Sfx.Play("Click", ClickVolume);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        goalScale = HoverScale;
    }
}
