using UnityEngine;

/// <summary>
/// The look of the game's UI in one place: fonts, colors and sprites.
/// The bar is dim and warm, so the look is quiet: dark panels, cream text, and amber for what matters.
/// The files are in Assets/Resources, so code can load them by name without Inspector fields.
/// </summary>
public static class UiTheme
{
    public static readonly Color TextColor = new Color(0.92f, 0.89f, 0.84f);
    public static readonly Color Muted = new Color(0.6f, 0.58f, 0.55f);
    public static readonly Color Hope = new Color(1f, 0.78f, 0.5f);
    public static readonly Color Danger = new Color(0.72f, 0.16f, 0.13f);

    /// <summary>An old book serif, for titles, names and buttons.</summary>
    public static Font TitleFont => Resources.Load<Font>("Fonts/CrimsonText");

    /// <summary>A typewriter font, for the rest of the text: orders, tasting notes and help.</summary>
    public static Font BodyFont => Resources.Load<Font>("Fonts/CourierPrime");

    /// <summary>A sprite from Assets/Resources/UI, for example "Panel".</summary>
    public static Sprite Sprite(string name)
    {
        return Resources.Load<Sprite>("UI/" + name);
    }
}
