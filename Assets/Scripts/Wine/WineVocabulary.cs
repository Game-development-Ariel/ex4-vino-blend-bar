using UnityEngine;

/// <summary>
/// Where the words for a wine change: for example, below what sweetness a wine is "dry",
/// and how much of a grape it takes for its aromas to be named.
/// The values use the same scale as GrapeData (1 to 10, and alcohol in percent),
/// and they are saved as an asset, so the words can be tuned without touching code.
/// </summary>
[CreateAssetMenu(menuName = "Wine Bar/Wine Vocabulary")]
public class WineVocabulary : ScriptableObject
{
    [Header("Color (0 is white, 1 is red)")]
    public float whiteBelow = 0.25f;
    public float redAbove = 0.75f;

    [Header("Sweetness")]
    public float dryBelow = 3f;
    public float offDryBelow = 4.5f;

    [Header("Acidity")]
    public float softAcidityBelow = 4.5f;
    public float balancedAcidityBelow = 6.5f;
    public float crispAcidityBelow = 8f;

    [Header("Tannins")]
    public float noTanninsBelow = 2.5f;
    public float silkyTanninsBelow = 4.5f;
    public float mediumTanninsBelow = 6.5f;

    [Header("Body")]
    public float lightBodyBelow = 4f;
    public float mediumBodyBelow = 6.5f;

    [Header("Aromas")]
    [Tooltip("A grape needs at least this percent of the blend for its aromas to come through")]
    public int noteThreshold = 20;

    [Header("Alcohol, in percent")]
    public float lowAlcoholBelow = 11.5f;
    public float moderateAlcoholBelow = 13f;
    public float strongAlcoholBelow = 14f;
}
