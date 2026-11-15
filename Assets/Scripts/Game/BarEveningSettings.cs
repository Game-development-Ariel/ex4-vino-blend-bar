using UnityEngine;

/// <summary>
/// The numbers of one evening at the bar, saved as an asset so they can be tuned without touching code.
/// </summary>
[CreateAssetMenu(menuName = "Wine Bar/Evening Settings")]
public class BarEveningSettings : ScriptableObject
{
    public string title = "Day 1";
    public GuestRecipe[] guests;

    [Tooltip("Where the words for a wine change, for the guests' orders and the tasting")]
    public WineVocabulary vocabulary;

    [Tooltip("The numbers the glass is scored with")]
    public JudgeSettings judge;

    [Header("Patience")]
    [Tooltip("Seconds each guest waits before leaving without paying")]
    public float patienceSeconds = 75f;

    [Tooltip("Seconds of patience each tasting costs")]
    public float tasteCostSeconds = 5f;

    [Header("Passing")]
    [Tooltip("A perfect evening gets this many stars")]
    public float maxStars = 5f;

    [Tooltip("The average score of the evening, in stars out of Max Stars, needed to win")]
    public float starsToPass = 3f;

    [Header("Money")]
    [Tooltip("Dollars every served guest pays, even for a bad glass")]
    public int basePay = 15;

    [Tooltip("Added to the base pay, multiplied by the score")]
    public int payForScore = 35;

    [Tooltip("Above this score the guest also tips")]
    public float tipScore = 0.85f;

    [Tooltip("The biggest tip in dollars, for a glass above Tip Score served right away. It shrinks with the patience left")]
    public int maxTip = 10;

    // Unity runs this in the editor when the asset changes, so a broken guest shows up right away.
    private void OnValidate()
    {
        if (guests == null)
        {
            return;
        }
        foreach (GuestRecipe guest in guests)
        {
            string problem = guest.Problem();
            if (problem != null)
            {
                Debug.LogWarning($"{name}, guest {guest.guestName}: {problem}", this);
            }
        }
    }
}
