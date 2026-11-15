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

    [Header("Patience")]
    public float patienceSeconds = 75f;

    [Tooltip("Seconds of patience each tasting costs")]
    public float tasteCostSeconds = 5f;

    [Header("Passing")]
    [Tooltip("The average score of the evening, in stars out of 5, needed to win")]
    public float starsToPass = 3f;

    [Header("Money")]
    public int basePay = 15;

    [Tooltip("Added to the base pay, multiplied by the score")]
    public int payForScore = 35;

    [Tooltip("Above this score the guest also tips")]
    public float tipScore = 0.85f;
    public int maxTip = 10;
}
