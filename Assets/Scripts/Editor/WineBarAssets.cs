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

    // The values of each grape, on the GrapeData scale: acidity, tannin, body and sweetness
    // from 1 to 10, and alcohol in percent. The color is the color of its wine in the glass.

    private static readonly Color CabernetColor = new Color(0.36f, 0.04f, 0.12f);
    private const float CabernetAcidity = 6f;
    private const float CabernetAlcohol = 14f;
    private const float CabernetTannin = 9f;
    private const float CabernetBody = 8f;
    private const float CabernetSweetness = 2f;

    private static readonly Color MerlotColor = new Color(0.48f, 0.07f, 0.16f);
    private const float MerlotAcidity = 5f;
    private const float MerlotAlcohol = 13.5f;
    private const float MerlotTannin = 5f;
    private const float MerlotBody = 6f;
    private const float MerlotSweetness = 3f;

    private static readonly Color PinotColor = new Color(0.62f, 0.13f, 0.2f);
    private const float PinotAcidity = 7f;
    private const float PinotAlcohol = 13f;
    private const float PinotTannin = 3f;
    private const float PinotBody = 3f;
    private const float PinotSweetness = 2f;

    private static readonly Color SyrahColor = new Color(0.3f, 0.03f, 0.16f);
    private const float SyrahAcidity = 5f;
    private const float SyrahAlcohol = 14.5f;
    private const float SyrahTannin = 7f;
    private const float SyrahBody = 8f;
    private const float SyrahSweetness = 2f;

    private static readonly Color GrenacheColor = new Color(0.66f, 0.16f, 0.18f);
    private const float GrenacheAcidity = 4f;
    private const float GrenacheAlcohol = 15f;
    private const float GrenacheTannin = 4f;
    private const float GrenacheBody = 6f;
    private const float GrenacheSweetness = 3f;

    private static readonly Color SauvignonBlancColor = new Color(0.88f, 0.92f, 0.55f);
    private const float SauvignonBlancAcidity = 9f;
    private const float SauvignonBlancAlcohol = 12.5f;
    private const float SauvignonBlancTannin = 1f;
    private const float SauvignonBlancBody = 3f;
    private const float SauvignonBlancSweetness = 2f;

    private static readonly Color ChardonnayColor = new Color(0.95f, 0.83f, 0.42f);
    private const float ChardonnayAcidity = 5f;
    private const float ChardonnayAlcohol = 13.5f;
    private const float ChardonnayTannin = 1f;
    private const float ChardonnayBody = 7f;
    private const float ChardonnaySweetness = 2f;

    private static readonly Color RieslingColor = new Color(0.93f, 0.9f, 0.62f);
    private const float RieslingAcidity = 9f;
    private const float RieslingAlcohol = 10f;
    private const float RieslingTannin = 1f;
    private const float RieslingBody = 3f;
    private const float RieslingSweetness = 6f;

    // The third guest's blend, in percent of the glass. Together they make Blend.Full.
    private const int MayaSyrahPercent = 70;
    private const int MayaGrenachePercent = 30;

    /// <summary>The eight grapes, in shelf order. The values are the ones from our prototype.</summary>
    public static GrapeData[] Grapes()
    {
        return new[]
        {
            Grape("Cabernet Sauvignon", true, CabernetColor,
                CabernetAcidity, CabernetAlcohol, CabernetTannin, CabernetBody, CabernetSweetness,
                "Grippy, full\nblackcurrant, cedar", "blackcurrant", "cedar", "mint"),
            Grape("Merlot", true, MerlotColor,
                MerlotAcidity, MerlotAlcohol, MerlotTannin, MerlotBody, MerlotSweetness,
                "Soft, round\nplum, chocolate", "plum", "chocolate"),
            Grape("Pinot Noir", true, PinotColor,
                PinotAcidity, PinotAlcohol, PinotTannin, PinotBody, PinotSweetness,
                "Light, fresh\ncherry, raspberry", "cherry", "raspberry", "mushroom"),
            Grape("Syrah", true, SyrahColor,
                SyrahAcidity, SyrahAlcohol, SyrahTannin, SyrahBody, SyrahSweetness,
                "Bold, spicy\nblackberry, pepper", "blackberry", "black pepper", "smoke"),
            Grape("Grenache", true, GrenacheColor,
                GrenacheAcidity, GrenacheAlcohol, GrenacheTannin, GrenacheBody, GrenacheSweetness,
                "Warm, juicy\nstrawberry, spice", "strawberry", "cinnamon"),
            Grape("Sauvignon Blanc", false, SauvignonBlancColor,
                SauvignonBlancAcidity, SauvignonBlancAlcohol, SauvignonBlancTannin, SauvignonBlancBody, SauvignonBlancSweetness,
                "Zesty, light\ngrapefruit, grass", "grapefruit", "cut grass"),
            Grape("Chardonnay", false, ChardonnayColor,
                ChardonnayAcidity, ChardonnayAlcohol, ChardonnayTannin, ChardonnayBody, ChardonnaySweetness,
                "Creamy, full\nbutter, apple", "butter", "apple", "vanilla"),
            Grape("Riesling", false, RieslingColor,
                RieslingAcidity, RieslingAlcohol, RieslingTannin, RieslingBody, RieslingSweetness,
                "Racy, a bit sweet\nlime, honey", "lime", "honey", "peach"),
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
        if (evening.judge == null)
        {
            evening.judge = SetupTools.Asset<JudgeSettings>(Folder + "JudgeSettings.asset");
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
            Guest("Noa", new[] { "cherry", "raspberry" }, (pinot, Blend.Full)),
            Guest("Itai", new[] { "lime", "honey" }, (riesling, Blend.Full)),
            Guest("Maya", new[] { "black pepper", "strawberry" }, (syrah, MayaSyrahPercent), (grenache, MayaGrenachePercent)),
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
