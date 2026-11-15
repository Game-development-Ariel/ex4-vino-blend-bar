using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class GuestOrderTests : CleanTestBase
    {
        // The words of the three Day 1 guests, pinned. A change to the grapes, the vocabulary or WineWords
        // that changes what a guest says fails here.
        [TestCase(0, "Noa", "I'd like a dry red with fresh, crisp acidity, silky tannins and a light body. " +
                            "Something fairly strong. I love notes of cherry and raspberry.")]
        [TestCase(1, "Itai", "I'd like a slightly sweet white with zingy, mouth-watering acidity and a light body. " +
                             "Something light and low in alcohol. I love notes of lime and honey.")]
        [TestCase(2, "Maya", "I'd like a dry red with balanced acidity, medium tannins and a full body. " +
                             "Something warm and high in alcohol. I love notes of black pepper and strawberry.")]
        public void Day1Guest_SaysTheirOrderInTheWordsOfTheVocabulary(int guest, string name, string speech)
        {
            GuestOrder order = TestData.Day1Order(guest);

            Assert.AreEqual(name, order.GuestName);
            Assert.AreEqual(speech, order.Speech());
        }

        [Test]
        public void Speech_IsBuiltFromWineWords()
        {
            GuestOrder order = TestData.Day1Order(2);
            WineVocabulary scale = TestData.Vocabulary;

            StringAssert.Contains(WineWords.Sentence(order.Target, scale), order.Speech());
            StringAssert.Contains(WineWords.Alcohol(order.Target.Alcohol, scale), order.Speech());
        }

        [Test]
        public void Speech_WithoutRequestedNotes_DoesNotTalkAboutNotes()
        {
            GuestRecipe recipe = TestData.Recipe("Quiet", null, (TestData.Grape("Merlot"), 100));

            var order = new GuestOrder(recipe, TestData.Vocabulary);

            StringAssert.DoesNotContain("I love notes of", order.Speech());
            Assert.AreEqual(0, order.RequestedNotes.Length);
        }

        [Test]
        public void Speech_WithOneNote_NamesIt()
        {
            GuestRecipe recipe = TestData.Recipe("One", new[] { "plum" }, (TestData.Grape("Merlot"), 100));

            StringAssert.EndsWith(" I love notes of plum.", new GuestOrder(recipe, TestData.Vocabulary).Speech());
        }

        [TestCase(0, "100% Pinot Noir")]
        [TestCase(1, "100% Riesling")]
        [TestCase(2, "70% Syrah + 30% Grenache")]
        public void Recipe_IsTheRightAnswerInWords(int guest, string recipe)
        {
            Assert.AreEqual(recipe, TestData.Day1Order(guest).Recipe);
        }

        [Test]
        public void Target_IsTheProfileOfTheRecipe()
        {
            GuestOrder order = TestData.Day1Order(0);
            GrapeData pinot = TestData.Grape("PinotNoir");

            Assert.AreEqual(pinot.acidity, order.Target.Acidity, 1e-4f);
            Assert.AreEqual(1f, order.Target.Redness, 1e-4f);
            CollectionAssert.AreEqual(new[] { "cherry", "raspberry" }, order.RequestedNotes);
        }
    }
}
