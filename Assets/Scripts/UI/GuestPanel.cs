using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The guest at the counter: the name, what the guest says, and a patience bar that turns red as it empties.
/// </summary>
public class GuestPanel : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const float Width = 760f;

    private static readonly Vector2 TopRight = new Vector2(1f, 1f);
    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
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
        UiFactory.Place(panel.rectTransform, TopRight, new Vector2(-30f, -30f), new Vector2(Width, 330f));

        nameText = UiFactory.CreateTitle(panel.transform, "Name", TopLeft, new Vector2(Width - 60f, 60f), 44, UiTheme.Hope);
        nameText.rectTransform.anchoredPosition = new Vector2(30f, -16f);
        nameText.alignment = TextAnchor.MiddleLeft;

        speechText = UiFactory.CreateText(panel.transform, "Speech", TopLeft, new Vector2(Width - 60f, 230f), 28, TextAnchor.UpperLeft);
        speechText.rectTransform.anchoredPosition = new Vector2(30f, -84f);

        Image barBack = UiFactory.CreatePanel(canvas.transform, "Patience Bar", "BarBack");
        UiFactory.Place(barBack.rectTransform, TopRight, new Vector2(-30f, -372f), new Vector2(Width, 16f));
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
