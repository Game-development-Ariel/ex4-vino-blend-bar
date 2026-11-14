using UnityEngine;

/// <summary>
/// A splash of wine flying from a bottle to the glass. It remembers which grape it is and how much.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class WineDrop : MonoBehaviour
{
    public GrapeData Grape { get; private set; }
    public int Percent { get; private set; }

    public void Setup(GrapeData grape, int percent)
    {
        Grape = grape;
        Percent = percent;
        GetComponent<SpriteRenderer>().color = grape.liquidColor;
    }
}
