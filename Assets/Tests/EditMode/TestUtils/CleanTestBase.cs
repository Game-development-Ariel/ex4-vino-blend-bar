using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// A base for tests that make their own ScriptableObjects and GameObjects.
    /// Everything made with Make or MakeObject is destroyed after each test.
    /// </summary>
    public abstract class CleanTestBase
    {
        private readonly List<Object> made = new List<Object>();

        [TearDown]
        public void DestroyWhatTheTestMade()
        {
            foreach (Object thing in made)
            {
                if (thing != null)
                {
                    Object.DestroyImmediate(thing);
                }
            }
            made.Clear();
        }

        protected T Make<T>() where T : ScriptableObject
        {
            T thing = ScriptableObject.CreateInstance<T>();
            made.Add(thing);
            return thing;
        }

        protected GameObject MakeObject(string name)
        {
            var thing = new GameObject(name);
            made.Add(thing);
            return thing;
        }

        /// <summary>A grape with these exact values, so a 100% blend of it has the same values.</summary>
        protected GrapeData MakeGrape(string name, bool isRed, float acidity = 5f, float alcohol = 13f, float tannin = 5f,
            float body = 5f, float sweetness = 2f, params string[] notes)
        {
            GrapeData grape = Make<GrapeData>();
            grape.displayName = name;
            grape.isRed = isRed;
            grape.acidity = acidity;
            grape.alcohol = alcohol;
            grape.tannin = tannin;
            grape.body = body;
            grape.sweetness = sweetness;
            grape.notes = notes;
            return grape;
        }

        /// <summary>Evening settings with the default numbers, a fresh vocabulary and judge, and this many empty guests.</summary>
        protected BarEveningSettings MakeSettings(int guests = 3)
        {
            BarEveningSettings settings = Make<BarEveningSettings>();
            settings.vocabulary = Make<WineVocabulary>();
            settings.judge = Make<JudgeSettings>();
            settings.guests = new GuestRecipe[guests];
            for (int i = 0; i < guests; i++)
            {
                settings.guests[i] = new GuestRecipe { guestName = "Guest " + i };
            }
            return settings;
        }

        protected static Blend Pure(GrapeData grape)
        {
            var blend = new Blend();
            blend.Add(grape, Blend.Full);
            return blend;
        }
    }
}
