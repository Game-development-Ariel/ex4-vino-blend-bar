using UnityEngine;

/// <summary>
/// Compares the served wine with the guest's order and gives a score.
/// 75 percent of the score is how close the properties are, and 25 percent is the requested aromas.
/// </summary>
public class BlendJudge
{
    private const float PropertiesWeight = 0.75f;
    private const float NotesWeight = 0.25f;

    // A property that is off by this share of its whole range gets no points.
    private const float WorstError = 0.35f;

    private readonly BarEveningSettings settings;

    // The sums for the weighted average of the properties, collected by Compare.
    private float weightedPoints;
    private float weights;

    public BlendJudge(BarEveningSettings settings)
    {
        this.settings = settings;
    }

    /// <param name="patienceLeft">From 0 to 1. A great glass served fast gets a bigger tip.</param>
    public ServeResult Judge(GuestOrder order, WineProfile served, float patienceLeft)
    {
        var result = new ServeResult();
        WineProfile target = order.Target;
        weightedPoints = 0f;
        weights = 0f;

        Compare(result, "Color", served.Redness, target.Redness, 1f, 2f);
        Compare(result, "Acidity", served.Acidity, target.Acidity, 9f, 1f);
        Compare(result, "Alcohol", served.Alcohol, target.Alcohol, 5f, 1f);
        if (target.Redness > 0.25f)
        {
            Compare(result, "Tannin", served.Tannin, target.Tannin, 9f, 1f);
        }
        Compare(result, "Body", served.Body, target.Body, 9f, 1f);
        Compare(result, "Sweetness", served.Sweetness, target.Sweetness, 9f, 1f);

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
        result.Score = weightedPoints / weights * PropertiesWeight + noteScore * NotesWeight;
        result.Pay = Mathf.RoundToInt(settings.basePay + settings.payForScore * result.Score);
        if (result.Score > settings.tipScore)
        {
            result.Pay += Mathf.RoundToInt(settings.maxTip * patienceLeft);
        }
        result.Reaction = Reaction(result.Score);
        return result;
    }

    /// <param name="range">The distance between the lowest and highest possible value of this property.</param>
    /// <param name="weight">How much this property counts compared to the others.</param>
    private void Compare(ServeResult result, string label, float got, float want, float range, float weight)
    {
        float error = Mathf.Abs(got - want) / range;
        weightedPoints += Mathf.Clamp01(1f - error / WorstError) * weight;
        weights += weight;

        if (error < 0.08f)
        {
            return; // Close enough. Only the problems are worth the guest's words.
        }
        string size = error < 0.2f ? "a little" : "way";
        result.Feedback.Add($"{label}: {size} too {(got > want ? "high" : "low")}");
    }

    private static string Reaction(float score)
    {
        if (score >= 0.9f)
        {
            return "Perfect! Exactly what I wanted!";
        }
        if (score >= 0.75f)
        {
            return "Lovely, thank you.";
        }
        return score >= 0.5f ? "It's okay, I guess." : "This is not what I asked for.";
    }
}
