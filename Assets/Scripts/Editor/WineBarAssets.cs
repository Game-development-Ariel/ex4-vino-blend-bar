using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The data assets of the wine bar: the eight grapes and the settings of the evening.
/// Values are written only when an asset is new, so tuning it in the Inspector is not overwritten.
/// </summary>
public static class WineBarAssets
{
    private const string Folder = "Assets/Data/";

    /// <summary>The eight grapes, in shelf order. The values are the ones from our prototype.</summary>
    public static GrapeData[] Grapes()
    {
        return new[]
        {
            Grape("Cabernet Sauvignon", true, new Color(0.36f, 0.04f, 0.12f), 6f, 14f, 9f, 8f, 2f, "Grippy, full\nblackcurrant, cedar", "blackcurrant", "cedar", "mint"),
            Grape("Merlot", true, new Color(0.48f, 0.07f, 0.16f), 5f, 13.5f, 5f, 6f, 3f, "Soft, round\nplum, chocolate", "plum", "chocolate"),
            Grape("Pinot Noir", true, new Color(0.62f, 0.13f, 0.2f), 7f, 13f, 3f, 3f, 2f, "Light, fresh\ncherry, raspberry", "cherry", "raspberry", "mushroom"),
            Grape("Syrah", true, new Color(0.3f, 0.03f, 0.16f), 5f, 14.5f, 7f, 8f, 2f, "Bold, spicy\nblackberry, pepper", "blackberry", "black pepper", "smoke"),
            Grape("Grenache", true, new Color(0.66f, 0.16f, 0.18f), 4f, 15f, 4f, 6f, 3f, "Warm, juicy\nstrawberry, spice", "strawberry", "cinnamon"),
            Grape("Sauvignon Blanc", false, new Color(0.88f, 0.92f, 0.55f), 9f, 12.5f, 1f, 3f, 2f, "Zesty, light\ngrapefruit, grass", "grapefruit", "cut grass"),
            Grape("Chardonnay", false, new Color(0.95f, 0.83f, 0.42f), 5f, 13.5f, 1f, 7f, 2f, "Creamy, full\nbutter, apple", "butter", "apple", "vanilla"),
            Grape("Riesling", false, new Color(0.93f, 0.9f, 0.62f), 9f, 10f, 1f, 3f, 6f, "Racy, a bit sweet\nlime, honey", "lime", "honey", "peach"),
        };
    }

    private static GrapeData Grape(string name, bool isRed, Color liquid, float acidity, float alcohol, float tannin,
        float body, float sweetness, string hint, params string[] notes)
    {
        string path = Folder + "Grapes/" + name.Replace(" ", "") + ".asset";
        bool isNew = !File.Exists(path);
        GrapeData grape = SetupTools.Asset<GrapeData>(path);
        if (isNew)
        {
            grape.displayName = name;
            grape.isRed = isRed;
            grape.liquidColor = liquid;
            grape.acidity = acidity;
            grape.alcohol = alcohol;
            grape.tannin = tannin;
            grape.body = body;
            grape.sweetness = sweetness;
            grape.hint = hint;
            grape.notes = notes;
            EditorUtility.SetDirty(grape);
        }
        return grape;
    }

    /// <summary>The first three guests of "Day 1" from the level design: two single grapes, then a blend.</summary>
    public static BarEveningSettings EveningSettings(GrapeData[] grapes)
    {
        string path = Folder + "Day1Settings.asset";
        bool isNew = !File.Exists(path);
        BarEveningSettings evening = SetupTools.Asset<BarEveningSettings>(path);
        if (evening.vocabulary == null)
        {
            evening.vocabulary = SetupTools.Asset<WineVocabulary>(Folder + "WineVocabulary.asset");
            EditorUtility.SetDirty(evening);
        }
        if (!isNew)
        {
            return evening;
        }

        GrapeData pinot = Find(grapes, "Pinot Noir");
        GrapeData syrah = Find(grapes, "Syrah");
        GrapeData grenache = Find(grapes, "Grenache");
        GrapeData riesling = Find(grapes, "Riesling");
        evening.guests = new[]
        {
            Guest("Noa", new[] { "cherry", "raspberry" }, (pinot, 100)),
            Guest("Itai", new[] { "lime", "honey" }, (riesling, 100)),
            Guest("Maya", new[] { "black pepper", "strawberry" }, (syrah, 70), (grenache, 30)),
        };
        EditorUtility.SetDirty(evening);
        return evening;
    }

    // By name and not by place on the shelf, so adding or moving a grape doesn't change the guests.
    private static GrapeData Find(GrapeData[] grapes, string displayName)
    {
        foreach (GrapeData grape in grapes)
        {
            if (grape.displayName == displayName)
            {
                return grape;
            }
        }
        Debug.LogError($"There is no grape called {displayName} for the default guests.");
        return null;
    }

    private static GuestRecipe Guest(string name, string[] notes, params (GrapeData grape, int percent)[] blend)
    {
        var recipe = new GuestRecipe { guestName = name, requestedNotes = notes, blend = new GuestRecipe.Share[blend.Length] };
        for (int i = 0; i < blend.Length; i++)
        {
            recipe.blend[i] = new GuestRecipe.Share { grape = blend[i].grape, percent = blend[i].percent };
        }
        return recipe;
    }
}
