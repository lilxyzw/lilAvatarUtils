using System.Globalization;
using System.Linq;
using System.Text;

namespace jp.lilxyzw.avatarutils
{
    internal static partial class DocsGeneratorMenu
    {
        private static void BuildIndexMts(string[] langs)
        {
            var codes = langs.Select(l => l.Replace('-', '_')).ToArray();
            var sb = new StringBuilder();
            sb.AppendLine("import { defineConfig } from 'vitepress'");
            sb.AppendLine("import { shared } from './shared'");
            foreach(var code in codes)
            sb.AppendLine($"import {{ {code} }} from './{code}'");
            sb.AppendLine();
            sb.AppendLine("export default defineConfig({");
            sb.AppendLine("  ...shared,");
            sb.AppendLine("  locales: {");
            foreach(var code in codes)
            {
            var name = new CultureInfo(code.Replace('_', '-')).NativeName;
            name = name.Substring(0, name.IndexOf('(')-1);
            sb.AppendLine($"    {code}: {{ label: '{name}', ...{code} }},");
            }
            sb.AppendLine("  }");
            sb.AppendLine("})");

            WriteText($"docs/.vitepress/config/index.mts", sb.ToString());
        }
    }
}
