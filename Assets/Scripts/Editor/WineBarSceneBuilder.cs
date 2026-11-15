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
    private const float TopShelfY = 2f;
    private const float ShelfSpacing = 3.6f;

    private static readonly Vector2 GlassBottom = new Vector2(0.3f, -2.6f);
    private const float GlassWidth = 1.4f;
    private const float GlassHeight = 2.2f;

    private static readonly Color Wall = new Color(0.17f, 0.1f, 0.08f);
    private static readonly Color Wood = new Color(0.4f, 0.24f, 0.14f);
    private static readonly Color GlassColor = new Color(0.85f, 0.9f, 0.95f, 0.5f);
    private static readonly Color LabelColor = new Color(0.98f, 0.93f, 0.84f);
    private static readonly Color HintColor = new Color(0.75f, 0.68f, 0.58f);

    // Half the height of the world the camera shows, in world units.
    private const float CameraHalfHeight = 5.4f;

    // Sorting orders: a higher number is drawn in front.
    private const int FurnitureOrder = 1;
    private const int BottleOrder = 3;
    private const int BottlePaperOrder = 4;
    private const int LiquidOrder = 3;
    private const int GlassOrder = 4;
    private const int DropOrder = 6;

    // How thick the plank of a shelf is, in world units. It hangs just under the bottles.
    private const float ShelfThickness = 0.16f;

    // How much darker the bottle glass is than its wine: 0 = the wine's color, 1 = black.
    private const float RedBottleDarkening = 0.35f;
    private const float WhiteBottleDarkening = 0.15f;

    // The parts of a bottle, in world units, from the bottom of the bottle (where it stands on the shelf).
    private static readonly Vector2 BottleBodyCenter = new Vector2(0f, 0.65f);
    private static readonly Vector2 BottleBodySize = new Vector2(0.55f, 1.3f);
    private static readonly Vector2 NeckCenter = new Vector2(0f, 1.5f);
    private static readonly Vector2 NeckSize = new Vector2(0.2f, 0.4f);
    private static readonly Vector2 PaperCenter = new Vector2(0f, 0.6f);
    private static readonly Vector2 PaperSize = new Vector2(0.55f, 0.45f);
    private static readonly Vector3 SpoutPosition = new Vector3(0f, 1.8f, 0f);

    // The grape's name and its hint under the shelf: the middle of each label (from the bottom of the bottle)
    // and how tall its letters are, in world units.
    private static readonly Vector2 NameLabelPosition = new Vector2(0f, -0.42f);
    private const float NameLabelHeight = 0.24f;
    private static readonly Vector2 HintLabelPosition = new Vector2(0f, -0.95f);
    private const float HintLabelHeight = 0.19f;

    // The area that catches a click on a bottle, a little bigger than the bottle. From the bottom of the bottle, in world units.
    private static readonly Vector2 BottleClickCenter = new Vector2(0f, 0.85f);
    private static readonly Vector2 BottleClickSize = new Vector2(0.9f, 1.8f);

    // The counter, in world units: the x of its middle, its width, and its thickness. Its top is at the bottom of the glass.
    private const float CounterCenterX = 4.5f;
    private const float CounterWidth = 11f;
    private const float CounterThickness = 0.4f;

    // How thick the sides and the base of the glass are, in world units.
    // The liquid is this much narrower and shorter than the glass, so it fits between the sides.
    private const float GlassWallThickness = 0.06f;

    // The trigger at the mouth of the glass, in world units: how far its middle is under the rim,
    // how much narrower it is than the glass (in total, not per side), and how tall it is.
    private const float MouthCenterBelowRim = 0.2f;
    private const float MouthInset = 0.1f;
    private const float MouthHeight = 1.2f;

    // How wide a splash is, in world units.
    private const float DropSize = 0.3f;

    [MenuItem("Tools/Build Scene Only")]
    public static void Build()
    {
        Scene scene = SceneParts.NewScene();
        Camera camera = SceneParts.CreateCamera(Wall, CameraHalfHeight);
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
            bottles[i] = CreateBottle(cellar, grapes[i], new Vector2(x, ShelfY(i / BottlesPerShelf)));
        }

        // As many shelves as the grapes need, so a ninth grape starts a third shelf.
        int shelves = (grapes.Length + BottlesPerShelf - 1) / BottlesPerShelf;
        for (int shelf = 0; shelf < shelves; shelf++)
        {
            float y = ShelfY(shelf);
            float width = BottlesPerShelf * BottleSpacing;
            float middle = FirstBottleX + (BottlesPerShelf - 1) * BottleSpacing / 2f;
            SceneParts.CreateBox("Shelf", cellar, new Vector2(middle, y - ShelfThickness / 2f), new Vector2(width, ShelfThickness), Wood, FurnitureOrder);
        }
        return bottles;
    }

    private static float ShelfY(int shelf)
    {
        return TopShelfY - shelf * ShelfSpacing;
    }

    /// <summary>A bottle standing on the shelf, with its name and its two hint lines under the shelf.</summary>
    private static GrapeBottle CreateBottle(Transform parent, GrapeData grape, Vector2 basePosition)
    {
        var bottle = new GameObject(grape.displayName);
        bottle.transform.SetParent(parent, false);
        bottle.transform.position = basePosition;

        // Dark glass with a hint of the wine's color.
        Color glassTint = Color.Lerp(grape.liquidColor, Color.black, grape.isRed ? RedBottleDarkening : WhiteBottleDarkening);
        SceneParts.CreateBox("Body", bottle.transform, BottleBodyCenter, BottleBodySize, glassTint, BottleOrder);
        SceneParts.CreateBox("Neck", bottle.transform, NeckCenter, NeckSize, glassTint, BottleOrder);
        SceneParts.CreateBox("Paper", bottle.transform, PaperCenter, PaperSize, LabelColor, BottlePaperOrder);

        var spout = new GameObject("Spout");
        spout.transform.SetParent(bottle.transform, false);
        spout.transform.localPosition = SpoutPosition;

        SceneParts.CreateLabel(grape.displayName, bottle.transform, NameLabelPosition, NameLabelHeight, LabelColor);
        SceneParts.CreateLabel(grape.hint, bottle.transform, HintLabelPosition, HintLabelHeight, HintColor);

        BoxCollider2D clickArea = bottle.AddComponent<BoxCollider2D>();
        clickArea.isTrigger = true;
        clickArea.offset = BottleClickCenter;
        clickArea.size = BottleClickSize;

        GrapeBottle grapeBottle = bottle.AddComponent<GrapeBottle>();
        SetupTools.SetField(grapeBottle, "grape", grape);
        SetupTools.SetField(grapeBottle, "spout", spout.transform);
        return grapeBottle;
    }

    /// <summary>The counter the glass stands on. It is only a picture, at the root of the scene.</summary>
    private static void CreateCounter()
    {
        SceneParts.CreateBox("Counter", null, new Vector2(CounterCenterX, GlassBottom.y - CounterThickness / 2f), new Vector2(CounterWidth, CounterThickness), Wood, FurnitureOrder);
    }

    private static Glass CreateGlass(out Transform pourTarget)
    {
        var glassObject = new GameObject("Glass");
        glassObject.transform.position = GlassBottom;
        Transform parent = glassObject.transform;

        SceneParts.CreateBox("Left Side", parent, new Vector2(-GlassWidth / 2f, GlassHeight / 2f), new Vector2(GlassWallThickness, GlassHeight), GlassColor, GlassOrder);
        SceneParts.CreateBox("Right Side", parent, new Vector2(GlassWidth / 2f, GlassHeight / 2f), new Vector2(GlassWallThickness, GlassHeight), GlassColor, GlassOrder);
        SceneParts.CreateBox("Base", parent, new Vector2(0f, GlassWallThickness / 2f), new Vector2(GlassWidth, GlassWallThickness), GlassColor, GlassOrder);

        // The trigger covers the mouth of the glass and a little above it.
        // A splash that enters it joins the blend, so a fast splash from a far bottle can't fly over it.
        BoxCollider2D mouth = glassObject.AddComponent<BoxCollider2D>();
        mouth.isTrigger = true;
        mouth.offset = new Vector2(0f, GlassHeight - MouthCenterBelowRim);
        mouth.size = new Vector2(GlassWidth - MouthInset, MouthHeight);
        Glass glass = glassObject.AddComponent<Glass>();

        GameObject liquid = SceneParts.CreateBox("Liquid", parent, new Vector2(0f, GlassHeight / 2f), new Vector2(GlassWidth - GlassWallThickness, GlassHeight - GlassWallThickness), Color.clear, LiquidOrder);
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
        drop.transform.localScale = Vector3.one * DropSize;
        SpriteRenderer spriteRenderer = drop.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SceneParts.Circle;
        spriteRenderer.sortingOrder = DropOrder;
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

        GlassReadout readout = ui.AddComponent<GlassReadout>();
        SetupTools.SetField(readout, "glass", glass);

        NotesBoard notes = ui.AddComponent<NotesBoard>();
        SetupTools.SetField(notes, "settings", settings);
        SetupTools.SetField(notes, "evening", evening);

        OrderButtons buttons = ui.AddComponent<OrderButtons>();
        SetupTools.SetField(buttons, "evening", evening);
        SetupTools.SetField(buttons, "glass", glass);

        EveningEndPanel end = ui.AddComponent<EveningEndPanel>();
        SetupTools.SetField(end, "settings", settings);
        SetupTools.SetField(end, "evening", evening);
    }
}
