using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Sets up the same conditions for every PlayMode test, on any computer:
/// 1. The sound is muted, so the music and clicks of the real scene don't come out of the speakers.
///    The tests still check that sounds are played, not what is heard.
/// 2. Every frame advances the game clock by exactly one sixtieth of a second, however long the frame really took.
///    So "wait 2 seconds" is always the same 120 frames, and a slow machine (like the GitHub server)
///    can't make a timing test fail.
/// It has no namespace, so NUnit runs it once around every test in this assembly.
/// </summary>
[SetUpFixture]
public class PlayModeTestEnvironment
{
    // The fixed length of one frame during the tests, in seconds.
    private const float FrameSeconds = 1f / 60f;

    private float volumeBefore;
    private float frameSecondsBefore;

    [OneTimeSetUp]
    public void SetUp()
    {
        volumeBefore = AudioListener.volume;
        AudioListener.volume = 0f;

        frameSecondsBefore = Time.captureDeltaTime;
        Time.captureDeltaTime = FrameSeconds;
    }

    [OneTimeTearDown]
    public void Restore()
    {
        AudioListener.volume = volumeBefore;
        Time.captureDeltaTime = frameSecondsBefore;
    }
}
