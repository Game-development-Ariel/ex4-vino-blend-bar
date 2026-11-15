using System.Collections.Generic;
using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class BlendJudgeTests : CleanTestBase
    {
        private const float ScoreTolerance = 1e-5f;

        // ---------- The real guests of Day 1 ----------

        [TestCase(0, "PinotNoir", 100)]
        [TestCase(1, "Riesling", 100)]
        [TestCase(2, "Syrah", 70, "Grenache", 30)]
        public void Day1Guest_ServedTheirExactRecipe_ScoresFullAndIsPerfect(int guest, params object[] blend)
        {
            BarEveningSettings day1 = TestData.Day1;

            ServeResult result = Judge(day1, TestData.Day1Order(guest), TestData.MakeBlend(blend), 1f);

            Assert.AreEqual(1f, result.Score, ScoreTolerance);
            Assert.AreEqual("Perfect! Exactly what I wanted!", result.Reaction);
            Assert.AreEqual(day1.basePay + day1.payForScore + day1.maxTip, result.Pay);
            foreach (string line in result.Feedback)
            {
                StringAssert.EndsWith(": yes", line, "A perfect glass only gets the notes it asked for");
            }
        }

        [Test]
        public void ClearlyWrongBlend_ScoresLowAndTheGuestComplains()
        {
            ServeResult result = Judge(TestData.Day1, TestData.Day1Order(1), TestData.MakeBlend("CabernetSauvignon", 100), 1f);

            Assert.Less(result.Score, 0.1f);
            Assert.AreEqual("This is not what I asked for.", result.Reaction);
            CollectionAssert.Contains(result.Feedback, "Color: way too high");
            CollectionAssert.Contains(result.Feedback, "Notes of lime: missing");
        }

        // Fixed blends with the exact score, pay and reaction they get today. If a change to the grapes,
        // JudgeSettings, the vocabulary or BlendJudge changes any score, this table fails loudly.
        // To accept a new scoring on purpose, update the numbers here in the same commit.
        private static IEnumerable<TestCaseData> RegressionTable()
        {
            yield return Row(0, new object[] { "PinotNoir", 100 }, 1f, 55, 60, "Perfect! Exactly what I wanted!");
            yield return Row(0, new object[] { "CabernetSauvignon", 100 }, 0.4404762f, 30, 30, "This is not what I asked for.");
            yield return Row(0, new object[] { "PinotNoir", 50, "Merlot", 50 }, 0.8486395f, 45, 45, "Lovely, thank you.");
            yield return Row(0, new object[] { "PinotNoir", 80, "Grenache", 20 }, 0.9210884f, 52, 57, "Perfect! Exactly what I wanted!");
            yield return Row(1, new object[] { "Riesling", 100 }, 1f, 55, 60, "Perfect! Exactly what I wanted!");
            yield return Row(1, new object[] { "SauvignonBlanc", 100 }, 0.5f, 32, 32, "It's okay, I guess.");
            yield return Row(1, new object[] { "Riesling", 50, "Chardonnay", 50 }, 0.6369048f, 37, 37, "It's okay, I guess.");
            yield return Row(1, new object[] { "CabernetSauvignon", 100 }, 0.00595237548f, 15, 15, "This is not what I asked for.");
            yield return Row(2, new object[] { "Syrah", 70, "Grenache", 30 }, 1f, 55, 60, "Perfect! Exactly what I wanted!");
            yield return Row(2, new object[] { "Syrah", 100 }, 0.794387758f, 43, 43, "Lovely, thank you.");
            yield return Row(2, new object[] { "Syrah", 60, "Grenache", 40 }, 0.9731292f, 54, 59, "Perfect! Exactly what I wanted!");
            yield return Row(2, new object[] { "Riesling", 100 }, 0f, 15, 15, "This is not what I asked for.");
        }

        private static TestCaseData Row(int guest, object[] blend, float score, int payHalfPatience, int payFullPatience, string reaction)
        {
            string name = string.Join("_", System.Array.ConvertAll(blend, part => part.ToString()));
            return new TestCaseData(guest, blend, score, payHalfPatience, payFullPatience, reaction)
                .SetName($"Regression_Guest{guest}_Served_{name}_KeepsItsScore");
        }

        [TestCaseSource(nameof(RegressionTable))]
        public void RegressionTable_FixedBlendsKeepTheirExactScores(int guest, object[] blend, float score,
            int payHalfPatience, int payFullPatience, string reaction)
        {
            BarEveningSettings day1 = TestData.Day1;
            GuestOrder order = TestData.Day1Order(guest);
            Blend served = TestData.MakeBlend(blend);

            ServeResult half = Judge(day1, order, served, 0.5f);
            ServeResult full = Judge(day1, order, served, 1f);

            Assert.AreEqual(score, half.Score, ScoreTolerance);
            Assert.AreEqual(payHalfPatience, half.Pay);
            Assert.AreEqual(payFullPatience, full.Pay);
            Assert.AreEqual(reaction, half.Reaction);
        }

        [Test]
        public void Regression_PinotForNoa_GivesExactlyTheseFeedbackLines()
        {
            ServeResult result = Judge(TestData.Day1, TestData.Day1Order(0), TestData.MakeBlend("PinotNoir", 50, "Merlot", 50), 1f);

            CollectionAssert.AreEqual(new[]
            {
                "Acidity: a little too low", "Tannin: a little too high", "Body: a little too high",
                "Notes of cherry: yes", "Notes of raspberry: yes",
            }, result.Feedback);
        }

        // ---------- The rules, with grapes made for the test ----------

        [Test]
        public void MissingAllNotes_LosesTheNotesPartOfTheScore()
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData wanted = MakeGrape("Wanted", true, 5f, 13f, 5f, 5f, 2f, "cherry", "plum");
            GrapeData sameWithoutNotes = MakeGrape("Plain", true);
            GuestOrder order = Order(settings, wanted, "cherry", "plum");

            ServeResult result = Judge(settings, order, Pure(sameWithoutNotes), 1f);

            Assert.AreEqual(settings.judge.propertiesWeight, result.Score, ScoreTolerance);
            CollectionAssert.Contains(result.Feedback, "Notes of cherry: missing");
            CollectionAssert.Contains(result.Feedback, "Notes of plum: missing");
        }

        [Test]
        public void MissingOneOfTwoNotes_LosesHalfTheNotesPart()
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData wanted = MakeGrape("Wanted", true, 5f, 13f, 5f, 5f, 2f, "cherry", "plum");
            GrapeData onlyCherry = MakeGrape("Cherry", true, 5f, 13f, 5f, 5f, 2f, "cherry");

            ServeResult result = Judge(settings, Order(settings, wanted, "cherry", "plum"), Pure(onlyCherry), 1f);

            Assert.AreEqual(settings.judge.propertiesWeight + settings.judge.notesWeight / 2f, result.Score, ScoreTolerance);
        }

        [Test]
        public void NoRequestedNotes_GivesTheFullNotesPart()
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData grape = MakeGrape("Grape", true);

            ServeResult result = Judge(settings, Order(settings, grape), Pure(grape), 1f);

            Assert.AreEqual(1f, result.Score, ScoreTolerance);
            Assert.AreEqual(0, result.Feedback.Count);
        }

        [Test]
        public void Tannin_OfAWhiteOrder_IsNotJudged()
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData white = MakeGrape("White", false, tannin: 1f);
            GrapeData tannicWhite = MakeGrape("Tannic", false, tannin: 9f);

            ServeResult result = Judge(settings, Order(settings, white), Pure(tannicWhite), 1f);

            Assert.AreEqual(1f, result.Score, ScoreTolerance);
            Assert.AreEqual(0, result.Feedback.Count);
        }

        [Test]
        public void Tannin_OfARedOrder_IsJudged()
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData red = MakeGrape("Red", true, tannin: 1f);
            GrapeData tannicRed = MakeGrape("Tannic", true, tannin: 9f);

            ServeResult result = Judge(settings, Order(settings, red), Pure(tannicRed), 1f);

            Assert.Less(result.Score, 1f);
            CollectionAssert.Contains(result.Feedback, "Tannin: way too high");
        }

        // Regression: the tannin rule used a fixed 0.25 instead of the vocabulary's whiteBelow.
        [TestCase(0.6f, false)]
        [TestCase(0.4f, true)]
        public void Regression_TanninThreshold_ComesFromTheVocabulary(float whiteBelow, bool judged)
        {
            BarEveningSettings settings = MakeSettings();
            settings.vocabulary.whiteBelow = whiteBelow;
            GrapeData red = MakeGrape("Red", true, tannin: 1f);
            GrapeData white = MakeGrape("White", false, tannin: 1f);
            GrapeData tannicRed = MakeGrape("TannicRed", true, tannin: 9f);
            GrapeData tannicWhite = MakeGrape("TannicWhite", false, tannin: 9f);
            GuestOrder halfRed = Order(settings, (red, 50), (white, 50));
            var served = new Blend();
            served.Add(tannicRed, 50);
            served.Add(tannicWhite, 50);

            ServeResult result = Judge(settings, halfRed, served, 1f);

            Assert.AreEqual(judged, result.Feedback.Contains("Tannin: way too high"));
        }

        // Acidity has a range of 9: each error is a share of that range.
        [TestCase(0.5f, null)]
        [TestCase(1.5f, "Acidity: a little too high")]
        [TestCase(-1.5f, "Acidity: a little too low")]
        [TestCase(3f, "Acidity: way too high")]
        [TestCase(-3f, "Acidity: way too low")]
        public void Feedback_SaysHowFarOffAPropertyIs(float acidityOff, string expected)
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData wanted = MakeGrape("Wanted", true, acidity: 5f);
            GrapeData served = MakeGrape("Served", true, acidity: 5f + acidityOff);

            ServeResult result = Judge(settings, Order(settings, wanted), Pure(served), 1f);

            if (expected == null)
            {
                Assert.AreEqual(0, result.Feedback.Count, "Close enough says nothing");
            }
            else
            {
                CollectionAssert.AreEqual(new[] { expected }, result.Feedback);
            }
        }

        [Test]
        public void Feedback_ThresholdsComeFromJudgeSettings()
        {
            BarEveningSettings settings = MakeSettings();
            settings.judge.closeEnoughError = 0.5f;
            GrapeData wanted = MakeGrape("Wanted", true, acidity: 5f);
            GrapeData served = MakeGrape("Served", true, acidity: 8f);

            ServeResult result = Judge(settings, Order(settings, wanted), Pure(served), 1f);

            Assert.AreEqual(0, result.Feedback.Count);
        }

        [Test]
        public void PropertyOffByTheWorstError_GetsNoPointsForIt()
        {
            BarEveningSettings settings = MakeSettings();
            JudgeSettings rules = settings.judge;
            GrapeData wanted = MakeGrape("Wanted", true, acidity: 1f);
            GrapeData served = MakeGrape("Served", true, acidity: 1f + rules.worstError * rules.acidity.range * 2f);

            ServeResult result = Judge(settings, Order(settings, wanted), Pure(served), 1f);

            // Weights: color 2, acidity 1, alcohol 1, tannin 1, body 1, sweetness 1 = 7. Acidity gets 0 of its 1.
            Assert.AreEqual(rules.propertiesWeight * 6f / 7f + rules.notesWeight, result.Score, ScoreTolerance);
        }

        [TestCase(0.95f, "Perfect! Exactly what I wanted!")]
        [TestCase(0.9f, "Perfect! Exactly what I wanted!")]
        [TestCase(0.89f, "Lovely, thank you.")]
        [TestCase(0.75f, "Lovely, thank you.")]
        [TestCase(0.74f, "It's okay, I guess.")]
        [TestCase(0.5f, "It's okay, I guess.")]
        [TestCase(0.49f, "This is not what I asked for.")]
        [TestCase(0f, "This is not what I asked for.")]
        public void Reaction_DependsOnTheScoreThresholds(float score, string reaction)
        {
            var judge = new BlendJudge(MakeSettings());

            Assert.AreEqual(reaction, Reflect.Call(judge, "Reaction", score));
        }

        [TestCase(1f, 60)]
        [TestCase(0.5f, 55)]
        [TestCase(0f, 50)]
        public void PerfectGlass_TipGrowsWithThePatienceLeft(float patienceLeft, int pay)
        {
            BarEveningSettings settings = MakeSettings();
            GrapeData grape = MakeGrape("Grape", true);

            Assert.AreEqual(pay, Judge(settings, Order(settings, grape), Pure(grape), patienceLeft).Pay);
        }

        [Test]
        public void ScoreAtOrBelowTipScore_GetsNoTip()
        {
            BarEveningSettings settings = MakeSettings();
            settings.tipScore = 1f;
            GrapeData grape = MakeGrape("Grape", true);

            ServeResult result = Judge(settings, Order(settings, grape), Pure(grape), 1f);

            Assert.AreEqual(settings.basePay + settings.payForScore, result.Pay);
        }

        // ---------- Helpers ----------

        private static ServeResult Judge(BarEveningSettings settings, GuestOrder order, Blend served, float patienceLeft)
        {
            var wine = new WineProfile(served, settings.vocabulary.noteThreshold);
            return new BlendJudge(settings).Judge(order, wine, patienceLeft);
        }

        private static GuestOrder Order(BarEveningSettings settings, GrapeData grape, params string[] notes)
        {
            return new GuestOrder(TestData.Recipe("Guest", notes, (grape, Blend.Full)), settings.vocabulary);
        }

        private static GuestOrder Order(BarEveningSettings settings, params (GrapeData grape, int percent)[] shares)
        {
            return new GuestOrder(TestData.Recipe("Guest", null, shares), settings.vocabulary);
        }
    }
}
