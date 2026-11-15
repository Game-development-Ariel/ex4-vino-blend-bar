using System;

/// <summary>
/// One guest of the evening, as written in the level design: the blend the guest really wants,
/// and the aromas the guest mentions.
/// </summary>
[Serializable]
public class GuestRecipe
{
    [Serializable]
    public class Share
    {
        public GrapeData grape;
        public int percent;
    }

    public string guestName;
    public Share[] blend;
    public string[] requestedNotes;

    /// <summary>What is wrong with this guest, or null if nothing is. The blend must fill exactly one glass.</summary>
    public string Problem()
    {
        if (blend == null || blend.Length == 0)
        {
            return "the blend is empty";
        }

        int total = 0;
        foreach (Share share in blend)
        {
            if (share.grape == null)
            {
                return "a share of the blend has no grape";
            }
            total += share.percent;
        }
        if (total != Blend.Full)
        {
            return $"the blend adds up to {total}%, not {Blend.Full}%";
        }

        if (requestedNotes != null)
        {
            foreach (string note in requestedNotes)
            {
                if (string.IsNullOrWhiteSpace(note))
                {
                    return "one of the requested notes is empty";
                }
            }
        }
        return null;
    }
}
