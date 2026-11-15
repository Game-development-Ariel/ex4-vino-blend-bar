using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the guest at the counter: the name, what the guest says, and how much patience is left
/// (a bar that turns red as it empties).
/// </summary>
public class GuestPanel : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const int NameFontSize = 44;
    private const int SpeechFontSize = 28;
    private const float Width = 760f;
    private const float Margin = 30f;

    private static readonly Vector2 TopRight = new Vector2(1f, 1f);
    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 PanelPosition = new Vector2(-30f, -30f);
    private static readonly Vector2 PanelSize = new Vector2(Width, 330f);
    private static readonly Vector2 NamePosition = new Vector2(Margin, -16f);
    private static readonly Vector2 NameSize = new Vector2(Width - 2f * Margin, 60f);
    private static readonly Vector2 SpeechPosition = new Vector2(Margin, -84f);
    private static readonly Vector2 SpeechSize = new Vector2(Width - 2f * Margin, 230f);
    private static readonly Vector2 BarPosition = new Vector2(-30f, -372f);
    private static readonly Vector2 BarSize = new Vector2(Width, 16f);
    private static readonly Color Calm = new Color(0.45f, 0.7f, 0.45f);

    [SerializeField] private BarEvening evening;
    [SerializeField] private Patience patience;

    private Text nameText;
    private Text speechText;
    private Image patienceFill;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Guest Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        Image panel = UiFactory.CreatePanel(canvas.transform, "Guest Panel", "Panel");
        UiFactory.Place(panel.rectTransform, TopRight, PanelPosition, PanelSize);

        nameText = UiFactory.CreateTitle(panel.transform, "Name", TopLeft, NameSize, NameFontSize, UiTheme.Hope);
        nameText.rectTransform.anchoredPosition = NamePosition;
        nameText.alignment = TextAnchor.MiddleLeft;

        speechText = UiFactory.CreateText(panel.transform, "Speech", TopLeft, SpeechSize, SpeechFontSize, TextAnchor.UpperLeft);
        speechText.rectTransform.anchoredPosition = SpeechPosition;

        Image barBack = UiFactory.CreatePanel(canvas.transform, "Patience Bar", "BarBack");
        UiFactory.Place(barBack.rectTransform, TopRight, BarPosition, BarSize);
        patienceFill = UiFactory.CreateImage(barBack.transform, "Fill", UiTheme.Sprite("BarFill"), Calm);
        UiFactory.Stretch(patienceFill.rectTransform);
        patienceFill.type = Image.Type.Filled; // fillAmount from 0 to 1 cuts the picture from the right.
        patienceFill.fillMethod = Image.FillMethod.Horizontal;
    }

    private void OnEnable()
    {
        evening.GuestArrived += ShowOrder;
        evening.GuestServed += ShowReaction;
        evening.GuestGaveUp += ShowGaveUp;
    }

    private void OnDisable()
    {
        evening.GuestArrived -= ShowOrder;
        evening.GuestServed -= ShowReaction;
        evening.GuestGaveUp -= ShowGaveUp;
    }

    private void Update()
    {
        patienceFill.fillAmount = patience.Fraction;
        patienceFill.color = Color.Lerp(UiTheme.Danger, Calm, patience.Fraction);
    }

    private void ShowOrder(GuestOrder order)
    {
        nameText.text = order.GuestName;
        speechText.text = $"\"{order.Speech()}\"";
    }

    private void ShowReaction(ServeResult result)
    {
        speechText.text = $"\"{result.Reaction}\"";
    }

    private void ShowGaveUp()
    {
        speechText.text = "\"I've waited long enough. Good night.\"";
    }
}
