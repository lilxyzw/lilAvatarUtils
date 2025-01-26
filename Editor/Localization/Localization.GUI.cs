using System.Linq;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils
{
    internal partial class L10n : ScriptableSingleton<L10n>
    {
        // LabelField
        public static void LabelField(string key, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), options);

        public static void LabelField(string[] key, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), options);

        public static void LabelField(Rect position, string key)
            => EditorGUI.LabelField(position, G(key));

        public static void LabelField(Rect position, string[] key, GUIStyle style)
            => EditorGUI.LabelField(position, G(key), style);

        // Button
        public static bool Button(string key, params GUILayoutOption[] options)
            => GUILayout.Button(G(key), options);

        public static bool Button(string[] key, params GUILayoutOption[] options)
            => GUILayout.Button(G(key), options);

        public static bool Button(Rect rect, string key)
            => GUI.Button(rect, G(key));

        public static bool Button(Rect rect, string[] key, GUIStyle style)
            => GUI.Button(rect, G(key), style);

        // Toggle
        public static bool ToggleLeft(string[] key, bool value, params GUILayoutOption[] options)
            => EditorGUILayout.ToggleLeft(G(key), value, options);

        // Other
        public static int Toolbar(int selected, string[] texts, params GUILayoutOption[] options)
            => GUILayout.Toolbar(selected, texts.Select(t => L(t)).ToArray(), options);

        public static bool DisplayDialog(string title, string message, string ok, string cancel)
            => EditorUtility.DisplayDialog(title, L(message), L(ok), L(cancel));

        public static bool DisplayDialog(string title, string message, string ok)
            => EditorUtility.DisplayDialog(title, L(message), L(ok));

        public static bool DisplayDialog(string title, string message, string ok, params object[] args)
            => EditorUtility.DisplayDialog(title, string.Format(L(message), args), L(ok));
    }
}
