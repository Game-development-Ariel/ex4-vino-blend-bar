using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// The rules of the course and of our README, checked on the code itself, so breaking one fails a test
    /// instead of waiting for the grader. They read the source files in Assets/Scripts and the game's assembly.
    /// </summary>
    public class ArchitectureTests
    {
        private const BindingFlags Declared = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public |
                                              BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        // The static state the README lists in its table of statics. Anything else static (that isn't
        // a constant or a readonly value like a Color) is new static state, and must be added to the README first.
        private static readonly string[] AllowedStaticFields = { "Sfx.clips", "Sfx.source", "ScreenFader.isFading" };
        private static readonly string[] AllowedStaticProperties = { "UiTheme.TitleFont", "UiTheme.BodyFont" };

        // The three classes whose literals are tuning numbers. Their numbers belong in JudgeSettings and WineVocabulary.
        private static readonly string[] TuningFreeFiles = { "BlendJudge", "WineWords", "WineProfile" };

        private static Assembly Game => typeof(BarEvening).Assembly;

        private static IEnumerable<Type> GameTypes()
        {
            return Game.GetTypes().Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute)) && !type.Name.Contains("<"));
        }

        // (a) The game code doesn't know the UI: the UI listens to the game's events, not the other way around.
        [Test]
        public void GameCode_OutsideUI_DoesNotUseAnyUIClass()
        {
            List<SourceFiles.File> runtime = SourceFiles.Runtime();
            List<string> uiClasses = runtime.Where(file => file.Folder == "UI").Select(file => file.Name).ToList();
            var problems = new List<string>();
            foreach (SourceFiles.File file in runtime.Where(file => file.Folder != "UI"))
            {
                foreach (string uiClass in uiClasses)
                {
                    if (Regex.IsMatch(file.Code, $@"\b{uiClass}\b"))
                    {
                        problems.Add($"{file.Path} uses {uiClass}");
                    }
                }
                if (file.Code.Contains("using UnityEngine.UI;"))
                {
                    problems.Add($"{file.Path} uses UnityEngine.UI");
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // (b) Every C# event is subscribed in OnEnable and unsubscribed in OnDisable of the same class.
        // Exception, as the README says: button listeners (onClick.AddListener) are added once in Awake.
        [Test]
        public void EverySubscriptionInOnEnable_HasTheMatchingUnsubscriptionInOnDisable()
        {
            var events = new HashSet<string>(GameTypes().SelectMany(type => type.GetEvents(Declared)).Select(e => e.Name));
            Assert.IsNotEmpty(events);
            var problems = new List<string>();
            int subscriptions = 0;

            foreach (SourceFiles.File file in SourceFiles.Runtime())
            {
                (int start, int end)? onEnable = SourceFiles.MethodBody(file.Code, "OnEnable");
                (int start, int end)? onDisable = SourceFiles.MethodBody(file.Code, "OnDisable");
                foreach (Match match in Regex.Matches(file.Code, @"([\w\.]+)\s*(\+=|\.Subscribe\()\s*([\w\.]+)"))
                {
                    string eventName = match.Groups[1].Value.Split('.').Last();
                    if (!events.Contains(eventName))
                    {
                        continue;
                    }
                    subscriptions++;
                    string where = $"{file.Path}: {match.Value}";
                    if (onEnable == null || match.Index < onEnable.Value.start || match.Index > onEnable.Value.end)
                    {
                        problems.Add(where + " is not in OnEnable");
                    }
                    string unsubscribe = $@"{Regex.Escape(match.Groups[1].Value)}\s*(-=|\.Unsubscribe\()\s*{Regex.Escape(match.Groups[3].Value)}\b";
                    string disableBody = onDisable == null ? "" : file.Code.Substring(onDisable.Value.start, onDisable.Value.end - onDisable.Value.start);
                    if (!Regex.IsMatch(disableBody, unsubscribe))
                    {
                        problems.Add(where + " has no matching -= in OnDisable");
                    }
                }
            }
            Assert.Greater(subscriptions, 10, "The scan found the subscriptions of the game");
            CollectionAssert.IsEmpty(problems);
        }

        // (c) No singleton: no class keeps a static instance of itself.
        [Test]
        public void NoClass_IsASingleton()
        {
            var problems = new List<string>();
            foreach (Type type in GameTypes())
            {
                foreach (FieldInfo field in type.GetFields(Declared).Where(f => f.IsStatic && f.FieldType == type))
                {
                    problems.Add($"{type.Name}.{field.Name}");
                }
                foreach (PropertyInfo property in type.GetProperties(Declared).Where(p => p.GetMethod != null && p.GetMethod.IsStatic && p.PropertyType == type))
                {
                    problems.Add($"{type.Name}.{property.Name}");
                }
                if (type.GetMember("Instance", Declared).Length > 0)
                {
                    problems.Add($"{type.Name}.Instance");
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // (c) Every static field or property is either a constant, a readonly value (like a Color), or in the allow-list.
        [Test]
        public void EveryStaticState_IsInTheAllowListOfTheReadme()
        {
            var problems = new List<string>();
            foreach (Type type in GameTypes())
            {
                foreach (FieldInfo field in type.GetFields(Declared).Where(f => f.IsStatic && !f.IsLiteral && !f.IsDefined(typeof(CompilerGeneratedAttribute))))
                {
                    string name = $"{type.Name}.{field.Name}";
                    bool constantValue = field.IsInitOnly && field.FieldType.IsValueType;
                    if (!constantValue && !AllowedStaticFields.Contains(name))
                    {
                        problems.Add(name);
                    }
                }
                foreach (PropertyInfo property in type.GetProperties(Declared).Where(p => p.GetMethod != null && p.GetMethod.IsStatic))
                {
                    string name = $"{type.Name}.{property.Name}";
                    if (!AllowedStaticProperties.Contains(name))
                    {
                        problems.Add(name);
                    }
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // (c) Every class with static state, and every static class, is explained in the README's table of statics.
        [Test]
        public void EveryStaticClassAndStaticState_IsInTheReadmeStaticsTable()
        {
            string readme = File.ReadAllText("README.md");
            int table = readme.IndexOf("## סינגלטון ומשתנים סטטיים", StringComparison.Ordinal);
            Assert.GreaterOrEqual(table, 0, "The README has a section about statics");
            int nextSection = readme.IndexOf("\n## ", table + 1, StringComparison.Ordinal);
            string section = nextSection < 0 ? readme.Substring(table) : readme.Substring(table, nextSection - table);

            var classes = new HashSet<string>(AllowedStaticFields.Concat(AllowedStaticProperties).Select(name => name.Split('.')[0]));
            foreach (Type type in GameTypes().Where(type => type.IsAbstract && type.IsSealed && !type.IsNested))
            {
                classes.Add(type.Name);
            }
            foreach (string name in classes)
            {
                StringAssert.Contains(name, section, $"{name} is static but not in the README's table");
            }
        }

        // (d) One top-level type per file, named like the file, so a class is always where its name says.
        [Test]
        public void EveryFile_HasOneTopLevelTypeNamedLikeTheFile()
        {
            var problems = new List<string>();
            var topLevel = new Regex(@"^(?:public\s+|internal\s+)?(?:static\s+|abstract\s+|sealed\s+|partial\s+)*(?:class|interface|struct|enum)\s+(\w+)", RegexOptions.Multiline);
            foreach (SourceFiles.File file in SourceFiles.All())
            {
                List<string> types = topLevel.Matches(file.Code).Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                if (types.Count != 1 || types[0] != file.Name)
                {
                    problems.Add($"{file.Path}: {string.Join(", ", types)}");
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // (e) A tuning number in the Inspector explains itself: a [Tooltip], or a [Range] / [Min] that shows its limits.
        [Test]
        public void EverySerializedNumber_HasATooltipOrALimit()
        {
            var problems = new List<string>();
            var number = new Regex(@"\[SerializeField[^\]]*\]\s*(?:private\s+|protected\s+)?(?:float|int)\s+(\w+)");
            foreach (SourceFiles.File file in SourceFiles.Runtime())
            {
                string[] lines = file.Code.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    Match match = number.Match(lines[i]);
                    if (!match.Success)
                    {
                        continue;
                    }
                    string attributes = lines[i];
                    for (int up = i - 1; up >= 0 && lines[up].Trim().StartsWith("["); up--)
                    {
                        attributes += lines[up];
                    }
                    if (!Regex.IsMatch(attributes, @"\b(Tooltip|Range|Min)\s*\("))
                    {
                        problems.Add($"{file.Path}: {match.Groups[1].Value}");
                    }
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // (f) The README explains the code, so every class of the game is mentioned there.
        [Test]
        public void Readme_MentionsEveryClassOfTheGame()
        {
            string readme = File.ReadAllText("README.md");
            var missing = GameTypes().Where(type => !type.IsNested && !Regex.IsMatch(readme, $@"\b{type.Name}\b")).Select(type => type.Name).ToList();

            CollectionAssert.IsEmpty(missing);
        }

        // (g) The scoring and the words read their numbers from JudgeSettings and WineVocabulary.
        // Only 0 and 1 (a start, an average, "all of it") may be written in their code.
        [Test]
        public void ScoringAndWords_HaveNoTuningNumbersInTheCode()
        {
            var problems = new List<string>();
            var literal = new Regex(@"(?<![\w\.])\d+(\.\d+)?[fFdDmM]?(?![\w\.])");
            foreach (SourceFiles.File file in SourceFiles.Runtime().Where(file => TuningFreeFiles.Contains(file.Name)))
            {
                foreach (Match match in literal.Matches(file.Code))
                {
                    string value = match.Value.TrimEnd('f', 'F', 'd', 'D', 'm', 'M');
                    if (value != "0" && value != "1")
                    {
                        problems.Add($"{file.Path}: {match.Value}");
                    }
                }
            }
            Assert.AreEqual(TuningFreeFiles.Length, SourceFiles.Runtime().Count(file => TuningFreeFiles.Contains(file.Name)));
            CollectionAssert.IsEmpty(problems);
        }

        // The course checker runs dotnet-format on Assets/Scripts. These are the rules of .editorconfig that are easy to check here.
        [Test]
        public void EveryFile_FollowsTheWhitespaceRulesOfTheChecker()
        {
            var problems = new List<string>();
            foreach (SourceFiles.File file in SourceFiles.All())
            {
                if (file.Text.Contains("\r"))
                {
                    problems.Add(file.Path + ": CRLF line endings");
                }
                if (file.Text.Contains("\t"))
                {
                    problems.Add(file.Path + ": tabs");
                }
                if (!file.Text.EndsWith("\n"))
                {
                    problems.Add(file.Path + ": no final newline");
                }
                if (Regex.IsMatch(file.Text, @"[ \t]+\n"))
                {
                    problems.Add(file.Path + ": trailing spaces");
                }
            }
            CollectionAssert.IsEmpty(problems);
        }

        // The game's code is in its own assembly, which the tests reference.
        [Test]
        public void GameCode_IsInTheVinoBlendBarAssembly()
        {
            Assert.AreEqual("VinoBlendBar", Game.GetName().Name);
            Assert.AreEqual("VinoBlendBar.Editor", typeof(ProjectBuilder).Assembly.GetName().Name);
        }
    }
}
