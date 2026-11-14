using System;
using UnityEngine;

/// <summary>
/// The patience of the guest at the counter: a timer that runs down while the guest waits.
/// </summary>
public class Patience : MonoBehaviour
{
    private float fullSeconds;
    private bool isRunning;

    public float SecondsLeft { get; private set; }

    /// <summary>From 1 (just arrived) to 0 (gone).</summary>
    public float Fraction => fullSeconds > 0f ? SecondsLeft / fullSeconds : 0f;

    /// <summary>Happens once, when the patience reaches zero.</summary>
    public event Action RanOut;

    private void Update()
    {
        if (isRunning)
        {
            Spend(Time.deltaTime);
        }
    }

    public void Restart(float seconds)
    {
        fullSeconds = seconds;
        SecondsLeft = seconds;
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void Spend(float seconds)
    {
        if (!isRunning)
        {
            return;
        }

        SecondsLeft = Mathf.Max(0f, SecondsLeft - seconds);
        if (SecondsLeft == 0f)
        {
            isRunning = false;
            RanOut?.Invoke();
        }
    }
}
