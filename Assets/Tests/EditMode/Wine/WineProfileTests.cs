using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class WineProfileTests : CleanTestBase
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void EmptyBlend_IsAnEmptyProfile()
        {
            var wine = new WineProfile(new Blend(), 20);

            Assert.IsTrue(wine.IsEmpty);
            Assert.AreEqual(0f, wine.Acidity);
            Assert.AreEqual(0f, wine.Alcohol);
            Assert.AreEqual(0f, wine.Redness);
            Assert.AreEqual(0, wine.Notes.Count);
        }

        [Test]
        public void OneRealGrape_HasExactlyThatGrapesValues()
        {
            GrapeData pinot = TestData.Grape("PinotNoir");

            var wine = new WineProfile(Pure(pinot), TestData.Vocabulary.noteThreshold);

            Assert.IsFalse(wine.IsEmpty);
            Assert.AreEqual(pinot.acidity, wine.Acidity, Tolerance);
            Assert.AreEqual(pinot.alcohol, wine.Alcohol, Tolerance);
            Assert.AreEqual(pinot.tannin, wine.Tannin, Tolerance);
            Assert.AreEqual(pinot.body, wine.Body, Tolerance);
            Assert.AreEqual(pinot.sweetness, wine.Sweetness, Tolerance);
            Assert.AreEqual(1f, wine.Redness, Tolerance);
            CollectionAssert.AreEqual(pinot.notes, wine.Notes);
        }

        [Test]
        public void TwoRealGrapes_GiveTheWeightedAverage()
        {
            GrapeData syrah = TestData.Grape("Syrah");
            GrapeData grenache = TestData.Grape("Grenache");

            var wine = new WineProfile(TestData.MakeBlend("Syrah", 70, "Grenache", 30), TestData.Vocabulary.noteThreshold);

            Assert.AreEqual(syrah.acidity * 0.7f + grenache.acidity * 0.3f, wine.Acidity, Tolerance);
            Assert.AreEqual(syrah.alcohol * 0.7f + grenache.alcohol * 0.3f, wine.Alcohol, Tolerance);
            Assert.AreEqual(syrah.tannin * 0.7f + grenache.tannin * 0.3f, wine.Tannin, Tolerance);
            Assert.AreEqual(syrah.body * 0.7f + grenache.body * 0.3f, wine.Body, Tolerance);
            Assert.AreEqual(syrah.sweetness * 0.7f + grenache.sweetness * 0.3f, wine.Sweetness, Tolerance);
        }

        [Test]
        public void RedAndWhite_RednessIsTheShareOfRedGrapes()
        {
            var wine = new WineProfile(TestData.MakeBlend("PinotNoir", 60, "Riesling", 40), 20);

            Assert.AreEqual(0.6f, wine.Redness, Tolerance);
        }

        [Test]
        public void HalfAGlass_IsWeightedByWhatIsInTheGlass()
        {
            var wine = new WineProfile(TestData.MakeBlend("PinotNoir", 20, "Riesling", 20), 20);

            Assert.AreEqual(0.5f, wine.Redness, Tolerance);
        }

        [Test]
        public void Notes_AppearAtTheThreshold()
        {
            int threshold = TestData.Vocabulary.noteThreshold;

            var wine = new WineProfile(TestData.MakeBlend("Syrah", Blend.Full - threshold, "Grenache", threshold), threshold);

            CollectionAssert.IsSubsetOf(TestData.Grape("Grenache").notes, wine.Notes);
        }

        [Test]
        public void Notes_DoNotAppearJustBelowTheThreshold()
        {
            int threshold = TestData.Vocabulary.noteThreshold;

            var wine = new WineProfile(TestData.MakeBlend("Syrah", Blend.Full - threshold + 1, "Grenache", threshold - 1), threshold);

            foreach (string note in TestData.Grape("Grenache").notes)
            {
                CollectionAssert.DoesNotContain(wine.Notes, note);
            }
            CollectionAssert.IsSubsetOf(TestData.Grape("Syrah").notes, wine.Notes);
        }

        // The threshold is a share of the blend (weight * Blend.Full), not of the glass:
        // 10 of a 50% glass is 20% of the blend.
        [Test]
        public void Notes_ThresholdIsAShareOfTheBlendNotOfTheGlass()
        {
            var wine = new WineProfile(TestData.MakeBlend("Syrah", 40, "Grenache", 10), 20);

            CollectionAssert.IsSubsetOf(TestData.Grape("Grenache").notes, wine.Notes);
        }

        [Test]
        public void Notes_SharedByTwoGrapes_AreListedOnce()
        {
            GrapeData first = MakeGrape("First", true, 5f, 13f, 5f, 5f, 2f, "cherry", "plum");
            GrapeData second = MakeGrape("Second", true, 5f, 13f, 5f, 5f, 2f, "cherry");
            var blend = new Blend();
            blend.Add(first, 50);
            blend.Add(second, 50);

            var wine = new WineProfile(blend, 20);

            CollectionAssert.AreEqual(new[] { "cherry", "plum" }, wine.Notes);
        }
    }
}
