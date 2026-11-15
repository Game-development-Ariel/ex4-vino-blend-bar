using UnityEditor;
using UnityEngine;

/// <summary>
/// Rebuilds the scene of the game from the setup tools, and builds the game for the browser.
/// The methods are public so they can also run from the command line (Unity -batchmode -executeMethod).
/// </summary>
public static class ProjectBuilder
{
    private const string ScenePath = "Assets/Scenes/WineBar.unity";

    [MenuItem("Tools/Rebuild Scene")]
    public static void BuildScene()
    {
        WineBarSceneBuilder.Build();
        // The game has one scene, and it is where the game starts.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    /// <summary>Builds the game for the browser into Build/WebGL, ready to zip and upload to itch.io.</summary>
    [MenuItem("Tools/Build WebGL")]
    public static void BuildWebGL()
    {
        // Gzip with the fallback loader works on itch.io, which doesn't send the compression headers.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Build/WebGL",
            target = BuildTarget.WebGL,
        };
        Debug.Log("WebGL build: " + BuildPipeline.BuildPlayer(options).summary.result);
    }
}
