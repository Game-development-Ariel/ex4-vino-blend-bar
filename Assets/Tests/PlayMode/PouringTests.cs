using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>The physics of pouring: a click throws a splash (Rigidbody2D + AddForce) that lands in the glass.</summary>
    public class PouringTests : BarSceneTestBase
    {
        [UnityTest]
        public IEnumerator ClickingABottle_ThrowsADropThatLandsAndAddsTenPercentOfItsGrape()
        {
            GrapeBottle pinot = Bottle("Pinot Noir");

            pinot.OnClicked();

            Assert.AreEqual(1, DropsInTheAir(), "A splash is in the air");
            Assert.AreEqual(0, Glass.Blend.Total, "It hasn't landed yet");
            yield return WaitUntil(() => Glass.Blend.Total > 0, "the splash to land");
            Assert.AreEqual(10, Glass.Blend.Total);
            Assert.AreEqual(10, Glass.Blend.Shares[pinot.Grape]);
            yield return null;
            Assert.AreEqual(0, DropsInTheAir(), "The splash is gone once it lands");
        }

        [UnityTest]
        public IEnumerator EveryBottleOnTheShelf_HitsTheGlass()
        {
            Time.timeScale = 4f;
            foreach (GrapeBottle bottle in Bottles)
            {
                bottle.OnClicked();
            }

            yield return WaitForDropsToLand();

            Assert.AreEqual(Bottles.Length * 10, Glass.Blend.Total);
            foreach (GrapeBottle bottle in Bottles)
            {
                Assert.AreEqual(10, Glass.Blend.Shares[bottle.Grape], bottle.Grape.displayName);
            }
        }

        // Regression: a pour size that doesn't divide 100 (like 30) left the glass at 90 and Serve never worked.
        [UnityTest]
        public IEnumerator Regression_OverflowClamp_AnyPourSizeFillsTheGlassToExactly100([Values(3, 7, 30, 100)] int pour)
        {
            Time.timeScale = 4f;
            PlayReflect.Set(Bartender, "percentPerPour", pour);
            int clicks = Mathf.CeilToInt(100f / pour) + 1; // One more than fits: its wine is lost.

            for (int i = 0; i < clicks; i++)
            {
                Bottle("Syrah").OnClicked();
            }
            yield return WaitForDropsToLand();

            Assert.AreEqual(100, Glass.Blend.Total);
            Assert.IsTrue(Glass.Blend.IsFull);
        }

        [UnityTest]
        public IEnumerator Glass_IgnoresSomethingThatIsNotADrop()
        {
            yield return null;
            int changes = 0;
            Glass.Changed += () => changes++;

            PlayReflect.Call(Glass, "OnTriggerEnter2D", Bottle("Merlot").GetComponent<Collider2D>());

            Assert.AreEqual(0, Glass.Blend.Total);
            Assert.AreEqual(0, changes);
            Assert.IsTrue(Bottle("Merlot") != null, "The glass doesn't destroy what isn't wine");
        }

        [UnityTest]
        public IEnumerator ClickingWithAFullGlass_ThrowsNothing()
        {
            FillGlass("Merlot", 100);
            yield return null;

            Bottle("Merlot").OnClicked();

            Assert.AreEqual(0, DropsInTheAir());
        }

        // Regression: splashes thrown just before "Empty glass" landed in the glass that was just emptied.
        [UnityTest]
        public IEnumerator Regression_DropsInFlight_AreDestroyedWhenTheGlassIsEmptied()
        {
            Bottle("Syrah").OnClicked();
            Bottle("Merlot").OnClicked();
            Assert.AreEqual(2, DropsInTheAir());

            Glass.Empty();
            yield return null;

            Assert.AreEqual(0, DropsInTheAir());
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0, Glass.Blend.Total, "Nothing landed after emptying");
        }

        // Regression: a splash in the air when the guest was served landed in the next guest's glass.
        [UnityTest]
        public IEnumerator Regression_DropsInFlight_AreDestroyedWhenTheGuestIsServed()
        {
            FillGlass("Pinot Noir", 90);
            Bottle("Syrah").OnClicked();
            DropIntoGlass("Pinot Noir", 10);

            Evening.Serve();
            yield return null;

            Assert.AreEqual(1, Served.Count);
            Assert.AreEqual(0, DropsInTheAir());
            Evening.NextGuest();
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0, Glass.Blend.Total, "Nothing landed in the next guest's glass");
        }

        [UnityTest]
        public IEnumerator Regression_DropsInFlight_AreDestroyedWhenTheGuestGivesUp()
        {
            Bottle("Syrah").OnClicked();

            Patience.Spend(1000f);
            yield return null;

            Assert.AreEqual(1, GaveUp);
            Assert.AreEqual(0, DropsInTheAir());
        }

        [UnityTest]
        public IEnumerator MissedDrop_IsDestroyedAfterItsLifetime()
        {
            WineDrop prefab = PlayReflect.Get<WineDrop>(Bartender, "dropPrefab");
            WineDrop drop = Object.Instantiate(prefab, new Vector3(-40f, 0f, 0f), Quaternion.identity);
            drop.Setup(Grape("Syrah"), 10);
            float lifetime = PlayReflect.Get<float>(drop, "lifetimeSeconds");
            Time.timeScale = 4f;

            yield return new WaitForSeconds(lifetime * 0.5f);
            Assert.IsTrue(drop != null, "Still falling halfway through its lifetime");

            yield return new WaitForSeconds(lifetime * 0.5f + 0.2f);
            Assert.IsTrue(drop == null, "Destroyed after its lifetime");
            Assert.AreEqual(0, Glass.Blend.Total);
        }

        // Regression: the throw ignored the splash's gravityScale, so a heavier or lighter splash missed the glass.
        [UnityTest]
        public IEnumerator Regression_GravityScale_TheThrowStillLandsInTheGlass([Values(0.5f, 3f)] float gravityScale)
        {
            WineDrop prefab = PlayReflect.Get<WineDrop>(Bartender, "dropPrefab");
            // A copy of the prefab with another gravity scale. It is thrown from in the same frame,
            // before any physics step, and destroyed at the end of the frame.
            WineDrop template = Object.Instantiate(prefab, new Vector3(500f, 500f, 0f), Quaternion.identity);
            template.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
            PlayReflect.Set(Bartender, "dropPrefab", template);

            Bottle("Cabernet Sauvignon").OnClicked();
            PlayReflect.Set(Bartender, "dropPrefab", prefab);
            Object.Destroy(template.gameObject);

            yield return WaitUntil(() => Glass.Blend.Total > 0, "the splash to land");
            Assert.AreEqual(10, Glass.Blend.Total);
        }

        [UnityTest]
        public IEnumerator AfterServing_ClickingABottleThrowsNothing()
        {
            FillGlass("Pinot Noir", 100);
            Evening.Serve();
            yield return null;

            Assert.IsFalse(Bartender.enabled);
            Bottle("Pinot Noir").OnClicked();
            Assert.AreEqual(0, DropsInTheAir());
        }

        [UnityTest]
        public IEnumerator WhenTheNextGuestArrives_TheBottlesWorkAgain()
        {
            FillGlass("Pinot Noir", 100);
            Evening.Serve();
            Evening.NextGuest();
            yield return null;

            Bottle("Riesling").OnClicked();

            Assert.AreEqual(1, DropsInTheAir());
        }
    }
}
