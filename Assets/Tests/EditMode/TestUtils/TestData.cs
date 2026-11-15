using NUnit.Framework;
using UnityEditor;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// Loads the real data of the game from Assets/Data, and builds blends and guests from it.
    /// </summary>
    public static class TestData
    {
        public const string DataFolder = "Assets/Data/";
        public const string GrapesFolder = DataFolder + "Grapes/";
        public const string ScenePath = "Assets/Scenes/WineBar.unity";

        // The asset names of the eight grapes, in shelf order.
        public static readonly string[] GrapeAssetNames =
        {
            "CabernetSauvignon", "Merlot", "PinotNoir", "Syrah", "Grenache", "SauvignonBlanc", "Chardonnay", "Riesling",
        };

        public static BarEveningSettings Day1 => Load<BarEveningSettings>(DataFolder + "Day1Settings.asset");
        public static WineVocabulary Vocabulary => Load<WineVocabulary>(DataFolder + "WineVocabulary.asset");
        public static JudgeSettings Judge => Load<JudgeSettings>(DataFolder + "JudgeSettings.asset");

        /// <summary>A grape by its asset name, for example "PinotNoir".</summary>
        public static GrapeData Grape(string assetName)
        {
            return Load<GrapeData>(GrapesFolder + assetName + ".asset");
        }

        /// <summary>A blend from pairs of asset name and percent: MakeBlend("Syrah", 70, "Grenache", 30).</summary>
        public static Blend MakeBlend(params object[] namesAndPercents)
        {
            var blend = new Blend();
            for (int i = 0; i < namesAndPercents.Length; i += 2)
            {
                Assert.IsTrue(blend.Add(Grape((string)namesAndPercents[i]), (int)namesAndPercents[i + 1]), "The test blend overflows");
            }
            return blend;
        }

        /// <summary>The order of one of the Day 1 guests, with the real vocabulary.</summary>
        public static GuestOrder Day1Order(int guest)
        {
            BarEveningSettings day1 = Day1;
            return new GuestOrder(day1.guests[guest], day1.vocabulary);
        }

        public static GuestRecipe Recipe(string name, string[] notes, params (GrapeData grape, int percent)[] shares)
        {
            var recipe = new GuestRecipe { guestName = name, requestedNotes = notes, blend = new GuestRecipe.Share[shares.Length] };
            for (int i = 0; i < shares.Length; i++)
            {
                recipe.blend[i] = new GuestRecipe.Share { grape = shares[i].grape, percent = shares[i].percent };
            }
            return recipe;
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, $"Could not load {path}");
            return asset;
        }
    }
}
