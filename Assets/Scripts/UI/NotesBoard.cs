using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The board on the right, under the guest: it shows what the player needs to read right now.
/// A tip when a guest arrives, the tasting notes after Taste, and the feedback after serving.
/// </summary>
public class NotesBoard : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const int FontSize = 26;
    private const float Width = 760f;
    private const float Margin = 30f;

    private static readonly Vector2 TopRight = new Vector2(1f, 1f);
    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 PanelPosition = new Vector2(-30f, -410f);
    private static readonly Vector2 PanelSize = new Vector2(Width, 440f);
    private static readonly Vector2 TextPosition = new Vector2(Margin, -24f);
    private static readonly Vector2 TextSize = new Vector2(Width - 2f * Margin, 400f);

    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private BarEvening evening;

    private Text notesText;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Notes Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        Image panel = UiFactory.CreatePanel(canvas.transform, "Notes Panel", "Panel");
        UiFactory.Place(panel.rectTransform, TopRight, PanelPosition, PanelSize);
        notesText = UiFactory.CreateText(panel.transform, "Notes", TopLeft, TextSize, FontSize, TextAnchor.UpperLeft);
        notesText.rectTransform.anchoredPosition = TextPosition;
    }

    private void OnEnable()
    {
        evening.GuestArrived += ShowTip;
        evening.Tasted += ShowTaste;
        evening.GuestServed += ShowResult;
        evening.GuestGaveUp += ShowGaveUp;
    }

    private void OnDisable()
    {
        evening.GuestArrived -= ShowTip;
        evening.Tasted -= ShowTaste;
        evening.GuestServed -= ShowResult;
        evening.GuestGaveUp -= ShowGaveUp;
    }

    private void ShowTip(GuestOrder order)
    {
        notesText.text = "Read the order, and look for the guest's words under the bottles.\n" +
                         "Taste tells you what is in the glass, but the guest gets impatient.";
    }

    // The same words as the guests' orders, so the player can compare them.
    private void ShowTaste(WineProfile wine)
    {
        notesText.text = WineWords.Taste(wine, settings.vocabulary);
    }

    private void ShowResult(ServeResult result)
    {
        string text = $"Match: {result.Score * 100f:0}%.  Paid ${result.Pay}.\n";
        if (result.Feedback.Count > 0)
        {
            text += string.Join("\n", result.Feedback) + "\n";
        }
        notesText.text = text + $"\nThe guest wanted: {evening.Order.Recipe}";
    }

    private void ShowGaveUp()
    {
        notesText.text = $"The guest left without paying.\n\nThe guest wanted: {evening.Order.Recipe}";
    }
}
