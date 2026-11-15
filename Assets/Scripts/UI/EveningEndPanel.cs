using UnityEngine;

/// <summary>
/// Shows the result of the evening when it ends: the stars, the money, and whether it was enough.
/// </summary>
public class EveningEndPanel : MonoBehaviour
{
    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private BarEvening evening;

    private void OnEnable()
    {
        evening.EveningEnded += Show;
    }

    private void OnDisable()
    {
        evening.EveningEnded -= Show;
    }

    private void Show(float stars, bool passed)
    {
        string score = $"{stars:0.0} of {settings.maxStars:0} stars. You earned ${evening.Money}.";
        if (passed)
        {
            ResultOverlay.Show("A good evening", score + "\nThe guests will come back tomorrow.", UiTheme.Hope);
        }
        else
        {
            ResultOverlay.Show("A hard evening", score + $"\nThe bar needs {settings.starsToPass:0} stars to stay open. Try again.", UiTheme.Danger);
        }
    }
}
