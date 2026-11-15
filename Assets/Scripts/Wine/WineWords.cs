/// <summary>
/// Turns the numbers of a wine into the words people use for it: "dry", "crisp acidity", "a full body".
/// The guests and the tasting use the same words, and that is how the player learns to read an order.
/// Where each word starts is in a WineVocabulary asset. Only functions, no state, so the class is static.
/// </summary>
public static class WineWords
{
    public static string Color(float redness, WineVocabulary scale)
    {
        if (redness > scale.redAbove)
        {
            return "red";
        }
        return redness < scale.whiteBelow ? "white" : "rose-style";
    }

    public static string Sweetness(float sweetness, WineVocabulary scale)
    {
        if (sweetness < scale.dryBelow)
        {
            return "dry";
        }
        return sweetness < scale.offDryBelow ? "off-dry" : "slightly sweet";
    }

    public static string Acidity(float acidity, WineVocabulary scale)
    {
        if (acidity < scale.softAcidityBelow)
        {
            return "soft, gentle acidity";
        }
        if (acidity < scale.balancedAcidityBelow)
        {
            return "balanced acidity";
        }
        return acidity < scale.crispAcidityBelow ? "fresh, crisp acidity" : "zingy, mouth-watering acidity";
    }

    public static string Tannin(float tannin, WineVocabulary scale)
    {
        if (tannin < scale.noTanninsBelow)
        {
            return "no tannins";
        }
        if (tannin < scale.silkyTanninsBelow)
        {
            return "silky tannins";
        }
        return tannin < scale.mediumTanninsBelow ? "medium tannins" : "firm, grippy tannins";
    }

    public static string Body(float body, WineVocabulary scale)
    {
        if (body < scale.lightBodyBelow)
        {
            return "a light body";
        }
        return body < scale.mediumBodyBelow ? "a medium body" : "a full body";
    }

    public static string Alcohol(float alcohol, WineVocabulary scale)
    {
        if (alcohol < scale.lowAlcoholBelow)
        {
            return "light and low in alcohol";
        }
        if (alcohol < scale.moderateAlcoholBelow)
        {
            return "moderate in alcohol";
        }
        return alcohol < scale.strongAlcoholBelow ? "fairly strong" : "warm and high in alcohol";
    }

    /// <summary>The main sentence about a wine, for example "a dry red with balanced acidity, silky tannins and a full body".</summary>
    public static string Sentence(WineProfile wine, WineVocabulary scale)
    {
        string kind = $"{Sweetness(wine.Sweetness, scale)} {Color(wine.Redness, scale)}";
        string article = kind.StartsWith("o") ? "an" : "a"; // "an off-dry", "a dry".
        string text = $"{article} {kind} with {Acidity(wine.Acidity, scale)}";
        if (wine.Redness > scale.whiteBelow)
        {
            text += $", {Tannin(wine.Tannin, scale)}";
        }
        return text + $" and {Body(wine.Body, scale)}";
    }

    /// <summary>What the player learns from tasting the glass.</summary>
    public static string Taste(WineProfile wine, WineVocabulary scale)
    {
        if (wine.IsEmpty)
        {
            return "The glass is empty.";
        }

        string text = $"It tastes like {Sentence(wine, scale)}. It feels {Alcohol(wine.Alcohol, scale)}.";
        if (wine.Notes.Count > 0)
        {
            return text + $"\nNotes of {string.Join(", ", wine.Notes)}.";
        }
        return text + "\nNo aroma stands out.";
    }
}
