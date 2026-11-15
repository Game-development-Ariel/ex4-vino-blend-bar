using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class GuestRecipeTests : CleanTestBase
    {
        private GrapeData syrah;
        private GrapeData grenache;

        [SetUp]
        public void MakeGrapes()
        {
            syrah = MakeGrape("Syrah", true);
            grenache = MakeGrape("Grenache", true);
        }

        [Test]
        public void Problem_OfAValidRecipe_IsNull()
        {
            GuestRecipe recipe = TestData.Recipe("Maya", new[] { "pepper" }, (syrah, 70), (grenache, 30));

            Assert.IsNull(recipe.Problem());
        }

        [Test]
        public void Problem_WithoutRequestedNotes_IsNull()
        {
            Assert.IsNull(TestData.Recipe("Maya", null, (syrah, 100)).Problem());
            Assert.IsNull(TestData.Recipe("Maya", new string[0], (syrah, 100)).Problem());
        }

        [Test]
        public void Problem_WithANullBlend_SaysTheBlendIsEmpty()
        {
            var recipe = new GuestRecipe { guestName = "Nobody", blend = null };

            Assert.AreEqual("the blend is empty", recipe.Problem());
        }

        [Test]
        public void Problem_WithNoShares_SaysTheBlendIsEmpty()
        {
            Assert.AreEqual("the blend is empty", TestData.Recipe("Nobody", null).Problem());
        }

        [Test]
        public void Problem_WithAShareWithoutAGrape_IsFlagged()
        {
            GuestRecipe recipe = TestData.Recipe("Maya", null, (syrah, 70), (null, 30));

            Assert.AreEqual("a share of the blend has no grape", recipe.Problem());
        }

        [TestCase(90)]
        [TestCase(99)]
        [TestCase(101)]
        [TestCase(0)]
        public void Problem_WhenTheSharesDoNotSumToFull_SaysWhatTheySumTo(int total)
        {
            GuestRecipe recipe = TestData.Recipe("Maya", null, (syrah, total));

            Assert.AreEqual($"the blend adds up to {total}%, not 100%", recipe.Problem());
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Problem_WithAnEmptyNote_IsFlagged(string note)
        {
            GuestRecipe recipe = TestData.Recipe("Maya", new[] { "pepper", note }, (syrah, 100));

            Assert.AreEqual("one of the requested notes is empty", recipe.Problem());
        }
    }
}
