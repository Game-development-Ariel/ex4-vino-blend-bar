using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Says in words what is in the glass, under the glass: "In the glass: 70% Syrah + 30% Grenache".
/// </summary>
public class GlassReadout : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const int FontSize = 28;

    private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
    private static readonly Vector2 Size = new Vector2(800f, 80f);
    private static readonly Vector2 Position = new Vector2(40f, 150f);

    [SerializeField] private Glass glass;

    private Text glassText;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Glass Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        glassText = UiFactory.CreateText(canvas.transform, "Glass", BottomCenter, Size, FontSize, TextAnchor.MiddleCenter);
        glassText.rectTransform.anchoredPosition = Position;
    }

    private void OnEnable()
    {
        glass.Changed += ShowGlass;
    }

    private void OnDisable()
    {
        glass.Changed -= ShowGlass;
    }

    private void ShowGlass()
    {
        Blend blend = glass.Blend;
        glassText.text = $"In the glass: {blend.Describe()}\nTotal {blend.Total}% of {Blend.Full}%";
    }
}
