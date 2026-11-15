using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows how the evening is going, along the top of the screen: its name, the money so far,
/// and which guest this is. A fixed line of help at the bottom tells how to pour.
/// </summary>
public class BarHud : MonoBehaviour
{
    private const int CanvasSortingOrder = 10;
    private const int BigFontSize = 56;
    private const int GuestFontSize = 28;
    private const int HelpFontSize = 26;

    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);

    // Left to right along the top: the title, the money, which guest.
    private static readonly Vector2 TitlePosition = new Vector2(40f, -20f);
    private static readonly Vector2 TitleSize = new Vector2(260f, 70f);
    private static readonly Vector2 MoneyPosition = new Vector2(300f, -20f);
    private static readonly Vector2 MoneySize = new Vector2(200f, 70f);
    private static readonly Vector2 GuestPosition = new Vector2(520f, -24f);
    private static readonly Vector2 GuestSize = new Vector2(300f, 70f);
    private static readonly Vector2 HelpPosition = new Vector2(210f, 30f);
    private static readonly Vector2 HelpSize = new Vector2(900f, 40f);

    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private BarEvening evening;

    private Text moneyText;
    private Text guestText;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("HUD Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);

        Text title = UiFactory.CreateTitle(canvas.transform, "Title", TopLeft, TitleSize, BigFontSize, UiTheme.TextColor);
        title.rectTransform.anchoredPosition = TitlePosition;
        title.alignment = TextAnchor.MiddleLeft;
        title.text = settings.title;

        moneyText = UiFactory.CreateTitle(canvas.transform, "Money", TopLeft, MoneySize, BigFontSize, UiTheme.Hope);
        moneyText.rectTransform.anchoredPosition = MoneyPosition;
        moneyText.alignment = TextAnchor.MiddleLeft;
        moneyText.text = "$0";

        guestText = UiFactory.CreateText(canvas.transform, "Guest", TopLeft, GuestSize, GuestFontSize, TextAnchor.MiddleLeft);
        guestText.rectTransform.anchoredPosition = GuestPosition;
        guestText.color = UiTheme.Muted;

        Text help = UiFactory.CreateText(canvas.transform, "Help", BottomLeft, HelpSize, HelpFontSize, TextAnchor.MiddleLeft);
        help.rectTransform.anchoredPosition = HelpPosition;
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
        moneyText.text = "$" + evening.Score.Money;
    }
}
