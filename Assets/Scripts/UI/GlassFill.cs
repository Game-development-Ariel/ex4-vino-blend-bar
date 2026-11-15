using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws the wine inside the glass: its height follows the total, and its color is the mix of the grapes' colors.
/// Put it on the sprite of the liquid, which should be one unit tall when the glass is full.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GlassFill : MonoBehaviour
{
    [SerializeField] private Glass glass;

    private SpriteRenderer liquid;
    private Vector3 fullScale;
    private Vector3 fullPosition;

    private void Awake()
    {
        liquid = GetComponent<SpriteRenderer>();
        fullScale = transform.localScale;
        fullPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        glass.Changed += Redraw;
    }

    private void OnDisable()
    {
        glass.Changed -= Redraw;
    }

    private void Start()
    {
        Redraw();
    }

    private void Redraw()
    {
        Blend blend = glass.Blend;
        float level = blend.Total / (float)Blend.Full;

        // The sprite scales around its middle, so it also moves down to keep its bottom in place.
        transform.localScale = new Vector3(fullScale.x, fullScale.y * level, fullScale.z);
        transform.localPosition = fullPosition - new Vector3(0f, fullScale.y * (1f - level) / 2f, 0f);

        Color mix = Color.clear;
        foreach (KeyValuePair<GrapeData, int> share in blend.Shares)
        {
            mix += share.Key.liquidColor * (share.Value / (float)blend.Total);
        }
        liquid.color = mix;
    }
}
