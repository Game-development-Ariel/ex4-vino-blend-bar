using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>
    /// A base for tests that play the real WineBar scene. Before each test the scene is loaded fresh,
    /// and the first guest is at the counter. Any error or exception in the log fails the test.
    /// </summary>
    public abstract class BarSceneTestBase
    {
        public const string ScenePath = "Assets/Scenes/WineBar.unity";

        // Long enough for any splash to land (it flies 0.7 seconds), short enough to fail fast.
        protected const float WaitLimitSeconds = 5f;

        protected BarEvening Evening;
        protected Glass Glass;
        protected Bartender Bartender;
        protected Patience Patience;
        protected BarEveningSettings Settings;
        protected GrapeBottle[] Bottles;

        // What the evening said during the test.
        protected readonly List<ServeResult> Served = new List<ServeResult>();
        protected readonly List<WineProfile> Tasted = new List<WineProfile>();
        protected readonly List<(float stars, bool passed)> Ended = new List<(float, bool)>();
        protected int GaveUp;
        protected int Arrived;

        [UnitySetUp]
        public IEnumerator LoadTheBar()
        {
            Time.timeScale = 1f;
            yield return LoadScene();
        }

        [UnityTearDown]
        public IEnumerator ResetTime()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        protected IEnumerator LoadScene()
        {
#if UNITY_EDITOR
            AsyncOperation loading = EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!loading.isDone)
            {
                yield return null;
            }
#else
            Assert.Ignore("These tests load the scene from the editor.");
#endif
            yield return null; // Start runs, and the first guest arrives.

            Evening = UnityEngine.Object.FindAnyObjectByType<BarEvening>();
            Assert.IsNotNull(Evening, "The scene has a BarEvening");
            Glass = UnityEngine.Object.FindAnyObjectByType<Glass>();
            Bartender = UnityEngine.Object.FindAnyObjectByType<Bartender>();
            Patience = UnityEngine.Object.FindAnyObjectByType<Patience>();
            Settings = PlayReflect.Get<BarEveningSettings>(Evening, "settings");
            Bottles = PlayReflect.Get<GrapeBottle[]>(Bartender, "bottles");
            Listen();
        }

        private void Listen()
        {
            Served.Clear();
            Tasted.Clear();
            Ended.Clear();
            GaveUp = 0;
            Arrived = 0;
            Evening.GuestServed += result => Served.Add(result);
            Evening.Tasted += wine => Tasted.Add(wine);
            Evening.EveningEnded += (stars, passed) => Ended.Add((stars, passed));
            Evening.GuestGaveUp += () => GaveUp++;
            Evening.GuestArrived += order => Arrived++;
        }

        protected GrapeBottle Bottle(string displayName)
        {
            GrapeBottle bottle = Array.Find(Bottles, b => b.Grape.displayName == displayName);
            Assert.IsNotNull(bottle, $"There is a bottle of {displayName}");
            return bottle;
        }

        protected GrapeData Grape(string displayName)
        {
            return Bottle(displayName).Grape;
        }

        protected static int DropsInTheAir()
        {
            return UnityEngine.Object.FindObjectsByType<WineDrop>(FindObjectsSortMode.None).Length;
        }

        /// <summary>
        /// Lands a splash in the glass right now, as if it had flown in: the glass's trigger is called with it.
        /// For tests that need a glass with something in it, and not the flight.
        /// </summary>
        protected void DropIntoGlass(string displayName, int percent)
        {
            WineDrop prefab = PlayReflect.Get<WineDrop>(Bartender, "dropPrefab");
            WineDrop drop = UnityEngine.Object.Instantiate(prefab, new Vector3(500f, 500f, 0f), Quaternion.identity);
            drop.Setup(Grape(displayName), percent);
            PlayReflect.Call(Glass, "OnTriggerEnter2D", drop.GetComponent<Collider2D>());
        }

        /// <summary>Fills the glass from pairs of grape name and percent: FillGlass("Syrah", 70, "Grenache", 30).</summary>
        protected void FillGlass(params object[] namesAndPercents)
        {
            for (int i = 0; i < namesAndPercents.Length; i += 2)
            {
                DropIntoGlass((string)namesAndPercents[i], (int)namesAndPercents[i + 1]);
            }
        }

        /// <summary>Waits until the condition is true, and fails the test if it takes longer than the limit (in real seconds).</summary>
        protected static IEnumerator WaitUntil(Func<bool> condition, string what, float limitSeconds = WaitLimitSeconds)
        {
            float giveUpAt = Time.realtimeSinceStartup + limitSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > giveUpAt)
                {
                    Assert.Fail($"Waited {limitSeconds} seconds for: {what}");
                }
                yield return null;
            }
        }

        protected static IEnumerator WaitForDropsToLand()
        {
            yield return WaitUntil(() => DropsInTheAir() == 0, "every splash to land");
        }

        protected static Text TextNamed(string name)
        {
            foreach (Text text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text.name == name)
                {
                    return text;
                }
            }
            Assert.Fail($"There is no text called {name}");
            return null;
        }

        protected static GameObject ObjectNamed(string name)
        {
            foreach (Transform thing in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (thing.name == name)
                {
                    return thing.gameObject;
                }
            }
            Assert.Fail($"There is no object called {name}");
            return null;
        }

        protected static Button ButtonNamed(string label)
        {
            return ObjectNamed(label + " Button").GetComponent<Button>();
        }

        /// <summary>Serves the exact recipe of every guest, one after the other, and ends the evening.</summary>
        protected void PlayThePerfectEvening()
        {
            FillGlass("Pinot Noir", 100);
            Evening.Serve();
            Evening.NextGuest();
            FillGlass("Riesling", 100);
            Evening.Serve();
            Evening.NextGuest();
            FillGlass("Syrah", 70, "Grenache", 30);
            Evening.Serve();
            Evening.NextGuest();
        }
    }
}
