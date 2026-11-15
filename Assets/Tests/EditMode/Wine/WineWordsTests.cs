using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class WineWordsTests : CleanTestBase
    {
        private const float Step = 0.01f;

        private WineVocabulary scale;

        [SetUp]
        public void MakeVocabulary()
        {
            scale = Make<WineVocabulary>();
        }

        // Each "...Below" threshold: just below it gives one word, at it and above it the next word.
        private static IEnumerable<TestCaseData> BelowThresholds()
        {
            yield return Case("dryBelow", WineWords.Sweetness, "dry", "off-dry");
            yield return Case("offDryBelow", WineWords.Sweetness, "off-dry", "slightly sweet");
            yield return Case("softAcidityBelow", WineWords.Acidity, "soft, gentle acidity", "balanced acidity");
            yield return Case("balancedAcidityBelow", WineWords.Acidity, "balanced acidity", "fresh, crisp acidity");
            yield return Case("crispAcidityBelow", WineWords.Acidity, "fresh, crisp acidity", "zingy, mouth-watering acidity");
            yield return Case("noTanninsBelow", WineWords.Tannin, "no tannins", "silky tannins");
            yield return Case("silkyTanninsBelow", WineWords.Tannin, "silky tannins", "medium tannins");
            yield return Case("mediumTanninsBelow", WineWords.Tannin, "medium tannins", "firm, grippy tannins");
            yield return Case("lightBodyBelow", WineWords.Body, "a light body", "a medium body");
            yield return Case("mediumBodyBelow", WineWords.Body, "a medium body", "a full body");
            yield return Case("lowAlcoholBelow", WineWords.Alcohol, "light and low in alcohol", "moderate in alcohol");
            yield return Case("moderateAlcoholBelow", WineWords.Alcohol, "moderate in alcohol", "fairly strong");
            yield return Case("strongAlcoholBelow", WineWords.Alcohol, "fairly strong", "warm and high in alcohol");
            yield return Case("whiteBelow", WineWords.Color, "white", "rose-style");
        }

        private static TestCaseData Case(string field, Func<float, WineVocabulary, string> words, string below, string atAndAbove)
        {
            return new TestCaseData(field, words, below, atAndAbove).SetName($"Threshold_{field}_SwitchesFrom_{below}_To_{atAndAbove}");
        }

        [TestCaseSource(nameof(BelowThresholds))]
        public void EachThreshold_GivesTheExpectedWordBelowAtAndAbove(string field, Func<float, WineVocabulary, string> words,
            string below, string atAndAbove)
        {
            float threshold = Reflect.Get<float>(scale, field);

            Assert.AreEqual(below, words(threshold - Step, scale), "just below");
            Assert.AreEqual(atAndAbove, words(threshold, scale), "at");
            Assert.AreEqual(atAndAbove, words(threshold + Step, scale), "just above");
        }

        // redAbove is the only "above" threshold: at it the wine is still rose-style.
        [Test]
        public void Color_IsRedOnlyAboveRedAbove()
        {
            Assert.AreEqual("rose-style", WineWords.Color(scale.redAbove - Step, scale));
            Assert.AreEqual("rose-style", WineWords.Color(scale.redAbove, scale));
            Assert.AreEqual("red", WineWords.Color(scale.redAbove + Step, scale));
        }

        [TestCase(0f, "white")]
        [TestCase(0.5f, "rose-style")]
        [TestCase(1f, "red")]
        public void Color_OfPureAndMixedWines(float redness, string expected)
        {
            Assert.AreEqual(expected, WineWords.Color(redness, scale));
        }

        [Test]
        public void Words_FollowTheVocabularyAsset_NotFixedNumbers()
        {
            scale.dryBelow = 1f;

            Assert.AreEqual("off-dry", WineWords.Sweetness(2f, scale));
        }

        [Test]
        public void Sentence_OfARed_MentionsTannins()
        {
            WineProfile wine = Wine(MakeGrape("Red", true, acidity: 5f, tannin: 7f, body: 7f, sweetness: 2f));

            Assert.AreEqual("a dry red with balanced acidity, firm, grippy tannins and a full body", WineWords.Sentence(wine, scale));
        }

        [Test]
        public void Sentence_OfAWhite_DoesNotMentionTannins()
        {
            WineProfile wine = Wine(MakeGrape("White", false, acidity: 9f, tannin: 7f, body: 3f, sweetness: 2f));

            Assert.AreEqual("a dry white with zingy, mouth-watering acidity and a light body", WineWords.Sentence(wine, scale));
        }

        [Test]
        public void Sentence_RednessExactlyAtWhiteBelow_DoesNotMentionTannins()
        {
            WineProfile wine = Mix(Wine25PercentRed());

            StringAssert.DoesNotContain("tannins", WineWords.Sentence(wine, scale));
        }

        [Test]
        public void Sentence_RednessJustAboveWhiteBelow_MentionsTannins()
        {
            GrapeData red = MakeGrape("Red", true, tannin: 7f);
            GrapeData white = MakeGrape("White", false, tannin: 7f);
            var blend = new Blend();
            blend.Add(red, 26);
            blend.Add(white, 74);

            StringAssert.Contains("tannins", WineWords.Sentence(new WineProfile(blend, 20), scale));
        }

        [Test]
        public void Sentence_UsesTheArticleAn_ForOffDry()
        {
            WineProfile wine = Wine(MakeGrape("OffDry", false, sweetness: 4f));

            StringAssert.StartsWith("an off-dry white", WineWords.Sentence(wine, scale));
        }

        [Test]
        public void Sentence_UsesTheArticleA_ForSlightlySweet()
        {
            WineProfile wine = Wine(MakeGrape("Sweet", false, sweetness: 6f));

            StringAssert.StartsWith("a slightly sweet white", WineWords.Sentence(wine, scale));
        }

        [Test]
        public void Taste_OfAnEmptyGlass_SaysItIsEmpty()
        {
            Assert.AreEqual("The glass is empty.", WineWords.Taste(new WineProfile(new Blend(), 20), scale));
        }

        [Test]
        public void Taste_WithNotes_ListsThem()
        {
            WineProfile wine = Wine(MakeGrape("Red", true, 5f, 14.5f, 7f, 8f, 2f, "blackberry", "smoke"));

            Assert.AreEqual("It tastes like a dry red with balanced acidity, firm, grippy tannins and a full body. " +
                            "It feels warm and high in alcohol.\nNotes of blackberry, smoke.", WineWords.Taste(wine, scale));
        }

        [Test]
        public void Taste_WithoutNotes_SaysNoAromaStandsOut()
        {
            WineProfile wine = Wine(MakeGrape("Plain", false));

            StringAssert.EndsWith("\nNo aroma stands out.", WineWords.Taste(wine, scale));
        }

        private WineProfile Wine(GrapeData grape)
        {
            return new WineProfile(Pure(grape), scale.noteThreshold);
        }

        private static WineProfile Mix(Blend blend)
        {
            return new WineProfile(blend, 20);
        }

        // 25% red, exactly at whiteBelow (0.25).
        private Blend Wine25PercentRed()
        {
            var blend = new Blend();
            blend.Add(MakeGrape("Red", true, tannin: 7f), 25);
            blend.Add(MakeGrape("White", false, tannin: 7f), 75);
            return blend;
        }
    }
}
