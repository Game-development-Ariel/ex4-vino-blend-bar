using UnityEngine;

/// <summary>
/// A splash of wine flying from a bottle to the glass. It remembers which grape it is and how much.
/// A splash that misses the glass is destroyed after its lifetime, so it doesn't fall forever.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class WineDrop : MonoBehaviour
{
    [Tooltip("Seconds until a splash that missed the glass is destroyed")]
    [SerializeField, Min(0.1f)] private float lifetimeSeconds = 3f;

    public GrapeData Grape { get; private set; }
    public int Percent { get; private set; }

    private void Start()
    {
        Destroy(gameObject, lifetimeSeconds);
    }

    public void Setup(GrapeData grape, int percent)
    {
        Grape = grape;
        Percent = percent;
        GetComponent<SpriteRenderer>().color = grape.liquidColor;
    }
}
