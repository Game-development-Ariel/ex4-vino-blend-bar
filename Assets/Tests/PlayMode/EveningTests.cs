using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>A whole evening at the bar: the guests one after the other, serving, tasting, winning and losing.</summary>
    public class EveningTests : BarSceneTestBase
    {
        [UnityTest]
        public IEnumerator PerfectEvening_EndsOnceWithAPass()
        {
            yield return null;

            PlayThePerfectEvening();

            Assert.AreEqual(3, Served.Count);
            foreach (ServeResult result in Served)
            {
                Assert.AreEqual(1f, result.Score, 1e-5f);
            }
            Assert.AreEqual(1, Ended.Count, "The evening ended exactly once");
            Assert.AreEqual(Settings.maxStars, Ended[0].stars, 1e-5f);
            Assert.IsTrue(Ended[0].passed);
            Assert.IsTrue(Evening.Score.Passed);
            int paid = 0;
            foreach (ServeResult result in Served)
            {
                paid += result.Pay;
            }
            Assert.AreEqual(paid, Evening.Score.Money);
        }

        [UnityTest]
        public IEnumerator AfterTheLastGuest_TheButtonSaysSeeResults()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);
            Evening.Serve();
            Assert.AreEqual("Next guest", TextOf("Next guest"));
            Evening.NextGuest();
            FillGlass("Riesling", 100);
            Evening.Serve();
            Evening.NextGuest();
            FillGlass("Syrah", 70, "Grenache", 30);

            Evening.Serve();

            Assert.AreEqual("See results", TextOf("Next guest"));
            Assert.AreEqual(0, Ended.Count, "The evening ends only when the player asks for the results");
        }

        // Regression: after the last guest, every Enter on the selected button (the keyboard's Submit) ended the evening again.
        [UnityTest]
        public IEnumerator Regression_EveningEndsOnce_EvenWhenNextIsPressedAgain()
        {
            yield return null;
            PlayThePerfectEvening();

            ButtonNamed("Next guest").onClick.Invoke();
            Evening.NextGuest();
            yield return null;

            Assert.AreEqual(1, Ended.Count);
            Assert.AreEqual(3, Evening.GuestNumber);
            Assert.IsFalse(ObjectNamed("Next guest Button").activeSelf, "The button is hidden at the end");
        }

        [UnityTest]
        public IEnumerator AfterTheEnd_TasteAndServeDoNothing()
        {
            yield return null;
            PlayThePerfectEvening();
            FillGlass("Merlot", 100);

            Evening.Taste();
            Evening.Serve();

            Assert.AreEqual(0, Tasted.Count);
            Assert.AreEqual(3, Served.Count);
        }

        [UnityTest]
        public IEnumerator WrongBlendsAllEvening_EndWithoutAPass()
        {
            yield return null;
            for (int guest = 0; guest < 3; guest++)
            {
                FillGlass("Sauvignon Blanc", 100);
                Evening.Serve();
                Evening.NextGuest();
            }

            Assert.AreEqual(1, Ended.Count);
            Assert.IsFalse(Ended[0].passed);
            Assert.Less(Ended[0].stars, Settings.starsToPass);
        }

        [UnityTest]
        public IEnumerator GuestsWhoGiveUp_AddZeroAndTheEveningIsLost()
        {
            yield return null;
            for (int guest = 0; guest < 3; guest++)
            {
                Patience.Spend(Settings.patienceSeconds);
                Evening.NextGuest();
            }

            Assert.AreEqual(3, GaveUp);
            Assert.AreEqual(0, Served.Count);
            Assert.AreEqual(1, Ended.Count);
            Assert.AreEqual(0f, Ended[0].stars);
            Assert.IsFalse(Ended[0].passed);
            Assert.AreEqual(0, Evening.Score.Money);
        }

        [UnityTest]
        public IEnumerator OneGuestGivesUp_TheOtherTwoPerfect_StarsAre33()
        {
            yield return null;
            Patience.Spend(Settings.patienceSeconds);
            Evening.NextGuest();
            FillGlass("Riesling", 100);
            Evening.Serve();
            Evening.NextGuest();
            FillGlass("Syrah", 70, "Grenache", 30);
            Evening.Serve();
            Evening.NextGuest();

            Assert.AreEqual(3.3f, Ended[0].stars, 1e-5f);
            Assert.IsTrue(Ended[0].passed);
        }

        [UnityTest]
        public IEnumerator Patience_RunningOutByItself_MakesTheGuestLeave()
        {
            Time.timeScale = 100f;

            yield return WaitUntil(() => GaveUp > 0, "the guest to give up");

            Assert.AreEqual(1, GaveUp);
            Assert.AreEqual(0f, Patience.SecondsLeft);
            Assert.IsFalse(Bartender.enabled);
            Time.timeScale = 1f;
            yield return null;
            Assert.AreEqual(1, GaveUp, "A guest gives up only once");
        }

        [UnityTest]
        public IEnumerator Serve_WithAGlassThatIsNotFull_IsRefused()
        {
            yield return null;
            FillGlass("Pinot Noir", 90);

            Evening.Serve();

            Assert.AreEqual(0, Served.Count);
            Assert.IsTrue(Bartender.enabled, "The guest is still ordering");
        }

        // Regression: Serve called twice (a double click) scored and paid the same guest twice.
        [UnityTest]
        public IEnumerator Regression_ServeTwice_ScoresOnce()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);

            Evening.Serve();
            Evening.Serve();

            Assert.AreEqual(1, Served.Count);
            Assert.AreEqual(Served[0].Pay, Evening.Score.Money);
        }

        [UnityTest]
        public IEnumerator Serve_AfterTheGuestGaveUp_IsRefused()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);
            Patience.Spend(Settings.patienceSeconds);

            Evening.Serve();

            Assert.AreEqual(0, Served.Count);
        }

        // Regression: tasting an empty glass cost patience, though it teaches nothing.
        [UnityTest]
        public IEnumerator Regression_EmptyGlassTasteIsFree()
        {
            yield return null;
            float before = Patience.SecondsLeft;

            Evening.Taste();

            Assert.AreEqual(before, Patience.SecondsLeft);
            Assert.AreEqual(1, Tasted.Count);
            Assert.IsTrue(Tasted[0].IsEmpty);
        }

        [UnityTest]
        public IEnumerator Taste_OfAFullGlass_CostsTasteCostSeconds()
        {
            yield return null;
            FillGlass("Syrah", 100);
            float before = Patience.SecondsLeft;

            Evening.Taste();

            Assert.AreEqual(before - Settings.tasteCostSeconds, Patience.SecondsLeft, 1e-4f);
            Assert.AreEqual(Grape("Syrah").tannin, Tasted[0].Tannin, 1e-4f);
        }

        [UnityTest]
        public IEnumerator Taste_OfAHalfGlass_AlsoCosts()
        {
            yield return null;
            FillGlass("Syrah", 50);
            float before = Patience.SecondsLeft;

            Evening.Taste();

            Assert.AreEqual(before - Settings.tasteCostSeconds, Patience.SecondsLeft, 1e-4f);
        }

        [UnityTest]
        public IEnumerator Taste_BetweenGuests_DoesNothing()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);
            Evening.Serve();

            Evening.Taste();

            Assert.AreEqual(0, Tasted.Count);
        }

        [UnityTest]
        public IEnumerator NextGuest_WhileAGuestIsOrdering_DoesNothing()
        {
            yield return null;

            Evening.NextGuest();

            Assert.AreEqual(1, Evening.GuestNumber);
            Assert.AreEqual("Noa", Evening.Order.GuestName);
            Assert.AreEqual(0, Arrived);
        }

        [UnityTest]
        public IEnumerator NextGuest_EmptiesTheGlassAndRestartsThePatience()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);
            Patience.Spend(20f);
            Evening.Serve();

            Evening.NextGuest();

            Assert.AreEqual(2, Evening.GuestNumber);
            Assert.AreEqual("Itai", Evening.Order.GuestName);
            Assert.AreEqual(0, Glass.Blend.Total);
            Assert.AreEqual(Settings.patienceSeconds, Patience.SecondsLeft, 1e-4f);
            Assert.IsTrue(Bartender.enabled);
            Assert.AreEqual(1, Arrived);
        }

        [UnityTest]
        public IEnumerator FastPerfectGlass_GetsABiggerTipThanASlowOne()
        {
            yield return null;
            FillGlass("Pinot Noir", 100);
            Evening.Serve();
            Evening.NextGuest();
            Patience.Spend(Settings.patienceSeconds * 0.9f);
            FillGlass("Riesling", 100);
            Evening.Serve();

            Assert.Greater(Served[0].Pay, Served[1].Pay);
        }

        private static string TextOf(string buttonLabel)
        {
            return ObjectNamed(buttonLabel + " Button").GetComponentInChildren<UnityEngine.UI.Text>(true).text;
        }
    }
}
