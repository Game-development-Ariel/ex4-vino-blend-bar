using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>
    /// An evening whose settings are broken says what is missing, once, and does nothing else.
    /// Each test builds a small bar of its own (an evening, a glass, patience and a bartender without bottles).
    /// </summary>
    public class MissingDataTests
    {
        private GameObject bar;
        private BarEveningSettings settings;
        private WineVocabulary vocabulary;
        private JudgeSettings judge;

        [SetUp]
        public void MakeSettings()
        {
            vocabulary = ScriptableObject.CreateInstance<WineVocabulary>();
            judge = ScriptableObject.CreateInstance<JudgeSettings>();
            settings = ScriptableObject.CreateInstance<BarEveningSettings>();
            settings.name = "BrokenSettings";
            settings.vocabulary = vocabulary;
            settings.judge = judge;
            settings.guests = new[] { new GuestRecipe { guestName = "Noa" } };
        }

        [TearDown]
        public void DestroyTheBar()
        {
            Object.Destroy(bar);
            Object.Destroy(vocabulary);
            Object.Destroy(judge);
            Object.Destroy(settings);
        }

        [UnityTest]
        public IEnumerator WithoutAVocabulary_TheEveningSaysSoAndNoGuestComes()
        {
            settings.vocabulary = null;
            LogAssert.Expect(LogType.Error, new Regex("BrokenSettings has no WineVocabulary"));

            BarEvening evening = MakeBar();
            yield return null;

            Assert.AreEqual(0, evening.GuestNumber);
            evening.NextGuest();
            Assert.AreEqual(0, evening.GuestNumber, "Nothing works without the data");
        }

        [UnityTest]
        public IEnumerator WithoutJudgeSettings_TheEveningSaysSo()
        {
            settings.judge = null;
            LogAssert.Expect(LogType.Error, new Regex("BrokenSettings has no JudgeSettings"));

            BarEvening evening = MakeBar();
            yield return null;

            Assert.AreEqual(0, evening.GuestNumber);
        }

        [UnityTest]
        public IEnumerator WithoutGuests_TheEveningSaysSo()
        {
            settings.guests = new GuestRecipe[0];
            LogAssert.Expect(LogType.Error, new Regex("BrokenSettings has no guests"));

            BarEvening evening = MakeBar();
            yield return null;

            Assert.AreEqual(0, evening.GuestCount);
            evening.NextGuest();
            Assert.AreEqual(0, evening.GuestNumber);
        }

        [UnityTest]
        public IEnumerator WithNullGuests_GuestCountIsZero()
        {
            settings.guests = null;
            LogAssert.Expect(LogType.Error, new Regex("BrokenSettings has no guests"));

            BarEvening evening = MakeBar();
            yield return null;

            Assert.AreEqual(0, evening.GuestCount);
        }

        private BarEvening MakeBar()
        {
            bar = new GameObject("Test Bar");
            bar.SetActive(false); // The fields are set before Awake and OnEnable, like in a saved scene.
            bar.AddComponent<BoxCollider2D>().isTrigger = true; // Glass needs a collider, and Collider2D itself can't be added.
            Glass glass = bar.AddComponent<Glass>();
            Patience patience = bar.AddComponent<Patience>();
            Bartender bartender = bar.AddComponent<Bartender>();
            PlayReflect.Set(bartender, "bottles", new GrapeBottle[0]);
            PlayReflect.Set(bartender, "glass", glass);
            BarEvening evening = bar.AddComponent<BarEvening>();
            PlayReflect.Set(evening, "settings", settings);
            PlayReflect.Set(evening, "glass", glass);
            PlayReflect.Set(evening, "patience", patience);
            PlayReflect.Set(evening, "bartender", bartender);
            bar.SetActive(true);
            return evening;
        }
    }
}
