using UnityEngine;

/// <summary>
/// One grape variety. The values are rough real-world averages: acidity, tannin, body and sweetness
/// on a scale of 1 to 10, and alcohol in percent. A new grape is a new asset of this type.
/// </summary>
[CreateAssetMenu(menuName = "Wine Bar/Grape")]
public class GrapeData : ScriptableObject
{
    public string displayName = "Grape";
    public bool isRed;
    public Color liquidColor = Color.red;

    [Tooltip("Acidity, from 1 (flat) to 10 (very sharp)")]
    public float acidity = 5f;

    [Tooltip("Alcohol, in percent")]
    public float alcohol = 13f;

    [Tooltip("Tannin, from 1 (none) to 10 (very grippy)")]
    public float tannin = 5f;

    [Tooltip("Body, from 1 (light like water) to 10 (very full)")]
    public float body = 5f;

    [Tooltip("Sweetness, from 1 (bone dry) to 10 (very sweet)")]
    public float sweetness = 2f;

    [Tooltip("Aromas of this grape. A guest can ask for them")]
    public string[] notes;

    [Tooltip("Two short lines under the bottle, in the same words the guests use")]
    [TextArea]
    public string hint;
}
