/// <summary>
/// Turns the numbers of a wine into the words people use for it: "dry", "crisp acidity", "a full body".
/// The guests and the tasting use the same words, and that is how the player learns to read an order.
/// Only functions, no state, so the class is static.
/// </summary>
public static class WineWords
{
    public static string Color(float redness)
    {
        if (redness > 0.75f)
        {
            return "red";
        }
        return redness < 0.25f ? "white" : "rose-style";
    }

    public static string Sweetness(float sweetness)
    {
        if (sweetness < 3f)
        {
            return "dry";
        }
        return sweetness < 4.5f ? "off-dry" : "slightly sweet";
    }

    public static string Acidity(float acidity)
    {
        if (acidity < 4.5f)
        {
            return "soft, gentle acidity";
        }
        if (acidity < 6.5f)
        {
            return "balanced acidity";
        }
        return acidity < 8f ? "fresh, crisp acidity" : "zingy, mouth-watering acidity";
    }

    public static string Tannin(float tannin)
    {
        if (tannin < 2.5f)
        {
            return "no tannins";
        }
        if (tannin < 4.5f)
        {
            return "silky tannins";
        }
        return tannin < 6.5f ? "medium tannins" : "firm, grippy tannins";
    }

    public static string Body(float body)
    {
        if (body < 4f)
        {
            return "a light body";
        }
        return body < 6.5f ? "a medium body" : "a full body";
    }

    public static string Alcohol(float alcohol)
    {
        if (alcohol < 11.5f)
        {
            return "light and low in alcohol";
        }
        if (alcohol < 13f)
        {
            return "moderate in alcohol";
        }
        return alcohol < 14f ? "fairly strong" : "warm and high in alcohol";
    }

    /// <summary>The main sentence about a wine, for example "a dry red with balanced acidity, silky tannins and a full body".</summary>
    public static string Sentence(WineProfile wine)
    {
        string kind = $"{Sweetness(wine.Sweetness)} {Color(wine.Redness)}";
        string article = kind.StartsWith("o") ? "an" : "a"; // "an off-dry", "a dry".
        string text = $"{article} {kind} with {Acidity(wine.Acidity)}";
        if (wine.Redness > 0.25f)
        {
            text += $", {Tannin(wine.Tannin)}";
        }
        return text + $" and {Body(wine.Body)}";
    }

    /// <summary>What the player learns from tasting the glass.</summary>
    public static string Taste(WineProfile wine)
    {
        if (wine.IsEmpty)
        {
            return "The glass is empty.";
        }

        string text = $"It tastes like {Sentence(wine)}. It feels {Alcohol(wine.Alcohol)}.";
        if (wine.Notes.Count > 0)
        {
            return text + $"\nNotes of {string.Join(", ", wine.Notes)}.";
        }
        return text + "\nNo aroma stands out.";
    }
}
