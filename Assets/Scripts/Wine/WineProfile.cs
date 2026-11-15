using System.Collections.Generic;

/// <summary>
/// The character of a wine: each value is the weighted average of the grapes in the blend.
/// </summary>
public class WineProfile
{
    public float Acidity { get; private set; }
    public float Alcohol { get; private set; }
    public float Tannin { get; private set; }
    public float Body { get; private set; }
    public float Sweetness { get; private set; }

    /// <summary>0 = all white grapes, 1 = all red grapes.</summary>
    public float Redness { get; private set; }

    public List<string> Notes { get; } = new List<string>();
    public bool IsEmpty { get; private set; } = true;

    /// <param name="noteThreshold">A grape needs at least this percent of the blend for its aromas to come through.</param>
    public WineProfile(Blend blend, int noteThreshold)
    {
        if (blend.Total == 0)
        {
            return;
        }
        IsEmpty = false;

        foreach (KeyValuePair<GrapeData, int> share in blend.Shares)
        {
            GrapeData grape = share.Key;
            float weight = share.Value / (float)blend.Total;
            Acidity += grape.acidity * weight;
            Alcohol += grape.alcohol * weight;
            Tannin += grape.tannin * weight;
            Body += grape.body * weight;
            Sweetness += grape.sweetness * weight;
            if (grape.isRed)
            {
                Redness += weight;
            }

            if (weight * Blend.Full >= noteThreshold)
            {
                AddNotes(grape);
            }
        }
    }

    private void AddNotes(GrapeData grape)
    {
        foreach (string note in grape.notes)
        {
            if (!Notes.Contains(note))
            {
                Notes.Add(note);
            }
        }
    }
}
