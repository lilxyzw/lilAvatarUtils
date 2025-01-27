using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace jp.lilxyzw.avatarutils
{
    internal static partial class DocsGeneratorMenu
    {
        private static void BuildDocsIndex(string root, string code, Func<string,string> loc)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {loc("Documents")}");
            sb.AppendLine();
            sb.AppendLine(loc("This is the documentation page for lilAvatarUtils."));
            sb.AppendLine();
            sb.AppendLine($"<div class=\"table-of-contents\">");
            sb.AppendLine($"    <ul>");

            var added = new HashSet<string>();
            // メインウィンドウ
            var mainPath = root+"/docs/AvatarUtils.md";
            var mainTitle = File.ReadLines(mainPath).First().Substring(2);
            added.Add(Path.GetFileNameWithoutExtension(mainPath));
            sb.AppendLine($"    <li><a href=\"./{Path.GetFileNameWithoutExtension(mainPath)}\">{mainTitle}</a></li>");

            // タブ
            var tabtypes = AvatarUtils.TabTypes.Select(t => root+"/docs/"+t.Item1.Name+".md");
            foreach(var file in tabtypes)
            {
            var title = File.ReadLines(file).First().Substring(2);
            added.Add(Path.GetFileNameWithoutExtension(file));
            sb.AppendLine($"    <li><a href=\"./{Path.GetFileNameWithoutExtension(file)}\">{title}</a></li>");
            }

            // 通常のクラス
            foreach(var file in Directory.GetFiles(root+"/docs", "*.md", SearchOption.TopDirectoryOnly))
            {
            if(file.EndsWith("index.md") || file.EndsWith("Settings.md") || !added.Add(Path.GetFileNameWithoutExtension(file))) continue;
            var title = File.ReadLines(file).First().Substring(2);
            sb.AppendLine($"    <li><a href=\"./{Path.GetFileNameWithoutExtension(file)}\">{title}</a></li>");
            }

            sb.AppendLine($"    </ul>");
            sb.AppendLine($"</div>");

            WriteText($"{root}/docs/index.md", sb.ToString());
        }
    }
}
