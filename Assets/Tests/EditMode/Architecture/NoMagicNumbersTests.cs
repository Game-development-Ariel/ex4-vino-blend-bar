using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

/// <summary>
/// No magic numbers: every number in Assets/Scripts (the game and the editor tools) comes from a name.
/// A number may be written as is only where it gets its name or explanation:
/// - in a const or static readonly declaration (with a comment that says what it means);
/// - as the default of a serialized field (the Tooltip explains it in the Inspector);
/// - inside an attribute, like [Range(0, 1)] or [Tooltip("...")].
/// Everywhere else only 0, 1, 2 and 0.5 are allowed: nothing, one, double or half, and the first items of an array.
/// The test reads the code as text: comments and strings are removed first, so "Day 1" or a note like "// 3 tiles" is fine.
/// </summary>
public class NoMagicNumbersTests
{
    private static readonly Regex BlockComment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
    private static readonly Regex LineComment = new Regex(@"//[^\n]*");
    private static readonly Regex TextOrChar = new Regex(@"@?\$?""(?:[^""\\\n]|\\.)*""|'(?:[^'\\]|\\.)'");
    private static readonly Regex NamedDeclaration = new Regex(@"\b(?:const|static\s+readonly)\b[^;]*;", RegexOptions.Singleline);
    private static readonly Regex FieldDefault = new Regex(
        @"(?:\[[^\]\n]*\bSerializeField\b[^\]\n]*\](?:\s*\[[^\]]*\])*\s*(?:private|protected)?\s*|(?<![\w.])public\s+)[\w<>\[\],. ]+?\s+\w+\s*=(?!>)[^;]*;");
    private static readonly Regex Attribute = new Regex(@"^\s*\[[^\]\n]*\]", RegexOptions.Multiline);
    private static readonly Regex Number = new Regex(@"(?<![\w.])\d+(?:\.\d+)?[fFdDmM]?(?![\w.])");

    private static readonly HashSet<string> AllowedValues = new HashSet<string> { "0", "1", "2", "0.5" };

    private static string KeepLines(Match match)
    {
        return new string('\n', match.Value.Count(c => c == '\n'));
    }

    private static string WithoutNamedPlaces(string code)
    {
        code = BlockComment.Replace(code, KeepLines);
        code = TextOrChar.Replace(code, "\"\"");
        code = LineComment.Replace(code, "");
        code = NamedDeclaration.Replace(code, KeepLines);
        code = FieldDefault.Replace(code, KeepLines);
        return Attribute.Replace(code, "");
    }

    private static string Value(string literal)
    {
        string value = literal.TrimEnd('f', 'F', 'd', 'D', 'm', 'M');
        return value.EndsWith(".0") ? value.Substring(0, value.Length - 2) : value;
    }

    [Test]
    public void EveryNumberInTheCode_HasAName()
    {
        var problems = new List<string>();
        foreach (string path in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories).OrderBy(path => path))
        {
            string code = File.ReadAllText(path).Replace("\r\n", "\n");
            string[] lines = WithoutNamedPlaces(code).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (Match number in Number.Matches(lines[i]))
                {
                    if (!AllowedValues.Contains(Value(number.Value)))
                    {
                        problems.Add($"{path.Replace('\\', '/')}:{i + 1}: {number.Value} in \"{lines[i].Trim()}\"");
                    }
                }
            }
        }

        Assert.IsEmpty(problems, "Give these numbers a name (a const with a comment, or a serialized field with a Tooltip):\n" + string.Join("\n", problems));
    }
}
