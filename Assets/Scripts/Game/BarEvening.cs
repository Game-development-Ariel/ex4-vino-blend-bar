using System;
using UnityEngine;

/// <summary>
/// Runs the evening: brings the guests one after the other, serves the glass, and decides at the end
/// whether the evening passed. It raises events, and the UI listens.
/// </summary>
public class BarEvening : MonoBehaviour
{
    private const float MaxStars = 5f;

    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private Glass glass;
    [SerializeField] private Patience patience;

    [Tooltip("Works only while a guest is waiting, so nothing is poured between guests")]
    [SerializeField] private Bartender bartender;

    private BlendJudge judge;
    private float scoreSum;

    // After the last guest nothing works any more. Without it, pressing Enter on the selected
    // button would end the evening again and again.
    private bool isOver;

    public GuestOrder Order { get; private set; }
    public int GuestNumber { get; private set; }
    public int GuestCount => settings.guests.Length;
    public int Money { get; private set; }

    public event Action<GuestOrder> GuestArrived;
    public event Action<ServeResult> GuestServed;
    public event Action GuestGaveUp;

    /// <summary>Gives the tasting notes of the glass.</summary>
    public event Action<string> Tasted;

    /// <summary>Gives the stars of the evening (0 to 5) and whether that is enough to pass.</summary>
    public event Action<float, bool> EveningEnded;

    private void Awake()
    {
        judge = new BlendJudge(settings);
    }

    private void OnEnable()
    {
        patience.RanOut += OnPatienceRanOut;
    }

    private void OnDisable()
    {
        patience.RanOut -= OnPatienceRanOut;
    }

    private void Start()
    {
        NextGuest();
    }

    /// <summary>Tasting tells the player what is in the glass, and costs some of the guest's patience.</summary>
    public void Taste()
    {
        if (isOver)
        {
            return;
        }

        Tasted?.Invoke(WineWords.Taste(new WineProfile(glass.Blend), settings.vocabulary));
        patience.Spend(settings.tasteCostSeconds);
    }

    public void Serve()
    {
        if (isOver || !glass.Blend.IsFull)
        {
            return;
        }

        ServeResult result = judge.Judge(Order, new WineProfile(glass.Blend), patience.Fraction);
        Money += result.Pay;
        scoreSum += result.Score;
        StopOrdering();
        GuestServed?.Invoke(result);
    }

    public void NextGuest()
    {
        if (isOver)
        {
            return;
        }
        if (GuestNumber == settings.guests.Length)
        {
            isOver = true;
            float stars = scoreSum / settings.guests.Length * MaxStars;
            EveningEnded?.Invoke(stars, stars >= settings.starsToPass);
            return;
        }

        Order = new GuestOrder(settings.guests[GuestNumber], settings.vocabulary);
        GuestNumber++;
        glass.Empty();
        patience.Restart(settings.patienceSeconds);
        bartender.enabled = true;
        GuestArrived?.Invoke(Order);
    }

    // A guest who gives up adds 0 to the score.
    private void OnPatienceRanOut()
    {
        StopOrdering();
        GuestGaveUp?.Invoke();
    }

    private void StopOrdering()
    {
        patience.Stop();
        bartender.enabled = false;
    }
}
