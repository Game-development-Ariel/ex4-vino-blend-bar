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

        // Only the part of the splash that fits goes in, the rest is lost. Then any pour size
        // fills the glass to exactly 100 percent, even one that doesn't divide 100.
        int room = Blend.Full - Blend.Total;
        if (room > 0)
        {
            Blend.Add(drop.Grape, Mathf.Min(drop.Percent, room));
        }
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
