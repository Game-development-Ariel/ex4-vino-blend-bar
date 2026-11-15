using NUnit.Framework;
using UnityEngine;

namespace VinoBlendBar.Tests.EditMode
{
    public class BlendTests : CleanTestBase
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
        public void NewBlend_IsEmpty()
        {
            var blend = new Blend();

            Assert.AreEqual(0, blend.Total);
            Assert.IsFalse(blend.IsFull);
            Assert.AreEqual(0, blend.Shares.Count);
            Assert.AreEqual("empty", blend.Describe());
        }

        [Test]
        public void Full_IsOneHundredPercent()
        {
            Assert.AreEqual(100, Blend.Full);
        }

        [Test]
        public void Add_UpToExactlyFull_MakesTheBlendFull()
        {
            var blend = new Blend();

            Assert.IsTrue(blend.Add(syrah, 70));
            Assert.IsFalse(blend.IsFull);
            Assert.IsTrue(blend.Add(grenache, 30));

            Assert.AreEqual(Blend.Full, blend.Total);
            Assert.IsTrue(blend.IsFull);
        }

        [Test]
        public void Add_ThatWouldOverflow_IsRefusedAndAddsNothing()
        {
            var blend = new Blend();
            blend.Add(syrah, 95);

            Assert.IsFalse(blend.Add(grenache, 10));

            Assert.AreEqual(95, blend.Total);
            Assert.IsFalse(blend.Shares.ContainsKey(grenache));
        }

        [Test]
        public void Add_ToAFullBlend_IsRefused()
        {
            Blend blend = Pure(syrah);

            Assert.IsFalse(blend.Add(syrah, 1));
            Assert.AreEqual(Blend.Full, blend.Total);
        }

        [Test]
        public void Add_TheSameGrapeTwice_SumsItsShare()
        {
            var blend = new Blend();
            blend.Add(syrah, 30);
            blend.Add(syrah, 40);

            Assert.AreEqual(70, blend.Shares[syrah]);
            Assert.AreEqual(1, blend.Shares.Count);
        }

        // The glass adds Mathf.Min(pour, room left), so any pour size ends at exactly 100 and never above.
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(7)]
        [TestCase(10)]
        [TestCase(30)]
        [TestCase(100)]
        public void PouringTheRoomLeft_EndsAtExactlyFull_WithAnyPourSize(int pour)
        {
            var blend = new Blend();
            int pours = 0;
            while (!blend.IsFull)
            {
                int room = Blend.Full - blend.Total;
                Assert.IsTrue(blend.Add(syrah, Mathf.Min(pour, room)));
                Assert.LessOrEqual(blend.Total, Blend.Full);
                pours++;
            }

            Assert.AreEqual(Blend.Full, blend.Total);
            Assert.AreEqual(Mathf.CeilToInt(Blend.Full / (float)pour), pours);
        }

        [Test]
        public void Clear_EmptiesTheBlend()
        {
            Blend blend = Pure(syrah);

            blend.Clear();

            Assert.AreEqual(0, blend.Total);
            Assert.AreEqual(0, blend.Shares.Count);
            Assert.AreEqual("empty", blend.Describe());
        }

        [Test]
        public void Describe_ListsTheSharesInPourOrder()
        {
            var blend = new Blend();
            blend.Add(syrah, 70);
            blend.Add(grenache, 30);

            Assert.AreEqual("70% Syrah + 30% Grenache", blend.Describe());
        }

        [Test]
        public void Describe_ShowsAHalfGlass()
        {
            var blend = new Blend();
            blend.Add(grenache, 40);

            Assert.AreEqual("40% Grenache", blend.Describe());
            Assert.AreEqual(40, blend.Total);
        }
    }
}
