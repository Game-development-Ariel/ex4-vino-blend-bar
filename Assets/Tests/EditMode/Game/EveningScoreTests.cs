using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    public class EveningScoreTests : CleanTestBase
    {
        private BarEveningSettings settings;
        private EveningScore score;

        [SetUp]
        public void MakeScore()
        {
            settings = MakeSettings(3);
            score = new EveningScore(settings);
        }

        [Test]
        public void NewEvening_HasNoMoneyAndNoStars()
        {
            Assert.AreEqual(0, score.Money);
            Assert.AreEqual(0f, score.Stars);
            Assert.IsFalse(score.Passed);
        }

        [Test]
        public void Money_IsTheSumOfThePays()
        {
            score.Add(Result(1f, 60));
            score.Add(Result(0.5f, 32));

            Assert.AreEqual(92, score.Money);
        }

        [Test]
        public void Stars_AreTheAverageScoreOfAllGuestsInStars()
        {
            score.Add(Result(1f));
            score.Add(Result(0.5f));
            score.Add(Result(0.3f));

            Assert.AreEqual(3f, score.Stars, 1e-5f);
        }

        [Test]
        public void GuestWhoLeft_CountsAsZero()
        {
            score.Add(Result(1f));
            score.Add(Result(1f));

            Assert.AreEqual(3.3f, score.Stars, 1e-5f);
        }

        [Test]
        public void PerfectEvening_GetsMaxStars()
        {
            for (int i = 0; i < 3; i++)
            {
                score.Add(Result(1f));
            }

            Assert.AreEqual(settings.maxStars, score.Stars, 1e-5f);
            Assert.IsTrue(score.Passed);
        }

        // Regression: 2.96 stars were shown as "3.0" and still lost. The stars are rounded once,
        // to the one decimal that is shown, and the pass check uses the rounded number.
        [Test]
        public void Regression_StarRounding_296StarsShowAs30AndPass()
        {
            AddEach(0.592f);

            Assert.AreEqual(3f, score.Stars, 1e-5f);
            Assert.AreEqual("3.0", score.Stars.ToString("0.0"));
            Assert.IsTrue(score.Passed);
        }

        [Test]
        public void StarRounding_294StarsShowAs29AndLose()
        {
            AddEach(0.588f);

            Assert.AreEqual(2.9f, score.Stars, 1e-5f);
            Assert.IsFalse(score.Passed);
        }

        [Test]
        public void Stars_AreRoundedToOneDecimal()
        {
            score.Add(Result(1f));
            score.Add(Result(0.5f));
            score.Add(Result(0.31f));

            // 1.81 / 3 * 5 = 3.0166...
            Assert.AreEqual(3f, score.Stars, 1e-5f);
        }

        [Test]
        public void ExactlyStarsToPass_Passes()
        {
            AddEach(0.6f);

            Assert.AreEqual(settings.starsToPass, score.Stars, 1e-5f);
            Assert.IsTrue(score.Passed);
        }

        [Test]
        public void StarsToPass_ComesFromTheSettings()
        {
            settings.starsToPass = 4f;
            AddEach(0.7f);

            Assert.AreEqual(3.5f, score.Stars, 1e-5f);
            Assert.IsFalse(score.Passed);
        }

        [Test]
        public void EveningWithoutGuests_HasZeroStars()
        {
            settings.guests = null;

            Assert.AreEqual(0f, score.Stars);
            Assert.IsFalse(score.Passed);
        }

        private void AddEach(float guestScore)
        {
            for (int i = 0; i < settings.guests.Length; i++)
            {
                score.Add(Result(guestScore));
            }
        }

        private static ServeResult Result(float guestScore, int pay = 0)
        {
            return new ServeResult { Score = guestScore, Pay = pay };
        }
    }
}
