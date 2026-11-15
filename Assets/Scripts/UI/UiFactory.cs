using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Small helpers that create UI objects in code: a canvas, texts, panels and buttons.
/// Everything takes its fonts, colors and sprites from UiTheme, so all the screens look the same.
/// </summary>
public static class UiFactory
{
    // The UI is designed for this screen size, and the CanvasScaler scales it to the real screen.
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    // How every text looks: a little air between lines, and a soft dark shadow so light text reads over the scene.
    private const float TextLineSpacing = 1.1f;
    private static readonly Color TextShadowColor = new Color(0f, 0f, 0f, 0.7f);
    private static readonly Vector2 TextShadowOffset = new Vector2(2f, -2f);

    private static readonly Vector2 Middle = new Vector2(0.5f, 0.5f);

    // 0.5 = scale by both width and height equally, so it works in landscape and portrait.
    private const float WidthHeightMatch = 0.5f;

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = WidthHeightMatch;
        return canvas;
    }

    /// <summary>
    /// Creates a text in the typewriter font. The anchor is a point from (0,0) bottom left
    /// to (1,1) top right of the parent, and the text's box is placed at that point.
    /// </summary>
    public static Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, TextAnchor alignment)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Place(textObject.GetComponent<RectTransform>(), anchor, Vector2.zero, size);

        Text text = textObject.GetComponent<Text>();
        text.font = UiTheme.BodyFont;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = UiTheme.TextColor;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.lineSpacing = TextLineSpacing;

        // A soft dark shadow keeps light text readable over the bar.
        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = TextShadowColor;
        shadow.effectDistance = TextShadowOffset;
        return text;
    }

    /// <summary>Same as CreateText, in the serif font, centered. Used for titles, names and buttons.</summary>
    public static Text CreateTitle(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, Color color)
    {
        Text text = CreateText(parent, name, anchor, size, fontSize, TextAnchor.MiddleCenter);
        text.font = UiTheme.TitleFont;
        text.color = color;
        return text;
    }

    /// <summary>Creates an image. Without a sprite it is a plain colored rectangle.</summary>
    public static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false; // Only buttons and covers need to catch clicks and touches.
        return image;
    }

    /// <summary>
    /// A framed panel. "Sliced" keeps the frame's rounded corners the same size however big the panel is,
    /// and stretches only the middle.
    /// </summary>
    public static Image CreatePanel(Transform parent, string name, string spriteName)
    {
        Image panel = CreateImage(parent, name, UiTheme.Sprite(spriteName), Color.white);
        panel.type = Image.Type.Sliced;
        return panel;
    }

    /// <summary>A quiet button: a thin light frame and a serif label.</summary>
    public static Button CreateButton(Transform parent, string label, Vector2 anchor, Vector2 size, int fontSize, Color color)
    {
        Image background = CreatePanel(parent, label + " Button", "Button");
        background.color = color;
        background.raycastTarget = true; // Buttons must catch clicks and touches.
        Place(background.rectTransform, anchor, Vector2.zero, size);
        background.rectTransform.pivot = Middle;

        var button = background.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None; // ButtonFeel animates it instead of changing colors.
        background.gameObject.AddComponent<ButtonFeel>();

        Text text = CreateTitle(background.transform, "Label", Middle, size, fontSize, UiTheme.TextColor);
        text.rectTransform.pivot = Middle;
        text.text = label;
        return button;
    }

    /// <summary>
    /// Buttons need an EventSystem to receive clicks and touches.
    /// The project uses the new Input System, so it needs the matching input module.
    /// </summary>
    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null || Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    /// <summary>Puts a rect at a point of its parent. Anchor (0,0) is the bottom left, (1,1) the top right.</summary>
    public static void Place(RectTransform rect, Vector2 anchor, Vector2 offset, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
    }

    /// <summary>Makes a rect fill its parent.</summary>
    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
