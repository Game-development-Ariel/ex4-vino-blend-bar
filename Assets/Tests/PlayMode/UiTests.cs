using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>What the player sees: each UI component shows what the evening and the glass say.</summary>
    public class UiTests : BarSceneTestBase
    {
        private const float ColorTolerance = 1e-3f;

        // ---------- GuestPanel ----------

        [UnityTest]
        public IEnumerator GuestPanel_ShowsTheNameAndTheOrderOfTheGuest()
        {
            yield return null;

            Assert.AreEqual("Noa", TextNamed("Name").text);
            Assert.AreEqual($"\"{Evening.Order.Speech()}\"", TextNamed("Speech").text);
        }

        [UnityTest]
        public IEnumerator GuestPanel_ShowsTheReactionAfterServing()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);

            Evening.Serve();

            Assert.AreEqual("\"Perfect! Exactly what I wanted!\"", TextNamed("Speech").text);
        }

        [UnityTest]
        public IEnumerator GuestPanel_SaysGoodNightWhenTheGuestGivesUp()
        {
            yield return null;

            Patience.Spend(Settings.patienceSeconds);

            Assert.AreEqual("\"I've waited long enough. Good night.\"", TextNamed("Speech").text);
        }

        [UnityTest]
        public IEnumerator GuestPanel_PatienceBar_EmptiesAndTurnsRed()
        {
            Image fill = ObjectNamed("Fill").GetComponent<Image>();
            Color calm = PlayReflect.GetStatic<Color>(typeof(GuestPanel), "Calm");
            yield return null;
            Assert.AreEqual(Patience.Fraction, fill.fillAmount, 0.01f);

            Patience.Spend(Settings.patienceSeconds / 2f);
            Patience.Stop();
            yield return null;

            Assert.AreEqual(0.5f, fill.fillAmount, 0.01f);
            AssertColor(Color.Lerp(UiTheme.Danger, calm, 0.5f), fill.color, 0.01f);

            Patience.Restart(1f);
            Patience.Spend(1f);
            yield return null;
            Assert.AreEqual(0f, fill.fillAmount);
            AssertColor(UiTheme.Danger, fill.color, ColorTolerance);
        }

        // ---------- GlassReadout ----------

        [UnityTest]
        public IEnumerator GlassReadout_SaysWhatIsInTheGlass()
        {
            yield return null;

            FillGlass("Syrah", 70, "Grenache", 30);

            Assert.AreEqual("In the glass: 70% Syrah + 30% Grenache\nTotal 100% of 100%", TextNamed("Glass").text);
        }

        [UnityTest]
        public IEnumerator GlassReadout_OfAnEmptyGlass_SaysEmpty()
        {
            yield return null;
            FillGlass("Syrah", 40);

            Glass.Empty();

            Assert.AreEqual("In the glass: empty\nTotal 0% of 100%", TextNamed("Glass").text);
        }

        // ---------- OrderButtons ----------

        [UnityTest]
        public IEnumerator OrderButtons_ServeIsDimmedUntilTheGlassIsFull()
        {
            yield return null;
            CanvasGroup serve = ObjectNamed("Serve Button").GetComponent<CanvasGroup>();
            Assert.IsFalse(serve.interactable);

            FillGlass("Syrah", 90);
            Assert.IsFalse(serve.interactable);
            Assert.Less(serve.alpha, 1f);

            DropIntoGlass("Syrah", 10);
            Assert.IsTrue(serve.interactable);
            Assert.AreEqual(1f, serve.alpha);
        }

        [UnityTest]
        public IEnumerator OrderButtons_WhileOrdering_ShowTasteEmptyAndServe_NotNext()
        {
            yield return null;

            Assert.IsTrue(ObjectNamed("Order Buttons").activeSelf);
            Assert.IsFalse(ObjectNamed("Next guest Button").activeSelf);
        }

        [UnityTest]
        public IEnumerator OrderButtons_AfterServing_ShowOnlyNextGuest()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);

            Evening.Serve();

            Assert.IsFalse(ObjectNamed("Order Buttons").activeSelf);
            Assert.IsTrue(ObjectNamed("Next guest Button").activeSelf);
            Assert.AreEqual("Next guest", ObjectNamed("Next guest Button").GetComponentInChildren<Text>().text);
        }

        [UnityTest]
        public IEnumerator OrderButtons_AfterTheGuestGivesUp_ShowOnlyNextGuest()
        {
            yield return null;

            Patience.Spend(Settings.patienceSeconds);

            Assert.IsFalse(ObjectNamed("Order Buttons").activeSelf);
            Assert.IsTrue(ObjectNamed("Next guest Button").activeSelf);
        }

        [UnityTest]
        public IEnumerator OrderButtons_EachButtonCallsTheEvening()
        {
            yield return null;
            FillGlass("Pinot Noir", 50);

            ButtonNamed("Taste").onClick.Invoke();
            Assert.AreEqual(1, Tasted.Count);

            ButtonNamed("Empty glass").onClick.Invoke();
            Assert.AreEqual(0, Glass.Blend.Total);

            FillGlass("Pinot Noir", 100);
            ButtonNamed("Serve").onClick.Invoke();
            Assert.AreEqual(1, Served.Count);

            ButtonNamed("Next guest").onClick.Invoke();
            Assert.AreEqual(2, Evening.GuestNumber);
            Assert.IsTrue(ObjectNamed("Order Buttons").activeSelf);
        }

        [UnityTest]
        public IEnumerator OrderButtons_AtTheEnd_HideNextAndClearTheKeyboardSelection()
        {
            yield return null;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(ObjectNamed("Next guest Button"));

            PlayThePerfectEvening();

            Assert.IsFalse(ObjectNamed("Next guest Button").activeSelf);
            Assert.IsNull(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        }

        // ---------- NotesBoard ----------

        [UnityTest]
        public IEnumerator NotesBoard_ShowsATipWhenAGuestArrives()
        {
            yield return null;

            StringAssert.StartsWith("Read the order", TextNamed("Notes").text);
        }

        [UnityTest]
        public IEnumerator NotesBoard_ShowsTheTastingInTheGuestsWords()
        {
            yield return null;
            FillGlass("Syrah", 100);

            Evening.Taste();

            Assert.AreEqual(WineWords.Taste(Tasted[0], Settings.vocabulary), TextNamed("Notes").text);
            StringAssert.StartsWith("It tastes like a dry red", TextNamed("Notes").text);
        }

        [UnityTest]
        public IEnumerator NotesBoard_ShowsTheMatchThePayTheFeedbackAndTheRecipe()
        {
            yield return null;
            FillGlass("Pinot Noir", 50, "Merlot", 50);

            Evening.Serve();

            string notes = TextNamed("Notes").text;
            StringAssert.StartsWith($"Match: 85%.  Paid ${Served[0].Pay}.\n", notes);
            StringAssert.Contains("Acidity: a little too low\n", notes);
            StringAssert.EndsWith("\nThe guest wanted: 100% Pinot Noir", notes);
        }

        [UnityTest]
        public IEnumerator NotesBoard_WhenTheGuestGivesUp_ShowsWhatTheyWanted()
        {
            yield return null;

            Patience.Spend(Settings.patienceSeconds);

            Assert.AreEqual("The guest left without paying.\n\nThe guest wanted: 100% Pinot Noir", TextNamed("Notes").text);
        }

        // ---------- BarHud ----------

        [UnityTest]
        public IEnumerator BarHud_ShowsTheTitleTheMoneyAndWhichGuest()
        {
            yield return null;

            Assert.AreEqual(Settings.title, TextNamed("Title").text);
            Assert.AreEqual("$0", TextNamed("Money").text);
            Assert.AreEqual("Guest 1 of 3", TextNamed("Guest").text);
            StringAssert.Contains("Click a bottle", TextNamed("Help").text);
        }

        [UnityTest]
        public IEnumerator BarHud_UpdatesTheMoneyAndTheGuestNumber()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);

            Evening.Serve();
            Assert.AreEqual("$" + Served[0].Pay, TextNamed("Money").text);

            Evening.NextGuest();
            Assert.AreEqual("Guest 2 of 3", TextNamed("Guest").text);
        }

        // ---------- EveningEndPanel and ResultOverlay ----------

        [UnityTest]
        public IEnumerator EveningEndPanel_AfterAGoodEvening_ShowsTheStarsOutOfMaxStarsAndTheMoney()
        {
            yield return null;

            PlayThePerfectEvening();

            Transform result = ObjectNamed("Result Canvas").transform;
            Assert.AreEqual("A good evening", result.Find("Title").GetComponent<Text>().text);
            Assert.AreEqual($"5.0 of {Settings.maxStars:0} stars. You earned ${Evening.Score.Money}.\nThe guests will come back tomorrow.",
                result.Find("Message").GetComponent<Text>().text);
            Assert.IsNotNull(result.Find("Play again Button"));
        }

        [UnityTest]
        public IEnumerator EveningEndPanel_AfterAHardEvening_SaysHowManyStarsWereNeeded()
        {
            yield return null;
            for (int guest = 0; guest < 3; guest++)
            {
                Patience.Spend(Settings.patienceSeconds);
                Evening.NextGuest();
            }

            Transform result = ObjectNamed("Result Canvas").transform;
            Assert.AreEqual("A hard evening", result.Find("Title").GetComponent<Text>().text);
            Assert.AreEqual($"0.0 of {Settings.maxStars:0} stars. You earned $0.\nThe bar needs {Settings.starsToPass:0} stars to stay open. Try again.",
                result.Find("Message").GetComponent<Text>().text);
        }

        [UnityTest]
        public IEnumerator EveningEndPanel_UsesMaxStarsFromTheSettings()
        {
            float maxStars = Settings.maxStars;
            Settings.maxStars = 10f;
            try
            {
                yield return null;
                PlayThePerfectEvening();

                StringAssert.StartsWith("10.0 of 10 stars.", ObjectNamed("Result Canvas").transform.Find("Message").GetComponent<Text>().text);
            }
            finally
            {
                Settings.maxStars = maxStars;
            }
        }

        [UnityTest]
        public IEnumerator PlayAgain_FadesAndLoadsTheBarAgain()
        {
            yield return null;
            PlayThePerfectEvening();
            BarEvening oldEvening = Evening;

            ObjectNamed("Play again Button").GetComponent<Button>().onClick.Invoke();
            ScreenFader.FadeAndLoad("WineBar"); // A double click: the second call is ignored.

            Assert.AreEqual(1, Object.FindObjectsByType<ScreenFader>(FindObjectsSortMode.None).Length, "One fade at a time");
            yield return WaitUntil(() => oldEvening == null, "the scene to load again");
            yield return WaitUntil(() => Object.FindAnyObjectByType<ScreenFader>() == null, "the fade to finish");
            BarEvening newEvening = Object.FindAnyObjectByType<BarEvening>();
            Assert.IsNotNull(newEvening);
            Assert.AreEqual(1, newEvening.GuestNumber, "A new evening starts with the first guest");
        }

        // ---------- GlassFill ----------

        [UnityTest]
        public IEnumerator GlassFill_OfAnEmptyGlass_IsFlatAndClear()
        {
            yield return null;
            SpriteRenderer liquid = Liquid();

            Assert.AreEqual(0f, liquid.transform.localScale.y, 1e-5f);
            Assert.AreEqual(0f, liquid.color.a, 1e-5f);
        }

        [UnityTest]
        public IEnumerator GlassFill_RisesWithTheGlassAndKeepsItsBottom()
        {
            yield return null;
            SpriteRenderer liquid = Liquid();
            GlassFill fill = liquid.GetComponent<GlassFill>();
            Vector3 fullScale = PlayReflect.Get<Vector3>(fill, "fullScale");
            Vector3 fullPosition = PlayReflect.Get<Vector3>(fill, "fullPosition");
            float bottom = fullPosition.y - fullScale.y / 2f;

            FillGlass("Pinot Noir", 50);

            Assert.AreEqual(fullScale.y * 0.5f, liquid.transform.localScale.y, 1e-4f);
            Assert.AreEqual(bottom, liquid.transform.localPosition.y - liquid.transform.localScale.y / 2f, 1e-4f, "The bottom stays in place");
            AssertColor(Grape("Pinot Noir").liquidColor, liquid.color, ColorTolerance);

            FillGlass("Pinot Noir", 50);
            Assert.AreEqual(fullScale.y, liquid.transform.localScale.y, 1e-4f);
        }

        [UnityTest]
        public IEnumerator GlassFill_MixesTheColorsOfTheGrapes()
        {
            yield return null;

            FillGlass("Pinot Noir", 50, "Riesling", 50);

            Color expected = Grape("Pinot Noir").liquidColor * 0.5f + Grape("Riesling").liquidColor * 0.5f;
            AssertColor(expected, Liquid().color, ColorTolerance);
        }

        private static SpriteRenderer Liquid()
        {
            return Object.FindAnyObjectByType<GlassFill>().GetComponent<SpriteRenderer>();
        }

        private static void AssertColor(Color expected, Color actual, float tolerance)
        {
            Assert.AreEqual(expected.r, actual.r, tolerance, "red");
            Assert.AreEqual(expected.g, actual.g, tolerance, "green");
            Assert.AreEqual(expected.b, actual.b, tolerance, "blue");
            Assert.AreEqual(expected.a, actual.a, tolerance, "alpha");
        }
    }
}
