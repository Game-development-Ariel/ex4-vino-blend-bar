/// <summary>
/// The order of the guest at the counter: the wine the guest wants, and how the guest says it.
/// The guest describes the taste and never names the grapes. Guessing them is the game.
/// </summary>
public class GuestOrder
{
    public string GuestName { get; }
    public WineProfile Target { get; }
    public string[] RequestedNotes { get; }

    /// <summary>The right answer, shown after serving, for example "70% Syrah + 30% Grenache".</summary>
    public string Recipe { get; }

    public GuestOrder(GuestRecipe recipe)
    {
        var blend = new Blend();
        foreach (GuestRecipe.Share share in recipe.blend)
        {
            blend.Add(share.grape, share.percent);
        }

        GuestName = recipe.guestName;
        Target = new WineProfile(blend);
        RequestedNotes = recipe.requestedNotes;
        Recipe = blend.Describe();
    }

    public string Speech()
    {
        return $"I'd like {WineWords.Sentence(Target)}. Something {WineWords.Alcohol(Target.Alcohol)}. " +
               $"I love notes of {string.Join(" and ", RequestedNotes)}.";
    }
}
