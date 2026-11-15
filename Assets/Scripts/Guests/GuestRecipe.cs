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
}
