using System;
using UnityEngine;

/// <summary>
/// A bottle of one grape on the shelf. Clicking it asks for a pour.
/// </summary>
public class GrapeBottle : MonoBehaviour, IClickable
{
    [SerializeField] private GrapeData grape;

    [Tooltip("Where the wine comes out")]
    [SerializeField] private Transform spout;

    public GrapeData Grape => grape;
    public Vector2 SpoutPosition => spout.position;

    /// <summary>Happens when the player clicks the bottle. The bartender listens.</summary>
    public event Action<GrapeBottle> Clicked;

    public void OnClicked()
    {
        Clicked?.Invoke(this);
    }
}
