using System.Text;
using System;

/// <summary>
/// ブリッジの名前空間
/// </summary>
namespace DefaultCompany.Test
{
    public static class StringBuilderExtensionMethods
    {
        /// <summary>tab == 4spaces</summary>
        public const string TabRepresentation = "    ";

        public static void AppendIndent(this StringBuilder self, int indentCount, string text)
        {
            // j AppendIndentFormatで統一すると、文字列に{や}が含まれているときにパースに失敗するので分けるしかない
            // e if you use AppendFormat in this context, format will be broken if there is single '{' or '}' in the string. so implementations must be separated.
            for (int i = 0; i < indentCount; i++)
            {
                self.Append(TabRepresentation);
            }
            self.Append(text);
        }

        public static void AppendIndentLine(this StringBuilder self, int indentCount, string text)
        {
            self.AppendIndent(indentCount, text);
            self.Append(Environment.NewLine);
        }

        public static void AppendIndentFormat(this StringBuilder self, int indentCount, string text, params object[] param)
        {
            for (int i = 0; i < indentCount; i++)
            {
                self.Append(TabRepresentation);
            }
            self.AppendFormat(text, param);
        }

        public static void AppendIndentFormatLine(this StringBuilder self, int indentCount, string text, params object[] param)
        {
            self.AppendIndentFormat(indentCount, text, param);
            self.Append(Environment.NewLine);
        }
    }
}
