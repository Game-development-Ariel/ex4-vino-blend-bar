using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Silences the game while the PlayMode tests run, so the music, steps and clicks of the real scene
/// don't come out of the speakers. The tests still check that sounds are played, not what is heard.
/// It has no namespace, so NUnit runs it once around every test in this assembly.
/// </summary>
[SetUpFixture]
public class MuteAudioDuringTests
{
    private float volumeBefore;

    [OneTimeSetUp]
    public void Mute()
    {
        volumeBefore = AudioListener.volume;
        AudioListener.volume = 0f;
    }

    [OneTimeTearDown]
    public void Restore()
    {
        AudioListener.volume = volumeBefore;
    }
}
