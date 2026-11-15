using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The top-left corner of the bar screen: the evening's name, the money, and which guest is at the counter.
/// </summary>
public class BarHud : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;

    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);

    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private BarEvening evening;

    private Text moneyText;
    private Text guestText;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("HUD Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        Text title = UiFactory.CreateTitle(canvas.transform, "Title", TopLeft, new Vector2(260f, 70f), 56, UiTheme.TextColor);
        title.rectTransform.anchoredPosition = new Vector2(40f, -20f);
        title.alignment = TextAnchor.MiddleLeft;
        title.text = settings.title;

        moneyText = UiFactory.CreateTitle(canvas.transform, "Money", TopLeft, new Vector2(200f, 70f), 56, UiTheme.Hope);
        moneyText.rectTransform.anchoredPosition = new Vector2(300f, -20f);
        moneyText.alignment = TextAnchor.MiddleLeft;
        moneyText.text = "$0";

        guestText = UiFactory.CreateText(canvas.transform, "Guest", TopLeft, new Vector2(300f, 70f), 28, TextAnchor.MiddleLeft);
        guestText.rectTransform.anchoredPosition = new Vector2(520f, -24f);
        guestText.color = UiTheme.Muted;

        Text help = UiFactory.CreateText(canvas.transform, "Help", BottomLeft, new Vector2(900f, 40f), 26, TextAnchor.MiddleLeft);
        help.rectTransform.anchoredPosition = new Vector2(210f, 30f);
        help.color = UiTheme.Muted;
        help.text = "Click a bottle to pour some of its wine into the glass.";
    }

    private void OnEnable()
    {
        evening.GuestArrived += ShowGuest;
        evening.GuestServed += ShowMoney;
    }

    private void OnDisable()
    {
        evening.GuestArrived -= ShowGuest;
        evening.GuestServed -= ShowMoney;
    }

    private void ShowGuest(GuestOrder order)
    {
        guestText.text = $"Guest {evening.GuestNumber} of {evening.GuestCount}";
    }

    private void ShowMoney(ServeResult result)
    {
        moneyText.text = "$" + evening.Money;
    }
}
