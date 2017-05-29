using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefaultCompany.Test
{
    // classes dont need to be static when you are using InitializeOnLoad
    [InitializeOnLoad]
    public class AnimParamCodeGenerator
    {
        private static AnimParamCodeGenerator instance;
        private CodeGeneratorCommon common = new CodeGeneratorCommon();
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "AnimParam";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static AnimParamCodeGenerator()
        {
            if (instance != null) return;
            instance = new AnimParamCodeGenerator();
            instance.common = new CodeGeneratorCommon();

            EditorApplication.update += UpdateAnimParams;
            Com.names = GetNewName();

            // first time creation
            if (!File.Exists(FilePath))
            {
                WriteCodeFile();
            }
        }

        static List<string> GetNewName()
        {
            var names = new List<string>();
            var anims = AssetDatabase.FindAssets("t:animatorcontroller");
            foreach (var anim in anims)
            {
                var path = AssetDatabase.GUIDToAssetPath(anim);
                var item = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
                if (item == null) continue;

                item.parameters.ToList().ForEach(x => names.Add(x.name));
            }

            names = names.Distinct().ToList();

            return names;
        }

        // update method that has to be called every frame in the editor
        private static void UpdateAnimParams()
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
                builder.AppendIndentLine(indentCount, "using UnityEngine;");
                builder.Append(Environment.NewLine);
                builder.AppendIndentLine(indentCount, Com.NameSpaceTemplate);
                using (new CurlyIndent(builder, indentCount))
                {
                    builder.AppendIndentFormatLine(indentCount, "public static class {0}", FileName);
                    using (new CurlyIndent(builder, indentCount))
                    {
                        // string
                        foreach (string name in Com.names)
                        {
                            builder.AppendIndentFormatLine(indentCount, "public const string {0}{1} = @\"{2}\";", CodeGeneratorCommon.StringPrefix, Com.MakeIdentifier(name), Com.EscapeDoubleQuote(name));
                        }
                        builder.Append(Environment.NewLine);
                        // hash storage
                        foreach (string name in Com.names)
                        {
                            builder.AppendIndentFormatLine(indentCount, "public const int {0} = {1};", Com.MakeIdentifier(name), Animator.StringToHash(name));
                        }
                    }
                }
            });
        }
    }
}
