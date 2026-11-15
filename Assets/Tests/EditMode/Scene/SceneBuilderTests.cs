using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// Runs the editor tools that build the scene. Every file they could write is saved before
    /// and put back after, so the tests leave the project exactly as it was.
    /// </summary>
    public class SceneBuilderTests
    {
        // The folders the builder writes to (the scene, the drop prefab, the data, the shape sprites, the build settings).
        private static readonly string[] WatchedFolders = { "Assets/Scenes", "Assets/Prefabs", "Assets/Data", "Assets/Sprites", "ProjectSettings" };

        private Dictionary<string, byte[]> before;

        [OneTimeSetUp]
        public void SaveTheFiles()
        {
            before = Snapshot();
        }

        [OneTimeTearDown]
        public void PutTheFilesBack()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (string path in Snapshot().Keys)
            {
                if (!before.ContainsKey(path))
                {
                    File.Delete(path);
                }
            }
            foreach (KeyValuePair<string, byte[]> file in before)
            {
                if (!File.Exists(file.Key) || !Same(File.ReadAllBytes(file.Key), file.Value))
                {
                    File.WriteAllBytes(file.Key, file.Value);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        [Test]
        public void BuildScene_MakesASceneWithoutMissingScriptsOrEmptyReferences()
        {
            ProjectBuilder.BuildScene();
            Scene scene = SceneManager.GetActiveScene();

            Assert.AreEqual(TestData.ScenePath, scene.path);
            CollectionAssert.IsEmpty(SceneChecks.ObjectsWithMissingScripts(scene));
            CollectionAssert.IsEmpty(SceneChecks.UnassignedReferences(scene));
        }

        [Test]
        public void BuildScene_PutsOneBottleOnTheShelfPerGrape()
        {
            ProjectBuilder.BuildScene();
            Scene scene = SceneManager.GetActiveScene();

            List<GrapeBottle> bottles = SceneChecks.All<GrapeBottle>(scene);
            Assert.AreEqual(WineBarAssets.Grapes().Length, bottles.Count);
            var names = new HashSet<string>();
            foreach (GrapeBottle bottle in bottles)
            {
                Assert.IsTrue(names.Add(bottle.Grape.displayName), $"Two bottles of {bottle.Grape.displayName}");
            }
        }

        [Test]
        public void BuildScene_Twice_GivesTheSameObjects()
        {
            ProjectBuilder.BuildScene();
            Dictionary<string, int> first = CountComponents(SceneManager.GetActiveScene());

            ProjectBuilder.BuildScene();
            Dictionary<string, int> second = CountComponents(SceneManager.GetActiveScene());

            CollectionAssert.AreEquivalent(first, second);
        }

        [Test]
        public void BuildScene_PutsTheSceneInTheBuild()
        {
            ProjectBuilder.BuildScene();

            Assert.AreEqual(1, EditorBuildSettings.scenes.Length);
            Assert.AreEqual(TestData.ScenePath, EditorBuildSettings.scenes[0].path);
        }

        [Test]
        public void BuildScene_KeepsTheTunedDataAssets()
        {
            BarEveningSettings day1 = TestData.Day1;
            string guestsBefore = JsonUtility.ToJson(day1);

            ProjectBuilder.BuildScene();

            Assert.AreEqual(guestsBefore, JsonUtility.ToJson(TestData.Day1));
        }

        // Regression: the shelves were a fixed list of two heights, so a ninth grape had no shelf.
        [Test]
        public void Regression_NinthGrape_StartsAThirdShelf()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var grapes = new GrapeData[9];
            for (int i = 0; i < grapes.Length; i++)
            {
                grapes[i] = ScriptableObject.CreateInstance<GrapeData>();
                grapes[i].displayName = "Grape " + i;
                grapes[i].hint = "one\ntwo";
            }

            try
            {
                var bottles = (GrapeBottle[])Reflect.CallStatic(typeof(WineBarSceneBuilder), "CreateCellar", new object[] { grapes });

                Assert.AreEqual(9, bottles.Length);
                Transform cellar = bottles[0].transform.parent;
                int shelves = 0;
                foreach (Transform child in cellar)
                {
                    if (child.name == "Shelf")
                    {
                        shelves++;
                    }
                }
                Assert.AreEqual(3, shelves);

                float top = ConstantOf("TopShelfY");
                float spacing = ConstantOf("ShelfSpacing");
                Assert.AreEqual(top, bottles[3].transform.position.y, 1e-4f, "The fourth bottle is still on the top shelf");
                Assert.AreEqual(top - spacing, bottles[4].transform.position.y, 1e-4f);
                Assert.AreEqual(top - 2f * spacing, bottles[8].transform.position.y, 1e-4f, "The ninth bottle is on the third shelf");
                Assert.AreEqual(bottles[0].transform.position.x, bottles[8].transform.position.x, 1e-4f, "It starts the shelf on the left");
            }
            finally
            {
                foreach (GrapeData grape in grapes)
                {
                    Object.DestroyImmediate(grape);
                }
            }
        }

        [Test]
        public void Regression_GuestsGrapesByName_FindIgnoresTheShelfOrder()
        {
            GrapeData[] grapes = WineBarAssets.Grapes();
            System.Array.Reverse(grapes);

            var found = (GrapeData)Reflect.CallStatic(typeof(WineBarAssets), "Find", grapes, "Syrah");

            Assert.AreEqual("Syrah", found.displayName);
        }

        [Test]
        public void FindingAGrapeThatDoesNotExist_SaysSo()
        {
            LogAssert.Expect(LogType.Error, new Regex("There is no grape called Zinfandel"));

            Assert.IsNull(Reflect.CallStatic(typeof(WineBarAssets), "Find", WineBarAssets.Grapes(), "Zinfandel"));
        }

        [Test]
        public void SetField_WithAWrongName_SaysSo()
        {
            GameObject thing = new GameObject("Thing");
            try
            {
                LogAssert.Expect(LogType.Error, new Regex("Patience has no field called nothing"));

                SetupTools.SetField(thing.AddComponent<Patience>(), "nothing", null);
            }
            finally
            {
                Object.DestroyImmediate(thing);
            }
        }

        // Builds by itself, so it doesn't depend on the order of the tests.
        [Test]
        public void PuttingTheFilesBack_AfterBuilding_LeavesTheProjectUnchanged()
        {
            ProjectBuilder.BuildScene();

            PutTheFilesBack();

            Dictionary<string, byte[]> after = Snapshot();
            CollectionAssert.AreEquivalent(before.Keys, after.Keys, "No file was added or removed");
            foreach (KeyValuePair<string, byte[]> file in before)
            {
                Assert.IsTrue(Same(file.Value, after[file.Key]), $"{file.Key} changed");
            }
        }

        private static float ConstantOf(string name)
        {
            return (float)typeof(WineBarSceneBuilder)
                .GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .GetValue(null);
        }

        private static Dictionary<string, int> CountComponents(Scene scene)
        {
            var counts = new Dictionary<string, int>();
            foreach (GameObject thing in SceneChecks.AllObjects(scene))
            {
                foreach (Component component in thing.GetComponents<Component>())
                {
                    string key = component.GetType().Name;
                    counts.TryGetValue(key, out int count);
                    counts[key] = count + 1;
                }
            }
            return counts;
        }

        private static Dictionary<string, byte[]> Snapshot()
        {
            var files = new Dictionary<string, byte[]>();
            foreach (string folder in WatchedFolders)
            {
                foreach (string path in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                {
                    files[path.Replace('\\', '/')] = File.ReadAllBytes(path);
                }
            }
            return files;
        }

        private static bool Same(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
