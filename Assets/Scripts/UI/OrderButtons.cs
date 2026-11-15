using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The buttons of the bar: Taste, Empty glass and Serve while a guest is ordering,
/// and Next guest (See results after the last guest) in between. Serve works only with a full glass.
/// </summary>
public class OrderButtons : MonoBehaviour
{
    private const int FontSize = 34;
    private const float ButtonHeight = 76f;
    private const float ButtonGap = 20f;
    private const float ButtonsY = 80f;
    private const float OrderButtonWidth = 240f;
    private const float NextButtonWidth = 400f;

    // The x of each button's middle, from the right edge of the screen.
    private const float TasteX = -660f;
    private const float EmptyX = -410f;
    private const float ServeX = -160f;
    private const float NextX = -410f;

    // How see-through Serve is while the glass is not full yet.
    private const float DisabledAlpha = 0.35f;

    private static readonly Vector2 BottomRight = new Vector2(1f, 0f);

    [SerializeField] private BarEvening evening;
    [SerializeField] private Glass glass;

    private GameObject orderButtons;
    private CanvasGroup serveGroup;
    private GameObject nextButton;
    private Text nextLabel;

    private void Awake()
    {
        UiFactory.EnsureEventSystem();
        Canvas canvas = UiFactory.CreateCanvas("Buttons Canvas", UiTheme.HudLayer);
        canvas.transform.SetParent(transform, false);

        orderButtons = new GameObject("Order Buttons", typeof(RectTransform));
        orderButtons.transform.SetParent(canvas.transform, false);
        UiFactory.Stretch((RectTransform)orderButtons.transform);
        AddButton(orderButtons.transform, "Taste", TasteX, OrderButtonWidth).onClick.AddListener(evening.Taste);
        AddButton(orderButtons.transform, "Empty glass", EmptyX, OrderButtonWidth).onClick.AddListener(glass.Empty);
        Button serve = AddButton(orderButtons.transform, "Serve", ServeX, OrderButtonWidth);
        serve.onClick.AddListener(evening.Serve);
        serveGroup = serve.gameObject.AddComponent<CanvasGroup>();

        Button next = AddButton(canvas.transform, "Next guest", NextX, NextButtonWidth);
        next.onClick.AddListener(evening.NextGuest);
        nextButton = next.gameObject;
        nextLabel = next.GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        glass.Changed += UpdateServe;
        evening.GuestArrived += ShowOrderButtons;
        evening.GuestServed += OnGuestServed;
        evening.GuestGaveUp += OnGuestGaveUp;
        evening.EveningEnded += HideNextButton;
    }

    private void OnDisable()
    {
        glass.Changed -= UpdateServe;
        evening.GuestArrived -= ShowOrderButtons;
        evening.GuestServed -= OnGuestServed;
        evening.GuestGaveUp -= OnGuestGaveUp;
        evening.EveningEnded -= HideNextButton;
    }

    private Button AddButton(Transform parent, string label, float x, float width)
    {
        Button button = UiFactory.CreateButton(parent, label, BottomRight, new Vector2(width - ButtonGap, ButtonHeight), FontSize, Color.white);
        ((RectTransform)button.transform).anchoredPosition = new Vector2(x, ButtonsY);
        return button;
    }

    // Serving is possible only with a full glass.
    private void UpdateServe()
    {
        bool full = glass.Blend.IsFull;
        serveGroup.interactable = full;
        serveGroup.alpha = full ? 1f : DisabledAlpha;
    }

    private void ShowOrderButtons(GuestOrder order)
    {
        ShowButtons(true);
    }

    private void OnGuestServed(ServeResult result)
    {
        ShowButtons(false);
    }

    private void OnGuestGaveUp()
    {
        ShowButtons(false);
    }

    private void ShowButtons(bool ordering)
    {
        orderButtons.SetActive(ordering);
        nextButton.SetActive(!ordering);
        nextLabel.text = evening.GuestNumber < evening.GuestCount ? "Next guest" : "See results";
    }

    // The end screen covers the bar, so the button goes away, and it is no longer selected for the keyboard.
    private void HideNextButton(float stars, bool passed)
    {
        nextButton.SetActive(false);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
