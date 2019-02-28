using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DefaultCompany.Test
{
    // classes dont need to be static when you are using InitializeOnLoad
    [InitializeOnLoad]
    public class SceneNameCodeGenerator
    {
        private static SceneNameCodeGenerator instance;
        private CodeGeneratorCommon common = new CodeGeneratorCommon();
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "SceneNames";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static SceneNameCodeGenerator()
        {
            if (instance != null) return;
            instance = new SceneNameCodeGenerator();
            instance.common = new CodeGeneratorCommon();
            //subscripe to event
            EditorApplication.update += UpdateSceneNames;
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
            return EditorBuildSettings.scenes.Select(x => x.path).ToList();
        }

        // update method that has to be called every frame in the editor
        private static void UpdateSceneNames()
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
                            // ex) assets/scenes/menu.unity -> menu 
                            var tail = name.Substring(name.LastIndexOf('/') + 1);
                            var result = tail.Substring(0, tail.LastIndexOf('.'));
                            builder.AppendIndentFormatLine(indentCount, "public const string {0} = @\"{1}\";", Com.MakeIdentifier(result), Com.EscapeDoubleQuote(name));
                        }
                    }
                }
            });
        }
    }
}
