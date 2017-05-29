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
    public class SortingLayerCodeGenerator
    {
        private static SortingLayerCodeGenerator instance;
        private CodeGeneratorCommon common;
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "SortingLayerNames";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static SortingLayerCodeGenerator()
        {
            if (instance != null) return;
            instance = new SortingLayerCodeGenerator();
            instance.common = new CodeGeneratorCommon();
            //subscripe to event
            EditorApplication.update += Update;
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
            return SortingLayer.layers.ToList().Select(x => x.name).ToList();
        }

        // update method that has to be called every frame in the editor
        private static void Update()
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
                        // name
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
