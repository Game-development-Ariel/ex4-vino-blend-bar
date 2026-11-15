using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Build Scene Only: creates the bar scene from code: the shelves with the eight bottles,
/// the glass on the counter, the game logic and the UI. Positions are in world units.
/// Running it again rebuilds the scene from scratch.
/// </summary>
public static class WineBarSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/WineBar.unity";
    private const string DropPrefabPath = "Assets/Prefabs/WineDrop.prefab";

    private const int BottlesPerShelf = 4;
    private const float FirstBottleX = -7.5f;
    private const float BottleSpacing = 1.8f;
    private static readonly float[] ShelfHeights = { 2f, -1.6f };

    private static readonly Vector2 GlassBottom = new Vector2(0.3f, -2.6f);
    private const float GlassWidth = 1.4f;
    private const float GlassHeight = 2.2f;

    private static readonly Color Wall = new Color(0.17f, 0.1f, 0.08f);
    private static readonly Color Wood = new Color(0.4f, 0.24f, 0.14f);
    private static readonly Color GlassColor = new Color(0.85f, 0.9f, 0.95f, 0.5f);
    private static readonly Color LabelColor = new Color(0.98f, 0.93f, 0.84f);
    private static readonly Color HintColor = new Color(0.75f, 0.68f, 0.58f);

    [MenuItem("Tools/Build Scene Only")]
    public static void Build()
    {
        Scene scene = SceneParts.NewScene();
        Camera camera = SceneParts.CreateCamera(Wall, 5.4f);
        SceneParts.CreateGlobalLight();
        SetupTools.SetField(camera.gameObject.AddComponent<WorldClicker>(), "worldCamera", camera);

        GrapeData[] grapes = WineBarAssets.Grapes();
        BarEveningSettings settings = WineBarAssets.EveningSettings(grapes);

        GrapeBottle[] bottles = CreateCellar(grapes);
        CreateCounter();
        Glass glass = CreateGlass(out Transform pourTarget);

        var game = new GameObject("Game");
        Patience patience = game.AddComponent<Patience>();
        Bartender bartender = game.AddComponent<Bartender>();
        SetupTools.SetArray(bartender, "bottles", bottles);
        SetupTools.SetField(bartender, "glass", glass);
        SetupTools.SetField(bartender, "dropPrefab", CreateDropPrefab());
        SetupTools.SetField(bartender, "pourTarget", pourTarget);

        BarEvening evening = game.AddComponent<BarEvening>();
        SetupTools.SetField(evening, "settings", settings);
        SetupTools.SetField(evening, "glass", glass);
        SetupTools.SetField(evening, "patience", patience);
        SetupTools.SetField(evening, "bartender", bartender);

        CreateUi(settings, evening, patience, glass);
        SceneParts.Save(scene, ScenePath);
    }

    // ---------- World ----------

    private static GrapeBottle[] CreateCellar(GrapeData[] grapes)
    {
        var cellar = new GameObject("Cellar").transform;
        var bottles = new GrapeBottle[grapes.Length];
        for (int i = 0; i < grapes.Length; i++)
        {
            float x = FirstBottleX + (i % BottlesPerShelf) * BottleSpacing;
            float y = ShelfHeights[i / BottlesPerShelf];
            bottles[i] = CreateBottle(cellar, grapes[i], new Vector2(x, y));
        }

        foreach (float y in ShelfHeights)
        {
            float width = BottlesPerShelf * BottleSpacing;
            float middle = FirstBottleX + (BottlesPerShelf - 1) * BottleSpacing / 2f;
            SceneParts.CreateBox("Shelf", cellar, new Vector2(middle, y - 0.08f), new Vector2(width, 0.16f), Wood, 1);
        }
        return bottles;
    }

    /// <summary>A bottle standing on the shelf, with its name and its two hint lines under the shelf.</summary>
    private static GrapeBottle CreateBottle(Transform parent, GrapeData grape, Vector2 basePosition)
    {
        var bottle = new GameObject(grape.displayName);
        bottle.transform.SetParent(parent, false);
        bottle.transform.position = basePosition;

        // Dark glass with a hint of the wine's color.
        Color glassTint = Color.Lerp(grape.liquidColor, Color.black, grape.isRed ? 0.35f : 0.15f);
        SceneParts.CreateBox("Body", bottle.transform, new Vector2(0f, 0.65f), new Vector2(0.55f, 1.3f), glassTint, 3);
        SceneParts.CreateBox("Neck", bottle.transform, new Vector2(0f, 1.5f), new Vector2(0.2f, 0.4f), glassTint, 3);
        SceneParts.CreateBox("Paper", bottle.transform, new Vector2(0f, 0.6f), new Vector2(0.55f, 0.45f), LabelColor, 4);

        var spout = new GameObject("Spout");
        spout.transform.SetParent(bottle.transform, false);
        spout.transform.localPosition = new Vector3(0f, 1.8f, 0f);

        SceneParts.CreateLabel(grape.displayName, bottle.transform, new Vector2(0f, -0.42f), 0.24f, LabelColor);
        SceneParts.CreateLabel(grape.hint, bottle.transform, new Vector2(0f, -0.95f), 0.19f, HintColor);

        BoxCollider2D clickArea = bottle.AddComponent<BoxCollider2D>();
        clickArea.isTrigger = true;
        clickArea.offset = new Vector2(0f, 0.85f);
        clickArea.size = new Vector2(0.9f, 1.8f);

        GrapeBottle grapeBottle = bottle.AddComponent<GrapeBottle>();
        SetupTools.SetField(grapeBottle, "grape", grape);
        SetupTools.SetField(grapeBottle, "spout", spout.transform);
        return grapeBottle;
    }

    /// <summary>The counter the glass stands on. It is only a picture, at the root of the scene.</summary>
    private static void CreateCounter()
    {
        SceneParts.CreateBox("Counter", null, new Vector2(4.5f, GlassBottom.y - 0.2f), new Vector2(11f, 0.4f), Wood, 1);
    }

    private static Glass CreateGlass(out Transform pourTarget)
    {
        var glassObject = new GameObject("Glass");
        glassObject.transform.position = GlassBottom;
        Transform parent = glassObject.transform;

        SceneParts.CreateBox("Left Side", parent, new Vector2(-GlassWidth / 2f, GlassHeight / 2f), new Vector2(0.06f, GlassHeight), GlassColor, 4);
        SceneParts.CreateBox("Right Side", parent, new Vector2(GlassWidth / 2f, GlassHeight / 2f), new Vector2(0.06f, GlassHeight), GlassColor, 4);
        SceneParts.CreateBox("Base", parent, new Vector2(0f, 0.03f), new Vector2(GlassWidth, 0.06f), GlassColor, 4);

        // The trigger covers the mouth of the glass and a little above it.
        // A splash that enters it joins the blend, so a fast splash from a far bottle can't fly over it.
        BoxCollider2D mouth = glassObject.AddComponent<BoxCollider2D>();
        mouth.isTrigger = true;
        mouth.offset = new Vector2(0f, GlassHeight - 0.2f);
        mouth.size = new Vector2(GlassWidth - 0.1f, 1.2f);
        Glass glass = glassObject.AddComponent<Glass>();

        GameObject liquid = SceneParts.CreateBox("Liquid", parent, new Vector2(0f, GlassHeight / 2f), new Vector2(GlassWidth - 0.06f, GlassHeight - 0.06f), Color.clear, 3);
        SetupTools.SetField(liquid.AddComponent<GlassFill>(), "glass", glass);

        pourTarget = new GameObject("Pour Target").transform;
        pourTarget.SetParent(parent, false);
        pourTarget.localPosition = new Vector3(0f, GlassHeight, 0f);
        return glass;
    }

    /// <summary>
    /// A small round splash. Its collider is a trigger, so two splashes in the air pass through each other
    /// instead of bumping one another out of the glass.
    /// </summary>
    private static WineDrop CreateDropPrefab()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DropPrefabPath));
        var drop = new GameObject("WineDrop");
        drop.transform.localScale = Vector3.one * 0.3f;
        SpriteRenderer spriteRenderer = drop.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SceneParts.Circle;
        spriteRenderer.sortingOrder = 6;
        drop.AddComponent<Rigidbody2D>();
        drop.AddComponent<CircleCollider2D>().isTrigger = true;
        drop.AddComponent<WineDrop>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(drop, DropPrefabPath);
        Object.DestroyImmediate(drop);
        return prefab.GetComponent<WineDrop>();
    }

    // ---------- UI ----------

    private static void CreateUi(BarEveningSettings settings, BarEvening evening, Patience patience, Glass glass)
    {
        var ui = new GameObject("UI");

        BarHud hud = ui.AddComponent<BarHud>();
        SetupTools.SetField(hud, "settings", settings);
        SetupTools.SetField(hud, "evening", evening);

        GuestPanel guest = ui.AddComponent<GuestPanel>();
        SetupTools.SetField(guest, "evening", evening);
        SetupTools.SetField(guest, "patience", patience);

        BlendPanel blend = ui.AddComponent<BlendPanel>();
        SetupTools.SetField(blend, "evening", evening);
        SetupTools.SetField(blend, "glass", glass);

        EveningEndPanel end = ui.AddComponent<EveningEndPanel>();
        SetupTools.SetField(end, "settings", settings);
        SetupTools.SetField(end, "evening", evening);
    }
}
