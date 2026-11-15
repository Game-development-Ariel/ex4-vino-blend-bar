using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class PatienceTests : CleanTestBase
    {
        private Patience patience;
        private int ranOut;

        [SetUp]
        public void MakePatience()
        {
            patience = MakeObject("Patience").AddComponent<Patience>();
            ranOut = 0;
            patience.RanOut += () => ranOut++;
        }

        [Test]
        public void BeforeRestart_FractionIsZero()
        {
            Assert.AreEqual(0f, patience.Fraction);
        }

        [Test]
        public void Restart_FillsThePatience()
        {
            patience.Restart(75f);

            Assert.AreEqual(75f, patience.SecondsLeft);
            Assert.AreEqual(1f, patience.Fraction);
        }

        [Test]
        public void Spend_TakesSecondsOff()
        {
            patience.Restart(75f);

            patience.Spend(5f);

            Assert.AreEqual(70f, patience.SecondsLeft, 1e-5f);
            Assert.AreEqual(70f / 75f, patience.Fraction, 1e-5f);
            Assert.AreEqual(0, ranOut);
        }

        [Test]
        public void SpendingEverything_RunsOutOnceAndStopsAtZero()
        {
            patience.Restart(10f);

            patience.Spend(30f);

            Assert.AreEqual(0f, patience.SecondsLeft);
            Assert.AreEqual(1, ranOut);
        }

        [Test]
        public void AfterRunningOut_SpendingMoreDoesNotRunOutAgain()
        {
            patience.Restart(10f);
            patience.Spend(10f);

            patience.Spend(1f);
            patience.Spend(1f);

            Assert.AreEqual(1, ranOut);
        }

        [Test]
        public void Stop_FreezesThePatience()
        {
            patience.Restart(10f);
            patience.Stop();

            patience.Spend(20f);

            Assert.AreEqual(10f, patience.SecondsLeft);
            Assert.AreEqual(0, ranOut);
        }

        [Test]
        public void Restart_AfterRunningOut_RunsAgain()
        {
            patience.Restart(10f);
            patience.Spend(10f);

            patience.Restart(20f);
            patience.Spend(20f);

            Assert.AreEqual(2, ranOut);
        }

        [Test]
        public void Spend_BeforeRestart_DoesNothing()
        {
            patience.Spend(5f);

            Assert.AreEqual(0f, patience.SecondsLeft);
            Assert.AreEqual(0, ranOut);
        }
    }
}
