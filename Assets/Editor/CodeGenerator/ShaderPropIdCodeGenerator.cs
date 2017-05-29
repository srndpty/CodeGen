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
    public class ShaderPropIdCodeGenerator
    {
        private static ShaderPropIdCodeGenerator instance;
        private CodeGeneratorCommon common = new CodeGeneratorCommon();
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "ShaderPropId";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static ShaderPropIdCodeGenerator()
        {
            if (instance != null) return;
            instance = new ShaderPropIdCodeGenerator();
            instance.common = new CodeGeneratorCommon();
            //subscripe to event
            EditorApplication.update += UpdateAnimParams;
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
            var names = new List<string>();
            var shaders = AssetDatabase.FindAssets("t:shader");
            foreach (var shader in shaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(shader);
                var item = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (item == null) continue;

                var count = ShaderUtil.GetPropertyCount(item);
                for (int i = 0; i < count; i++)
                {
                    var name = ShaderUtil.GetPropertyName(item, i);
                    names.Add(name);
                }
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
                            builder.AppendIndentFormatLine(indentCount, "public const string {0}{1} = \"{2}\";", CodeGeneratorCommon.StringPrefix, Com.MakeIdentifier(name), Com.EscapeDoubleQuote(name));
                        }
                        builder.Append(Environment.NewLine);
                        // hash storage
                        foreach (string name in Com.names)
                        {
                            builder.AppendIndentFormatLine(indentCount, "public const int {0} = {1};", Com.MakeIdentifier(name), Shader.PropertyToID(name));
                        }
                    }
                }
            });
        }
    }
}
