using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>The mouse and the feel of the bar: clicking the world, the buttons' feel, and the sound effects.</summary>
    public class InputAndFeelTests : BarSceneTestBase
    {
        private Mouse mouse;
        private InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
        private InputSettings.BackgroundBehavior backgroundBehavior;

        [SetUp]
        public void AddAMouse()
        {
            // In batch mode there is no focused Game view, so input must be told to reach the game anyway.
            editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            backgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse = InputSystem.AddDevice<Mouse>();
            mouse.MakeCurrent();
        }

        [TearDown]
        public void RemoveTheMouse()
        {
            InputSystem.RemoveDevice(mouse);
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
            InputSystem.settings.backgroundBehavior = backgroundBehavior;
        }

        // ---------- WorldClicker ----------

        [UnityTest]
        public IEnumerator WorldClicker_ClickOnABottle_Pours()
        {
            yield return null;
            GrapeBottle merlot = Bottle("Merlot");
            Vector2 clickArea = (Vector2)merlot.transform.position + merlot.GetComponent<BoxCollider2D>().offset;

            yield return Click(clickArea);

            Assert.AreEqual(1, DropsInTheAir());
        }

        [UnityTest]
        public IEnumerator WorldClicker_ClickOnTheWall_DoesNothing()
        {
            yield return null;

            yield return Click(new Vector2(-8.5f, 4.5f));

            Assert.AreEqual(0, DropsInTheAir());
        }

        [UnityTest]
        public IEnumerator WorldClicker_ClickOnAUIButton_DoesNotReachTheBottleBehindIt()
        {
            yield return null;
            Vector2 buttonOnScreen = ObjectNamed("Taste Button").transform.position;
            Vector2 behindTheButton = WorldCamera().ScreenToWorldPoint(buttonOnScreen);
            GrapeBottle merlot = Bottle("Merlot");
            merlot.transform.position = behindTheButton - merlot.GetComponent<BoxCollider2D>().offset;
            yield return new WaitForFixedUpdate(); // The physics sees the bottle where it is now.

            yield return Click(behindTheButton);

            Assert.AreEqual(0, DropsInTheAir());
        }

        [UnityTest]
        public IEnumerator WorldClicker_HoldingTheButton_PoursOnlyOnce()
        {
            yield return null;
            GrapeBottle merlot = Bottle("Merlot");
            Vector2 clickArea = (Vector2)merlot.transform.position + merlot.GetComponent<BoxCollider2D>().offset;
            Vector2 screen = WorldCamera().WorldToScreenPoint(clickArea);

            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(1, DropsInTheAir());
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            yield return null;
        }

        // ---------- ButtonFeel ----------

        [UnityTest]
        public IEnumerator ButtonFeel_GrowsUnderTheMouseAndShrinksWhenPressed()
        {
            ButtonFeel feel = ObjectNamed("Taste Button").GetComponent<ButtonFeel>();

            feel.OnPointerEnter(null);
            yield return WaitUntil(() => feel.transform.localScale.x > 1.03f, "the button to grow");

            feel.OnPointerDown(null);
            yield return WaitUntil(() => feel.transform.localScale.x < 0.98f, "the button to shrink");

            feel.OnPointerUp(null);
            yield return WaitUntil(() => feel.transform.localScale.x > 1.03f, "the button to grow back");

            feel.OnPointerExit(null);
            yield return WaitUntil(() => Mathf.Abs(feel.transform.localScale.x - 1f) < 0.005f, "the button to go back to its size");
        }

        // Regression: a button hidden while it was big (like Serve under the mouse) came back still big.
        [UnityTest]
        public IEnumerator Regression_ButtonFeelReset_AHiddenButtonComesBackAtItsNormalSize()
        {
            ButtonFeel feel = ObjectNamed("Serve Button").GetComponent<ButtonFeel>();
            feel.OnPointerEnter(null);
            yield return WaitUntil(() => feel.transform.localScale.x > 1.03f, "the button to grow");

            feel.gameObject.SetActive(false);
            Assert.AreEqual(Vector3.one, feel.transform.localScale);

            feel.gameObject.SetActive(true);
            yield return null;
            yield return null;
            Assert.AreEqual(1f, feel.transform.localScale.x, 1e-4f, "It doesn't grow again by itself");
        }

        [UnityTest]
        public IEnumerator ButtonFeel_PressingPlaysTheClick()
        {
            yield return null;

            ObjectNamed("Taste Button").GetComponent<ButtonFeel>().OnPointerDown(null);

            Assert.IsNotNull(GameObject.Find("Sound Effects"));
            Assert.IsNotNull(SfxClips()["Click"]);
        }

        // ---------- Sfx ----------

        [UnityTest]
        public IEnumerator Sfx_LoadsEachSoundOnceAndKeepsOnePlayer()
        {
            yield return null;

            Sfx.Play("Click");
            AudioClip first = SfxClips()["Click"];
            Sfx.Play("Click", 0.5f);

            Assert.AreSame(first, SfxClips()["Click"]);
            Assert.AreEqual(1, CountObjectsNamed("Sound Effects"));
        }

        [UnityTest]
        public IEnumerator Sfx_ASoundThatDoesNotExist_IsSilentAndRemembered()
        {
            yield return null;

            Sfx.Play("NoSuchSound");

            Assert.IsTrue(SfxClips().ContainsKey("NoSuchSound"));
            Assert.IsNull(SfxClips()["NoSuchSound"]);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Sfx_ThePlayerSurvivesLoadingTheScene()
        {
            Sfx.Play("Click");
            GameObject player = GameObject.Find("Sound Effects");

            yield return LoadScene();

            Assert.IsTrue(player != null);
        }

        // ---------- Helpers ----------

        private IEnumerator Click(Vector2 worldPoint)
        {
            Vector2 screen = WorldCamera().WorldToScreenPoint(worldPoint);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            yield return null;
        }

        private static Camera WorldCamera()
        {
            return PlayReflect.Get<Camera>(Object.FindAnyObjectByType<WorldClicker>(), "worldCamera");
        }

        private static Dictionary<string, AudioClip> SfxClips()
        {
            return PlayReflect.GetStatic<Dictionary<string, AudioClip>>(typeof(Sfx), "clips");
        }

        private static int CountObjectsNamed(string name)
        {
            int count = 0;
            foreach (Transform thing in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (thing.name == name)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
