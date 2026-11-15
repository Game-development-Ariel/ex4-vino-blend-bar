using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VinoBlendBar.Tests.EditMode
{
    public class BarEveningSettingsTests : CleanTestBase
    {
        [Test]
        public void OnValidate_WarnsAboutAGuestWhoseBlendDoesNotFillTheGlass()
        {
            BarEveningSettings settings = MakeSettings(0);
            settings.name = "Broken";
            settings.guests = new[] { TestData.Recipe("Dana", null, (MakeGrape("Syrah", true), 90)) };

            LogAssert.Expect(LogType.Warning, new Regex("Broken, guest Dana: the blend adds up to 90%, not 100%"));
            Reflect.Call(settings, "OnValidate");
        }

        [Test]
        public void OnValidate_WarnsAboutAnEmptyNote()
        {
            BarEveningSettings settings = MakeSettings(0);
            settings.guests = new[] { TestData.Recipe("Dana", new[] { "" }, (MakeGrape("Syrah", true), 100)) };

            LogAssert.Expect(LogType.Warning, new Regex("guest Dana: one of the requested notes is empty"));
            Reflect.Call(settings, "OnValidate");
        }

        [Test]
        public void OnValidate_WarnsAboutAGuestWithoutGrapes()
        {
            BarEveningSettings settings = MakeSettings(0);
            settings.guests = new[] { new GuestRecipe { guestName = "Dana" } };

            LogAssert.Expect(LogType.Warning, new Regex("guest Dana: the blend is empty"));
            Reflect.Call(settings, "OnValidate");
        }

        [Test]
        public void OnValidate_OfValidGuests_SaysNothing()
        {
            BarEveningSettings settings = MakeSettings(0);
            settings.guests = new[] { TestData.Recipe("Dana", new[] { "plum" }, (MakeGrape("Merlot", true), 100)) };

            Reflect.Call(settings, "OnValidate");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void OnValidate_WithoutGuests_SaysNothing()
        {
            BarEveningSettings settings = MakeSettings(0);
            settings.guests = null;

            Reflect.Call(settings, "OnValidate");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Defaults_AreTheNumbersOfTheGameDesign()
        {
            BarEveningSettings settings = Make<BarEveningSettings>();

            Assert.AreEqual(75f, settings.patienceSeconds);
            Assert.AreEqual(5f, settings.tasteCostSeconds);
            Assert.AreEqual(5f, settings.maxStars);
            Assert.AreEqual(3f, settings.starsToPass);
        }
    }
}
