using System.Collections.Generic;

/// <summary>
/// What is in the glass: how many percent of each grape. It can never hold more than 100 percent.
/// </summary>
public class Blend
{
    public const int Full = 100;

    private readonly Dictionary<GrapeData, int> shares = new Dictionary<GrapeData, int>();

    public int Total { get; private set; }
    public bool IsFull => Total == Full;
    public IReadOnlyDictionary<GrapeData, int> Shares => shares;

    /// <summary>Adds wine of one grape. Returns false, and adds nothing, if the glass would overflow.</summary>
    public bool Add(GrapeData grape, int percent)
    {
        if (Total + percent > Full)
        {
            return false;
        }

        shares.TryGetValue(grape, out int current); // current stays 0 if the grape is not in the glass yet.
        shares[grape] = current + percent;
        Total += percent;
        return true;
    }

    public void Clear()
    {
        shares.Clear();
        Total = 0;
    }

    /// <summary>For example "70% Syrah + 30% Grenache".</summary>
    public string Describe()
    {
        var parts = new List<string>();
        foreach (KeyValuePair<GrapeData, int> share in shares)
        {
            parts.Add($"{share.Value}% {share.Key.displayName}");
        }
        return parts.Count > 0 ? string.Join(" + ", parts) : "empty";
    }
}
