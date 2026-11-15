using System;
using UnityEngine;

/// <summary>
/// The glass on the counter. A splash of wine that falls into it joins the blend.
/// Needs a trigger collider at the mouth of the glass.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Glass : MonoBehaviour
{
    public Blend Blend { get; } = new Blend();

    /// <summary>Happens every time the blend in the glass changes.</summary>
    public event Action Changed;

    /// <summary>Happens when the glass is emptied, right before Changed.</summary>
    public event Action Emptied;

    private void OnTriggerEnter2D(Collider2D other)
    {
        WineDrop drop = other.GetComponent<WineDrop>();
        if (drop == null)
        {
            return;
        }

        Blend.Add(drop.Grape, drop.Percent); // A splash that doesn't fit is simply lost.
        Destroy(drop.gameObject);
        Changed?.Invoke();
    }

    public void Empty()
    {
        Blend.Clear();
        Emptied?.Invoke();
        Changed?.Invoke();
    }
}
