using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    internal static partial class DocsGeneratorMenu
    {
        private static int frameCount = 0;
        private static string currentLang;
        private static HashSet<string> queue = new();
        private static Type[] types;

        [MenuItem("Help/DocsGenerator/lilAvatarUtils")]
        private static void Generate()
        {
            var sceneView = SceneView.lastActiveSceneView;
            sceneView.pivot = new(-0.07f, 1.04f, -0.41f);
            sceneView.rotation = new(-0.00663f, 0.99329f, -0.07495f, -0.08783f);
            sceneView.size = 0.616483f;

            currentLang = AvatarUtilsSettings.instance.language;
            var asms = new HashSet<Assembly>(){Assembly.GetExecutingAssembly()};
            types = asms.SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttribute<DocsAttribute>() != null).ToArray();
            var langs = L10n.GetLanguages();
            queue.UnionWith(langs);
            EditorApplication.update += Next;
            BuildIndexMts(langs);
        }

        private static void Next()
        {
            EditorApplication.update -= Next;
            frameCount = 0;
            windowQueue.Clear();
            if(queue.Count > 0)
            {
                var lang = queue.First();
                queue.Remove(lang);
                AvatarUtilsSettings.instance.language = lang;
                L10n.Load();
                var path = AssetDatabase.GUIDToAssetPath("f4bbb8506708257469d53648b2e0854f") + "/" + lang + ".po";
                var localizationAsset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(path);
                Func<string,string> loc = localizationAsset.GetLocalizedString;
                var code = lang.Replace('-', '_');
                var root = $"docs/{code}";
                DocsGenerator.Generate(
                    (t) => TypeToPath(root, t),
                    loc,
                    GetHeader,
                    GetTooltip,
                    NeedToDraw,
                    (t,sb) => ActionPerType(t,sb,code),
                    types);

                BuildHome(root, code, loc);
                BuildDocsIndex(root, code, loc);
                BuildIndex(root, code, loc);
            }
            else
            {
                AvatarUtilsSettings.instance.language = currentLang;
            }
        }

        private static (string,string) GetHeader(FieldInfo field)
        {
            var header = field.GetCustomAttribute<HeaderAttribute>()?.header;
            return (header, null);
        }

        private static string GetTooltip(FieldInfo field) => field.GetCustomAttribute<TooltipAttribute>()?.tooltip;
        private static bool NeedToDraw(FieldInfo field) => field.GetCustomAttribute<SerializeField>() != null || !field.IsNotSerialized && !field.IsStatic && field.IsPublic;

        private static string TypeToPath(string root, Type type)
        {
            return $"{root}/docs/{type.Name}.md";
        }

        private static void WriteText(string path, string text)
        {
            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private static void WriteBytes(string path, byte[] bytes)
        {
            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, bytes);
        }
    }
}
