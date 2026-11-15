using System.Collections;
using UnityEngine;

/// <summary>
/// Small animations for the UI, written as coroutines: start one with StartCoroutine(UiAnim.Fade(...)).
/// Each one changes a value a little every frame until the time is up.
/// </summary>
public static class UiAnim
{
    /// <summary>Fades a whole group of UI objects (alpha 0 = invisible, 1 = visible).</summary>
    public static IEnumerator Fade(CanvasGroup group, float to, float seconds)
    {
        float from = group.alpha;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        group.alpha = to;
    }
}
