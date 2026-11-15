using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.PlayMode
{
    public class SmokeTests : BarSceneTestBase
    {
        [UnityTest]
        public IEnumerator Scene_RunsThreeSecondsWithoutErrors()
        {
            yield return new WaitForSeconds(3f);

            LogAssert.NoUnexpectedReceived();
            Assert.AreEqual(0, GaveUp);
        }

        [UnityTest]
        public IEnumerator Scene_StartsWithTheFirstGuestAtTheCounter()
        {
            yield return null;

            Assert.AreEqual(1, Evening.GuestNumber);
            Assert.AreEqual(3, Evening.GuestCount);
            Assert.AreEqual("Noa", Evening.Order.GuestName);
            Assert.IsTrue(Bartender.enabled);
            Assert.AreEqual(0, Glass.Blend.Total);
            Assert.That(Patience.Fraction, Is.InRange(0.9f, 1f));
            Assert.AreEqual(0, Evening.Score.Money);
        }

        [UnityTest]
        public IEnumerator Patience_RunsDownWithTime()
        {
            float before = Patience.SecondsLeft;

            yield return new WaitForSeconds(0.5f);

            Assert.Less(Patience.SecondsLeft, before);
        }
    }
}
