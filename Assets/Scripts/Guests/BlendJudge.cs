using UnityEngine;

/// <summary>
/// Compares the served wine with the guest's order and gives a score.
/// Part of the score is how close the properties are, and part is the requested aromas.
/// The numbers are in a JudgeSettings asset, and the money in BarEveningSettings.
/// </summary>
public class BlendJudge
{
    private readonly BarEveningSettings settings;
    private readonly JudgeSettings rules;

    public BlendJudge(BarEveningSettings settings)
    {
        this.settings = settings;
        rules = settings.judge;
    }

    /// <param name="patienceLeft">From 0 to 1. A great glass served fast gets a bigger tip.</param>
    public ServeResult Judge(GuestOrder order, WineProfile served, float patienceLeft)
    {
        var result = new ServeResult();
        WineProfile target = order.Target;

        // The sums for the weighted average of the properties.
        float weightedPoints = 0f;
        float weights = 0f;

        Compare(result, "Color", served.Redness, target.Redness, rules.color, ref weightedPoints, ref weights);
        Compare(result, "Acidity", served.Acidity, target.Acidity, rules.acidity, ref weightedPoints, ref weights);
        Compare(result, "Alcohol", served.Alcohol, target.Alcohol, rules.alcohol, ref weightedPoints, ref weights);

        // Same rule as the words: a guest who orders a white doesn't talk about tannins.
        if (target.Redness > settings.vocabulary.whiteBelow)
        {
            Compare(result, "Tannin", served.Tannin, target.Tannin, rules.tannin, ref weightedPoints, ref weights);
        }
        Compare(result, "Body", served.Body, target.Body, rules.body, ref weightedPoints, ref weights);
        Compare(result, "Sweetness", served.Sweetness, target.Sweetness, rules.sweetness, ref weightedPoints, ref weights);

        int notesFound = 0;
        foreach (string note in order.RequestedNotes)
        {
            bool found = served.Notes.Contains(note);
            if (found)
            {
                notesFound++;
            }
            result.Feedback.Add($"Notes of {note}: {(found ? "yes" : "missing")}");
        }

        // A guest who asked for no aromas gets the full points for them.
        float noteScore = order.RequestedNotes.Length > 0 ? notesFound / (float)order.RequestedNotes.Length : 1f;
        result.Score = weightedPoints / weights * rules.propertiesWeight + noteScore * rules.notesWeight;
        result.Pay = Mathf.RoundToInt(settings.basePay + settings.payForScore * result.Score);
        if (result.Score > settings.tipScore)
        {
            result.Pay += Mathf.RoundToInt(settings.maxTip * patienceLeft);
        }
        result.Reaction = Reaction(result.Score);
        return result;
    }

    /// <summary>Adds the points of one property to the sums, and the guest's words if it is off.</summary>
    private void Compare(ServeResult result, string label, float got, float want, JudgeSettings.Property property,
        ref float weightedPoints, ref float weights)
    {
        float error = Mathf.Abs(got - want) / property.range;
        weightedPoints += Mathf.Clamp01(1f - error / rules.worstError) * property.weight;
        weights += property.weight;

        if (error < rules.closeEnoughError)
        {
            return; // Close enough. Only the problems are worth the guest's words.
        }
        string size = error < rules.littleOffError ? "a little" : "way";
        result.Feedback.Add($"{label}: {size} too {(got > want ? "high" : "low")}");
    }

    private string Reaction(float score)
    {
        if (score >= rules.perfectScore)
        {
            return "Perfect! Exactly what I wanted!";
        }
        if (score >= rules.lovelyScore)
        {
            return "Lovely, thank you.";
        }
        return score >= rules.okayScore ? "It's okay, I guess." : "This is not what I asked for.";
    }
}
