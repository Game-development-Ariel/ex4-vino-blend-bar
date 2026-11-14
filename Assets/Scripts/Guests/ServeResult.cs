using System.Collections.Generic;

/// <summary>
/// What came out of serving one glass: the score, the money, and what the guest thought about each property.
/// </summary>
public class ServeResult
{
    /// <summary>From 0 (nothing like the order) to 1 (exactly the order).</summary>
    public float Score;

    public int Pay;
    public string Reaction;
    public List<string> Feedback = new List<string>();
}
