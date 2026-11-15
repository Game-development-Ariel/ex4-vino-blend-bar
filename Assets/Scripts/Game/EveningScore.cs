using System;

/// <summary>
/// The score of the evening: the money the guests paid, and the stars from their scores.
/// A guest who left without being served adds 0.
/// </summary>
public class EveningScore
{
    // The stars are shown with one decimal ("2.9 of 5 stars"), so the pass check rounds the same way.
    // Otherwise 2.96 stars would show "3.0" and still lose.
    private const int StarDecimals = 1;

    private readonly BarEveningSettings settings;
    private float scoreSum;

    public EveningScore(BarEveningSettings settings)
    {
        this.settings = settings;
    }

    public int Money { get; private set; }

    /// <summary>The average score of all the guests of the evening, from 0 to maxStars, rounded as it is shown.</summary>
    public float Stars
    {
        get
        {
            int guests = settings.guests != null ? settings.guests.Length : 0;
            if (guests == 0)
            {
                return 0f;
            }
            double stars = scoreSum / guests * settings.maxStars;
            return (float)Math.Round(stars, StarDecimals, MidpointRounding.AwayFromZero);
        }
    }

    public bool Passed => Stars >= settings.starsToPass;

    public void Add(ServeResult result)
    {
        Money += result.Pay;
        scoreSum += result.Score;
    }
}
