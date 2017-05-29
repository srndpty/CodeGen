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
    public class LayerCodeGenerator
    {
        private static LayerCodeGenerator instance;
        private CodeGeneratorCommon common = new CodeGeneratorCommon();
        private static CodeGeneratorCommon Com { get { return instance.common; } }

        private const string FileName = "Layers";
        private static string FilePath { get { return string.Format(CodeGeneratorCommon.FilePathFormat, CodeGeneratorCommon.DirPath, FileName); } }

        // static constructor
        static LayerCodeGenerator()
        {
            if (instance != null) return;
            instance = new LayerCodeGenerator();
            instance.common = new CodeGeneratorCommon();
            //subscripe to event
            EditorApplication.update += UpdateLayers;
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
            return Enumerable.Range(0, 32).Select(x => LayerMask.LayerToName(x)).Where(y => y.Length > 0).ToList();

            //var layers = new List<string>();
            //for (int i = 0; i < 32; i++)
            //{
            //    var name = LayerMask.LayerToName(i);
            //    if (name.Length > 0)
            //    {
            //        layers.Add(name);
            //    }
            //}
            //return layers;
        }

        // update method that has to be called every frame in the editor
        private static void UpdateLayers()
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
