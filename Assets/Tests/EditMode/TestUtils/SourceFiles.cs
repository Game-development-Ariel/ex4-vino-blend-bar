using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// The C# files of the game, read as text, for the tests that check the rules of the course.
    /// </summary>
    public static class SourceFiles
    {
        public const string ScriptsFolder = "Assets/Scripts";

        /// <summary>One source file: where it is, its folder under Assets/Scripts, and its code without comments and strings.</summary>
        public class File
        {
            public string Path;
            public string Folder;
            public string Name;
            public string Text;
            public string Code;
        }

        /// <summary>Every .cs file under Assets/Scripts, including the editor tools.</summary>
        public static List<File> All()
        {
            var files = new List<File>();
            foreach (string path in Directory.GetFiles(ScriptsFolder, "*.cs", SearchOption.AllDirectories))
            {
                string relative = path.Replace('\\', '/').Substring(ScriptsFolder.Length + 1);
                string text = System.IO.File.ReadAllText(path);
                files.Add(new File
                {
                    Path = path.Replace('\\', '/'),
                    Folder = relative.Contains("/") ? relative.Substring(0, relative.IndexOf('/')) : "",
                    Name = System.IO.Path.GetFileNameWithoutExtension(path),
                    Text = text,
                    Code = WithoutCommentsAndStrings(text),
                });
            }
            return files;
        }

        /// <summary>The files of the game itself, without the editor tools.</summary>
        public static List<File> Runtime()
        {
            return All().FindAll(file => file.Folder != "Editor");
        }

        /// <summary>
        /// The code with every comment removed and every string and char emptied (the quotes stay),
        /// so a word inside a comment or a text doesn't count as code. Line breaks are kept.
        /// </summary>
        public static string WithoutCommentsAndStrings(string text)
        {
            var code = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                char next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (c == '/' && next == '/')
                {
                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }
                }
                else if (c == '/' && next == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                    {
                        if (text[i] == '\n')
                        {
                            code.Append('\n');
                        }
                        i++;
                    }
                    i += 2;
                }
                else if (c == '"' || c == '\'')
                {
                    code.Append(c);
                    i++;
                    while (i < text.Length && text[i] != c && text[i] != '\n')
                    {
                        i += text[i] == '\\' ? 2 : 1;
                    }
                    code.Append(c);
                    i++;
                }
                else
                {
                    code.Append(c);
                    i++;
                }
            }
            return code.ToString();
        }

        /// <summary>The start and end (index of the closing brace) of a method's body, or null if the file has no such method.</summary>
        public static (int start, int end)? MethodBody(string code, string method)
        {
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(code, @"\b" + method + @"\s*\([^)]*\)\s*\{");
            if (!match.Success)
            {
                return null;
            }
            int start = match.Index + match.Length;
            int depth = 1;
            for (int i = start; i < code.Length; i++)
            {
                if (code[i] == '{')
                {
                    depth++;
                }
                else if (code[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return (start, i);
                    }
                }
            }
            return null;
        }
    }
}
