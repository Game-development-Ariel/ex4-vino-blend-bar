using System;
using UnityEngine;

/// <summary>
/// The numbers BlendJudge scores with: how much each property counts, how far off is too far,
/// and from what score the guest says what. Saved as an asset, so the scoring can be tuned without touching code.
/// </summary>
[CreateAssetMenu(menuName = "Wine Bar/Judge Settings")]
public class JudgeSettings : ScriptableObject
{
    /// <summary>How one property of the wine is compared.</summary>
    [Serializable]
    public class Property
    {
        [Tooltip("The distance between the lowest and highest possible value of this property")]
        public float range;

        [Tooltip("How much this property counts compared to the others")]
        public float weight;
    }

    [Header("The parts of the score (together 1)")]
    public float propertiesWeight = 0.75f;
    public float notesWeight = 0.25f;

    [Header("Properties")]
    [Tooltip("A property that is off by this share of its whole range gets no points")]
    public float worstError = 0.35f;

    // Color goes from 0 (white) to 1 (red), alcohol from about 10 to 15 percent, the rest from 1 to 10.
    public Property color = new Property { range = 1f, weight = 2f };
    public Property acidity = new Property { range = 9f, weight = 1f };
    public Property alcohol = new Property { range = 5f, weight = 1f };
    public Property tannin = new Property { range = 9f, weight = 1f };
    public Property body = new Property { range = 9f, weight = 1f };
    public Property sweetness = new Property { range = 9f, weight = 1f };

    [Header("Feedback after serving (errors as a share of the range)")]
    [Tooltip("Below this error the guest says nothing about the property")]
    public float closeEnoughError = 0.08f;

    [Tooltip("Below this error it is \"a little\" too high or low, above it \"way\"")]
    public float littleOffError = 0.2f;

    [Header("The guest's reaction, from this score up")]
    public float perfectScore = 0.9f;
    public float lovelyScore = 0.75f;
    public float okayScore = 0.5f;
}
