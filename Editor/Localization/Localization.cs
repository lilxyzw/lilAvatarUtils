using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    internal partial class L10n : ScriptableSingleton<L10n>
    {
        public LocalizationAsset localizationAsset;
        private static string[] languages;
        private static string[] languageNames;
        private static readonly Dictionary<string, GUIContent> guicontents = new();
        private static string localizationFolder => AssetDatabase.GUIDToAssetPath("f4bbb8506708257469d53648b2e0854f");

        internal static void Load()
        {
            guicontents.Clear();
            var path = localizationFolder + "/" + AvatarUtilsSettings.instance.language + ".po";
            if(File.Exists(path)) instance.localizationAsset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(path);

            if(!instance.localizationAsset) instance.localizationAsset = new LocalizationAsset();
        }

        internal static string[] GetLanguages()
        {
            return languages ??= Directory.GetFiles(localizationFolder).Where(f => f.EndsWith(".po")).Select(f => Path.GetFileNameWithoutExtension(f)).Where(f => !f.StartsWith(".")).ToArray();
        }

        internal static string[] GetLanguageNames()
        {
            return languageNames ??= languages.Select(l => {
                if(l == "zh-Hans") return "简体中文";
                if(l == "zh-Hant") return "繁體中文";
                return new CultureInfo(l).NativeName;
            }).ToArray();
        }

        internal static string L(string key)
        {
            if(!instance.localizationAsset) Load();
            return instance.localizationAsset.GetLocalizedString(key);
        }

        private static GUIContent G(string key) => G(key, null, "");
        private static GUIContent G(string[] key) => key.Length == 2 ? G(key[0], null, key[1]) : G(key[0], null, null);
        internal static GUIContent G(string key, string tooltip) => G(key, null, tooltip); // From EditorToolboxSettings

        private static GUIContent G(string key, Texture image, string tooltip)
        {
            if(!instance.localizationAsset) Load();
            var k = key+tooltip;
            if(guicontents.TryGetValue(k, out var content)) return content;
            return guicontents[k] = new GUIContent(L(key), image, L(tooltip));
        }
    }
}
