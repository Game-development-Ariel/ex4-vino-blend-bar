using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Building blocks for the scene builder of the bar: a camera, a light,
/// colored boxes and world labels. The bar uses plain shapes on purpose, there is no final art yet.
/// </summary>
public static class SceneParts
{
    private const string SquarePath = "Assets/Sprites/Shapes/Square.png";
    private const string CirclePath = "Assets/Sprites/Shapes/Circle.png";
    private const string FontPath = "Assets/Resources/Fonts/CrimsonText.ttf";

    // The camera stands in front of the 2D scene, which is at z = 0.
    private static readonly Vector3 CameraPosition = new Vector3(0f, 0f, -10f);

    // World labels: drawn with a big font so the letters are sharp, then scaled down.
    // World labels use a big font size, so the letters stay sharp.
    // A TextMesh draws 10 font pixels per world unit, so at character size 1 the letters are
    // LabelFontSize / 10 world units tall. characterSize = height / LabelHeightPerCharacterSize gives letters "height" units tall.
    private const int LabelFontSize = 64;
    private const float TextMeshPixelsPerUnit = 10f;
    private const float LabelHeightPerCharacterSize = LabelFontSize / TextMeshPixelsPerUnit;

    // In front of every box, so the labels are never hidden.
    private const int LabelSortingOrder = 20;

    /// <summary>A white square, one world unit wide. Tinting and scaling it gives any colored box.</summary>
    public static Sprite Square => LoadShape(SquarePath, (x, y) => Color.white);

    public static Sprite Circle => LoadShape(CirclePath, (x, y) =>
    {
        float half = PlaceholderSprites.PixelsPerTile / 2f;
        // x + 0.5 is the middle of pixel x.
        bool inside = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude <= half;
        return inside ? Color.white : Color.clear;
    });

    // The file is drawn and imported only the first time. After that it is just loaded.
    private static Sprite LoadShape(string path, System.Func<int, int, Color> pixelColor)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return sprite != null ? sprite : PlaceholderSprites.LoadOrCreate(path, pixelColor);
    }

    public static Scene NewScene()
    {
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    public static Camera CreateCamera(Color background, float orthographicSize)
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = CameraPosition;
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = orthographicSize;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        return camera;
    }

    /// <summary>The project uses the 2D renderer, where sprites are black without a light.</summary>
    public static void CreateGlobalLight()
    {
        Light2D light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
    }

    /// <summary>A colored rectangle. The center and size are in world units.</summary>
    public static GameObject CreateBox(string name, Transform parent, Vector2 center, Vector2 size, Color color, int sortingOrder)
    {
        var box = new GameObject(name);
        box.transform.SetParent(parent, false);
        box.transform.localPosition = center;
        box.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer spriteRenderer = box.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Square;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        return box;
    }

    /// <summary>A text that lives in the world (not on the UI), like the grape names under the bottles.</summary>
    public static TextMesh CreateLabel(string text, Transform parent, Vector2 position, float height, Color color)
    {
        var label = new GameObject("Label " + text);
        label.transform.SetParent(parent, false);
        label.transform.localPosition = position;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.font = font;
        textMesh.fontSize = LabelFontSize;
        textMesh.characterSize = height / LabelHeightPerCharacterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = font.material;
        meshRenderer.sortingOrder = LabelSortingOrder;
        return textMesh;
    }

    public static void Save(Scene scene, string path)
    {
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Built and saved " + path);
    }
}
