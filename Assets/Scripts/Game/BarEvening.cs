using System;
using UnityEngine;

/// <summary>
/// Runs the evening: brings the guests one after the other, lets the player taste and serve,
/// and ends the evening after the last guest. The score is kept in EveningScore.
/// It raises events, and the UI listens.
/// </summary>
public class BarEvening : MonoBehaviour
{
    [SerializeField] private BarEveningSettings settings;
    [SerializeField] private Glass glass;
    [SerializeField] private Patience patience;

    [Tooltip("Works only while a guest is waiting, so nothing is poured between guests")]
    [SerializeField] private Bartender bartender;

    private BlendJudge judge;

    // After the last guest nothing works any more. Without it, pressing Enter on the selected
    // button would end the evening again and again.
    private bool isOver;

    public GuestOrder Order { get; private set; }
    public int GuestNumber { get; private set; }
    public int GuestCount => settings.guests != null ? settings.guests.Length : 0;
    public EveningScore Score { get; private set; }

    public event Action<GuestOrder> GuestArrived;
    public event Action<ServeResult> GuestServed;
    public event Action GuestGaveUp;

    /// <summary>Gives the tasting notes of the glass.</summary>
    public event Action<string> Tasted;

    /// <summary>Gives the stars of the evening (0 to maxStars) and whether that is enough to pass.</summary>
    public event Action<float, bool> EveningEnded;

    private void Awake()
    {
        judge = new BlendJudge(settings);
        Score = new EveningScore(settings);
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
        if (!HasTheData())
        {
            isOver = true; // Nothing can work without the data, and the error said what is missing.
            return;
        }
        NextGuest();
    }

    /// <summary>
    /// Tasting tells the player what is in the glass, and costs some of the guest's patience.
    /// Tasting an empty glass teaches nothing, so it is free.
    /// </summary>
    public void Taste()
    {
        if (isOver)
        {
            return;
        }

        var wine = new WineProfile(glass.Blend, settings.vocabulary.noteThreshold);
        Tasted?.Invoke(WineWords.Taste(wine, settings.vocabulary));
        if (!wine.IsEmpty)
        {
            patience.Spend(settings.tasteCostSeconds);
        }
    }

    public void Serve()
    {
        if (isOver || !glass.Blend.IsFull)
        {
            return;
        }

        ServeResult result = judge.Judge(Order, new WineProfile(glass.Blend, settings.vocabulary.noteThreshold), patience.Fraction);
        Score.Add(result);
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
            EveningEnded?.Invoke(Score.Stars, Score.Passed);
            return;
        }

        Order = new GuestOrder(settings.guests[GuestNumber], settings.vocabulary);
        GuestNumber++;
        glass.Empty();
        patience.Restart(settings.patienceSeconds);
        bartender.enabled = true;
        GuestArrived?.Invoke(Order);
    }

    private bool HasTheData()
    {
        if (settings.vocabulary == null)
        {
            Debug.LogError($"{settings.name} has no WineVocabulary. Drag Assets/Data/WineVocabulary into its Vocabulary field.", settings);
            return false;
        }
        if (settings.judge == null)
        {
            Debug.LogError($"{settings.name} has no JudgeSettings. Drag Assets/Data/JudgeSettings into its Judge field.", settings);
            return false;
        }
        if (GuestCount == 0)
        {
            Debug.LogError($"{settings.name} has no guests, so there is no evening to play.", settings);
            return false;
        }
        return true;
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
