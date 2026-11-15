using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>The data assets in Assets/Data load and make sense together.</summary>
    public class DataAssetTests
    {
        [Test]
        public void EveryAssetInData_LoadsWithItsScript()
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/Data" });
            int assets = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }
                assets++;
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path), $"{path} does not load (missing script?)");
            }
            Assert.AreEqual(11, assets, "8 grapes, Day1Settings, WineVocabulary and JudgeSettings");
        }

        [Test]
        public void TheEightGrapes_AllLoad()
        {
            string[] guids = AssetDatabase.FindAssets("t:GrapeData", new[] { "Assets/Data/Grapes" });

            Assert.AreEqual(TestData.GrapeAssetNames.Length, guids.Length);
            foreach (string name in TestData.GrapeAssetNames)
            {
                Assert.IsNotNull(TestData.Grape(name));
            }
        }

        [Test]
        public void EveryGrape_HasANameNotesAHintAndValuesOnTheScale()
        {
            var names = new HashSet<string>();
            foreach (string assetName in TestData.GrapeAssetNames)
            {
                GrapeData grape = TestData.Grape(assetName);

                Assert.IsTrue(names.Add(grape.displayName), $"Two grapes are called {grape.displayName}");
                Assert.AreEqual(assetName, grape.displayName.Replace(" ", ""), "The file is named after the grape");
                Assert.IsNotEmpty(grape.notes, grape.displayName);
                Assert.AreEqual(2, grape.hint.Split('\n').Length, $"{grape.displayName}: the hint is two lines under the bottle");
                foreach (float value in new[] { grape.acidity, grape.tannin, grape.body, grape.sweetness })
                {
                    Assert.That(value, Is.InRange(1f, 10f), grape.displayName);
                }
                Assert.That(grape.alcohol, Is.InRange(10f, 15f), grape.displayName);
                Assert.AreEqual(1f, grape.liquidColor.a, grape.displayName);
            }
        }

        [Test]
        public void Day1Settings_UsesTheVocabularyAndJudgeAssets()
        {
            BarEveningSettings day1 = TestData.Day1;

            Assert.AreSame(TestData.Vocabulary, day1.vocabulary);
            Assert.AreSame(TestData.Judge, day1.judge);
        }

        [Test]
        public void Day1Settings_HasThreeValidGuests()
        {
            BarEveningSettings day1 = TestData.Day1;

            Assert.AreEqual(3, day1.guests.Length);
            foreach (GuestRecipe guest in day1.guests)
            {
                Assert.IsNull(guest.Problem(), guest.guestName);
            }
        }

        // Regression: the default guests took their grapes by place on the shelf (grapes[2]),
        // so adding or moving a grape changed the guests. Now they are found by name.
        [TestCase(0, "Noa", "Pinot Noir")]
        [TestCase(1, "Itai", "Riesling")]
        [TestCase(2, "Maya", "Syrah", "Grenache")]
        public void Regression_GuestsGrapesByName_Day1GuestsDrinkTheGrapesOfTheLevelDesign(int index, string name, params string[] grapes)
        {
            GuestRecipe guest = TestData.Day1.guests[index];

            Assert.AreEqual(name, guest.guestName);
            Assert.AreEqual(grapes.Length, guest.blend.Length);
            for (int i = 0; i < grapes.Length; i++)
            {
                Assert.AreEqual(grapes[i], guest.blend[i].grape.displayName);
            }
        }

        // Otherwise even the exact recipe could not get the notes part of the score.
        [Test]
        public void EveryRequestedNote_ComesFromTheGuestsOwnRecipe()
        {
            BarEveningSettings day1 = TestData.Day1;
            foreach (GuestRecipe guest in day1.guests)
            {
                GuestOrder order = new GuestOrder(guest, day1.vocabulary);
                foreach (string note in order.RequestedNotes)
                {
                    CollectionAssert.Contains(order.Target.Notes, note, $"{guest.guestName} asks for {note}");
                }
            }
        }

        [Test]
        public void Day1Settings_NumbersMakeSense()
        {
            BarEveningSettings day1 = TestData.Day1;

            Assert.Greater(day1.patienceSeconds, day1.tasteCostSeconds);
            Assert.That(day1.starsToPass, Is.InRange(0f, day1.maxStars));
            Assert.That(day1.tipScore, Is.InRange(0f, 1f));
            Assert.GreaterOrEqual(day1.basePay, 0);
        }

        [Test]
        public void JudgeSettings_PartsOfTheScoreSumToOne()
        {
            JudgeSettings judge = TestData.Judge;

            Assert.AreEqual(1f, judge.propertiesWeight + judge.notesWeight, 1e-5f);
            Assert.Less(judge.closeEnoughError, judge.littleOffError);
            Assert.Less(judge.okayScore, judge.lovelyScore);
            Assert.Less(judge.lovelyScore, judge.perfectScore);
            foreach (JudgeSettings.Property property in new[] { judge.color, judge.acidity, judge.alcohol, judge.tannin, judge.body, judge.sweetness })
            {
                Assert.Greater(property.range, 0f);
                Assert.Greater(property.weight, 0f);
            }
        }

        [Test]
        public void WineVocabulary_ThresholdsGoUp()
        {
            WineVocabulary v = TestData.Vocabulary;

            Assert.Less(v.whiteBelow, v.redAbove);
            Assert.Less(v.dryBelow, v.offDryBelow);
            Assert.Less(v.softAcidityBelow, v.balancedAcidityBelow);
            Assert.Less(v.balancedAcidityBelow, v.crispAcidityBelow);
            Assert.Less(v.noTanninsBelow, v.silkyTanninsBelow);
            Assert.Less(v.silkyTanninsBelow, v.mediumTanninsBelow);
            Assert.Less(v.lightBodyBelow, v.mediumBodyBelow);
            Assert.Less(v.lowAlcoholBelow, v.moderateAlcoholBelow);
            Assert.Less(v.moderateAlcoholBelow, v.strongAlcoholBelow);
            Assert.That(v.noteThreshold, Is.InRange(1, Blend.Full));
        }
    }
}
