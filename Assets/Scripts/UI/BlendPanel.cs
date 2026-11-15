using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The blending station: what is in the glass, the tasting notes and the feedback after serving,
/// and the buttons Taste, Empty glass and Serve. Between guests it shows "Next guest" instead.
/// </summary>
public class BlendPanel : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const float Width = 760f;

    private static readonly Vector2 TopRight = new Vector2(1f, 1f);
    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 BottomRight = new Vector2(1f, 0f);
    private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);

    [SerializeField] private BarEvening evening;
    [SerializeField] private Glass glass;

    private Text glassText;
    private Text notesText;
    private GameObject orderButtons;
    private CanvasGroup serveGroup;
    private GameObject nextButton;

    private void Awake()
    {
        UiFactory.EnsureEventSystem();
        Canvas canvas = UiFactory.CreateCanvas("Blend Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        glassText = UiFactory.CreateText(canvas.transform, "Glass", BottomCenter, new Vector2(800f, 80f), 28, TextAnchor.MiddleCenter);
        glassText.rectTransform.anchoredPosition = new Vector2(40f, 150f);

        Image panel = UiFactory.CreatePanel(canvas.transform, "Notes Panel", "Panel");
        UiFactory.Place(panel.rectTransform, TopRight, new Vector2(-30f, -410f), new Vector2(Width, 440f));
        notesText = UiFactory.CreateText(panel.transform, "Notes", TopLeft, new Vector2(Width - 60f, 400f), 26, TextAnchor.UpperLeft);
        notesText.rectTransform.anchoredPosition = new Vector2(30f, -24f);

        orderButtons = new GameObject("Order Buttons", typeof(RectTransform));
        orderButtons.transform.SetParent(canvas.transform, false);
        UiFactory.Stretch((RectTransform)orderButtons.transform);
        AddButton(orderButtons.transform, "Taste", -660f, 240f).onClick.AddListener(evening.Taste);
        AddButton(orderButtons.transform, "Empty glass", -410f, 240f).onClick.AddListener(glass.Empty);
        Button serve = AddButton(orderButtons.transform, "Serve", -160f, 240f);
        serve.onClick.AddListener(evening.Serve);
        serveGroup = serve.gameObject.AddComponent<CanvasGroup>();

        Button next = AddButton(canvas.transform, "Next guest", -410f, 400f);
        next.onClick.AddListener(evening.NextGuest);
        nextButton = next.gameObject;
    }

    private void OnEnable()
    {
        glass.Changed += ShowGlass;
        evening.GuestArrived += ShowNewOrder;
        evening.Tasted += ShowNotes;
        evening.GuestServed += ShowResult;
        evening.GuestGaveUp += ShowGaveUp;
    }

    private void OnDisable()
    {
        glass.Changed -= ShowGlass;
        evening.GuestArrived -= ShowNewOrder;
        evening.Tasted -= ShowNotes;
        evening.GuestServed -= ShowResult;
        evening.GuestGaveUp -= ShowGaveUp;
    }

    private Button AddButton(Transform parent, string label, float x, float width)
    {
        Button button = UiFactory.CreateButton(parent, label, BottomRight, new Vector2(width - 20f, 76f), 34, Color.white);
        ((RectTransform)button.transform).anchoredPosition = new Vector2(x, 80f);
        return button;
    }

    private void ShowGlass()
    {
        Blend blend = glass.Blend;
        glassText.text = $"In the glass: {blend.Describe()}\nTotal {blend.Total}% of {Blend.Full}%";

        // Serving is possible only with a full glass.
        serveGroup.interactable = blend.IsFull;
        serveGroup.alpha = blend.IsFull ? 1f : 0.35f;
    }

    private void ShowNewOrder(GuestOrder order)
    {
        ShowButtons(true);
        ShowNotes("Read the order, and look for the guest's words under the bottles.\nTaste tells you what is in the glass, but the guest gets impatient.");
    }

    private void ShowNotes(string notes)
    {
        notesText.text = notes;
    }

    private void ShowResult(ServeResult result)
    {
        string text = $"Match: {result.Score * 100f:0}%.  Paid ${result.Pay}.\n";
        if (result.Feedback.Count > 0)
        {
            text += string.Join("\n", result.Feedback) + "\n";
        }
        ShowNotes(text + $"\nThe guest wanted: {evening.Order.Recipe}");
        ShowButtons(false);
    }

    private void ShowGaveUp()
    {
        ShowNotes($"The guest left without paying.\n\nThe guest wanted: {evening.Order.Recipe}");
        ShowButtons(false);
    }

    private void ShowButtons(bool ordering)
    {
        orderButtons.SetActive(ordering);
        nextButton.SetActive(!ordering);
    }
}
