using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DefaultCompany.Test
{
    // classes dont need to be static when you are using InitializeOnLoad
    [InitializeOnLoad]
    public class InputPropCodeGenerator
    {
        private static InputPropCodeGenerator instance;
        private CodeGeneratorCommon common;
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "InputProp";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static InputPropCodeGenerator()
        {
            if (instance != null) return;
            instance = new InputPropCodeGenerator();
            instance.common = new CodeGeneratorCommon();

            // force text to make this code work
            //EditorSettings.serializationMode = SerializationMode.ForceText;

            //subscripe to event
            EditorApplication.update += UpdateTags;
            // get tags
            Com.names = GetNewName();
            // write file
            if (!File.Exists(FilePath))
            {
                WriteCodeFile();
            }
        }

        static List<string> GetNewName()
        {
            // Edit > Project Settings > Editor > Asset Serialization must be `Force Text` this code to work
            var basePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            var path = string.Format("{0}/ProjectSettings/InputManager.asset", basePath);
            if (IsWindows(Environment.OSVersion.Platform))
            {
                path = path.Replace('/', '\\');
            }

            var entireString = File.ReadAllText(path);
            if (string.IsNullOrEmpty(entireString))
            {
                Debug.LogErrorFormat("Failed to load `{0}`. Make sure Edit > Project Settings > Editor > Asset Serialization is `Force Text`", path);
                return new List<string>();
            }
            var matches = Regex.Matches(entireString, "m_Name: (.+)\\r?\\n");
            if (matches.Count == 0)
            {
                Debug.LogErrorFormat("Failed to find input name from `{0}`. Make sure Edit > Project Settings > Editor > Asset Serialization is `Force Text`, and it also could be internal system of Unity has been changed.", path);
                return new List<string>();
            }

            var names = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                names.Add(matches[i].Groups[1].Value);
            }
            names = names.Distinct().ToList();
            return names;

            //return matches.Cast<Match>().ToList().Select(x => x.Groups[1].Value).Distinct().ToList();
        }

        static bool IsWindows(PlatformID id)
        {
            switch (id)
            {
                case PlatformID.Win32S:
                    return true;
                case PlatformID.Win32Windows:
                    return true;
                case PlatformID.Win32NT:
                    return true;
                case PlatformID.WinCE:
                    return true;
                case PlatformID.Unix:
                    return false;
                case PlatformID.Xbox:
                    return false;
                case PlatformID.MacOSX:
                    return false;
                default:
                    Debug.Assert(false, "Unknown platform detected: " + id.ToString());
                    return false;
            }
        }

        // update method that has to be called every frame in the editor
        private static void UpdateTags()
        {
            if (Application.isPlaying) return;

            if (EditorApplication.timeSinceStartup < Com.nextCheckTime) return;
            Com.nextCheckTime = EditorApplication.timeSinceStartup + CodeGeneratorCommon.CheckIntervalSec;

            var newNames = GetNewName();
            if (Com.SomethingHasChanged(Com.names, newNames))
            {
                Com.names = newNames;
                WriteCodeFile();
            }
        }


        // writes a file to the project folder
        private static void WriteCodeFile()
        {
            Com.WriteCodeFile(FilePath, builder =>
            {
                WrappedInt indentCount = 0;
                builder.AppendIndentLine(indentCount, Com.AutoGenTemplate);
                builder.AppendIndentLine(indentCount, Com.NameSpaceTemplate);
                using (new CurlyIndent(builder, indentCount))
                {
                    builder.AppendIndentFormatLine(indentCount, "public static class {0}", FileName);
                    using (new CurlyIndent(builder, indentCount))
                    {
                        foreach (string name in Com.names)
                        {
                            builder.AppendIndentFormatLine(indentCount, "public const string {0} = @\"{1}\";", Com.MakeIdentifier(name), Com.EscapeDoubleQuote(name));
                        }
                    }
                }
            });
        }
    }
}
