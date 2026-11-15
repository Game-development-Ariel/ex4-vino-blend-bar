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
    [Tooltip("Below this redness the wine is called white. Up to it, nobody talks about tannins and the judge skips them")]
    public float whiteBelow = 0.25f;

    [Tooltip("Above this redness the wine is called red. In between it is rose-style")]
    public float redAbove = 0.75f;

    [Header("Sweetness")]
    [Tooltip("Below this sweetness the wine is dry")]
    public float dryBelow = 3f;

    [Tooltip("Below this sweetness the wine is off-dry, above it slightly sweet")]
    public float offDryBelow = 4.5f;

    [Header("Acidity")]
    [Tooltip("Below this acidity it is soft")]
    public float softAcidityBelow = 4.5f;

    [Tooltip("Below this acidity it is balanced")]
    public float balancedAcidityBelow = 6.5f;

    [Tooltip("Below this acidity it is crisp, above it zingy")]
    public float crispAcidityBelow = 8f;

    [Header("Tannins")]
    [Tooltip("Below this tannin the wine has no tannins")]
    public float noTanninsBelow = 2.5f;

    [Tooltip("Below this tannin the tannins are silky")]
    public float silkyTanninsBelow = 4.5f;

    [Tooltip("Below this tannin the tannins are medium, above it firm")]
    public float mediumTanninsBelow = 6.5f;

    [Header("Body")]
    [Tooltip("Below this body the wine has a light body")]
    public float lightBodyBelow = 4f;

    [Tooltip("Below this body it has a medium body, above it a full body")]
    public float mediumBodyBelow = 6.5f;

    [Header("Aromas")]
    [Tooltip("A grape needs at least this percent of the blend for its aromas to come through")]
    public int noteThreshold = 20;

    [Header("Alcohol, in percent")]
    [Tooltip("Below this alcohol percent the wine is low in alcohol")]
    public float lowAlcoholBelow = 11.5f;

    [Tooltip("Below this alcohol percent it is moderate")]
    public float moderateAlcoholBelow = 13f;

    [Tooltip("Below this alcohol percent it is fairly strong, above it high in alcohol")]
    public float strongAlcoholBelow = 14f;
}
