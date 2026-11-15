using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>The saved WineBar scene is wired the way the game needs it.</summary>
    public class SceneWiringTests
    {
        private Scene scene;

        [OneTimeSetUp]
        public void OpenTheScene()
        {
            scene = EditorSceneManager.OpenScene(TestData.ScenePath, OpenSceneMode.Single);
        }

        [OneTimeTearDown]
        public void CloseTheScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void Scene_HasNoMissingScripts()
        {
            CollectionAssert.IsEmpty(SceneChecks.ObjectsWithMissingScripts(scene));
        }

        [Test]
        public void Scene_HasNoUnassignedReferences()
        {
            CollectionAssert.IsEmpty(SceneChecks.UnassignedReferences(scene));
        }

        // The check above would be worthless if it never found anything.
        [Test]
        public void UnassignedReferenceCheck_FindsAnEmptyField()
        {
            var unwired = new GameObject("Unwired");
            try
            {
                unwired.AddComponent<GlassReadout>();

                CollectionAssert.Contains(SceneChecks.UnassignedReferences(scene), "Unwired/GlassReadout.glass");
            }
            finally
            {
                Object.DestroyImmediate(unwired);
            }
        }

        [Test]
        public void Scene_IsTheFirstSceneOfTheBuild()
        {
            Assert.IsNotEmpty(EditorBuildSettings.scenes);
            Assert.AreEqual(TestData.ScenePath, EditorBuildSettings.scenes[0].path);
            Assert.IsTrue(EditorBuildSettings.scenes[0].enabled);
        }

        [TestCase(typeof(BarEvening))]
        [TestCase(typeof(Glass))]
        [TestCase(typeof(Bartender))]
        [TestCase(typeof(Patience))]
        [TestCase(typeof(WorldClicker))]
        [TestCase(typeof(BarHud))]
        [TestCase(typeof(GuestPanel))]
        [TestCase(typeof(GlassReadout))]
        [TestCase(typeof(NotesBoard))]
        [TestCase(typeof(OrderButtons))]
        [TestCase(typeof(EveningEndPanel))]
        [TestCase(typeof(GlassFill))]
        public void Scene_HasExactlyOne(System.Type component)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                count += root.GetComponentsInChildren(component, true).Length;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Scene_HasOneBottlePerGrape_AllOnTheBartendersList()
        {
            List<GrapeBottle> bottles = SceneChecks.All<GrapeBottle>(scene);
            GrapeBottle[] bartendersBottles = Reflect.Get<GrapeBottle[]>(SceneChecks.All<Bartender>(scene)[0], "bottles");

            Assert.AreEqual(TestData.GrapeAssetNames.Length, bottles.Count);
            CollectionAssert.AreEquivalent(bottles, bartendersBottles);
            var grapes = new List<GrapeData>();
            foreach (GrapeBottle bottle in bottles)
            {
                grapes.Add(bottle.Grape);
            }
            foreach (string name in TestData.GrapeAssetNames)
            {
                CollectionAssert.Contains(grapes, TestData.Grape(name));
            }
        }

        [Test]
        public void Scene_EveryBottleCanBeClicked()
        {
            foreach (GrapeBottle bottle in SceneChecks.All<GrapeBottle>(scene))
            {
                Assert.IsNotNull(bottle.GetComponent<Collider2D>(), bottle.name);
            }
        }

        [Test]
        public void Scene_TheGlassMouthIsATrigger()
        {
            Glass glass = SceneChecks.All<Glass>(scene)[0];

            Assert.IsTrue(glass.GetComponent<Collider2D>().isTrigger);
        }

        [Test]
        public void Scene_TheEveningUsesDay1Settings()
        {
            BarEvening evening = SceneChecks.All<BarEvening>(scene)[0];

            Assert.AreSame(TestData.Day1, Reflect.Get<BarEveningSettings>(evening, "settings"));
        }

        [Test]
        public void DropPrefab_IsAFlyingTriggerWithAWineDrop()
        {
            WineDrop drop = Reflect.Get<WineDrop>(SceneChecks.All<Bartender>(scene)[0], "dropPrefab");

            Assert.IsTrue(EditorUtility.IsPersistent(drop), "The drop is a prefab asset, not a scene object");
            Rigidbody2D body = drop.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(body);
            Assert.AreEqual(RigidbodyType2D.Dynamic, body.bodyType);
            Assert.Greater(body.gravityScale, 0f);
            Assert.IsTrue(drop.GetComponent<Collider2D>().isTrigger);
        }
    }
}
