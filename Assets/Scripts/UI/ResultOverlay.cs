using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates the simple end-of-round screen: a dark cover, a title, a message,
/// and a "Play again" button.
/// </summary>
public static class ResultOverlay
{
    // Above everything else in the scene, below the scene fader.
    private const int SortingOrder = 60;

    private const int TitleFontSize = 100;
    private const int MessageFontSize = 36;
    private const int ButtonFontSize = 40;

    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
    private static readonly Color CoverColor = new Color(0.03f, 0.03f, 0.04f, 0.9f);

    // Top to bottom, from the middle of the screen.
    private static readonly Vector2 TitlePosition = new Vector2(0f, 220f);
    private static readonly Vector2 TitleSize = new Vector2(1500f, 150f);
    private static readonly Vector2 MessageSize = new Vector2(1300f, 300f);
    private static readonly Vector2 ButtonPosition = new Vector2(0f, -260f);
    private static readonly Vector2 ButtonSize = new Vector2(380f, 84f);

    public static void Show(string title, string message, Color titleColor)
    {
        UiFactory.EnsureEventSystem();
        Transform canvas = UiFactory.CreateCanvas("Result Canvas", SortingOrder).transform;

        Image cover = UiFactory.CreateImage(canvas, "Cover", null, CoverColor);
        UiFactory.Stretch(cover.rectTransform);
        cover.raycastTarget = true; // Blocks clicks on the game behind it.

        Text titleText = UiFactory.CreateTitle(canvas, "Title", Center, TitleSize, TitleFontSize, titleColor);
        titleText.rectTransform.pivot = Center;
        titleText.rectTransform.anchoredPosition = TitlePosition;
        titleText.text = title;

        Text messageText = UiFactory.CreateText(canvas, "Message", Center, MessageSize, MessageFontSize, TextAnchor.MiddleCenter);
        messageText.rectTransform.pivot = Center;
        messageText.text = message;

        string thisScene = SceneManager.GetActiveScene().name;
        Button again = UiFactory.CreateButton(canvas, "Play again", Center, ButtonSize, ButtonFontSize, Color.white);
        ((RectTransform)again.transform).anchoredPosition = ButtonPosition;
        again.onClick.AddListener(() => ScreenFader.FadeAndLoad(thisScene));
    }
}
