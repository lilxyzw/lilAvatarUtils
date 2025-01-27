using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.avatarutils
{
    internal static partial class DocsGeneratorMenu
    {
        private static HashSet<(Type type, EditorWindow window, string lang)> windowQueue = new();
        private static Object SampleObject => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath("eb69ff4f02591fa459063499dbaf58aa"));

        private static void Capture()
        {
            if(frameCount++ < 10) return;
            foreach(var (type, window, lang) in windowQueue)
            {
                window.Focus();
                var pos = new Vector2(window.position.x, window.position.y);
                var w = (int)window.position.width;
                var h = (int)window.position.yMax + 21 - (int)pos.y;
                var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(pos, w, h);
                var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);

                var path = $"docs/public/images/{lang}/{type.Name}.png";
                var directory = Path.GetDirectoryName(path);
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                WriteBytes(path, texture.EncodeToPNG());

                window.Close();
            }
            EditorApplication.update -= Capture;
            EditorApplication.update += Next;
        }

        private static void ActionPerType(Type type, StringBuilder sb, string lang)
        {
            if(!AvatarUtils.TabTypes.Any(t => t.Item1 == type)) return;
            frameCount = 0;
            if(windowQueue.Count == 0) EditorApplication.update += Capture;

            sb.AppendLine($"![{type.Name}](/images/{lang}/{type.Name}.png \"{type.Name}\")");

            var window = EditorWindow.CreateInstance(typeof(AvatarUtils)) as AvatarUtils;
            window.titleContent = new GUIContent($"{AvatarUtils.TEXT_WINDOW_NAME} {PackageJsonReader.GetVersion()}");
            window.gameObject = SampleObject as GameObject;
            window.Show();

            int width = 1280;
            int height = 720;

            if(type == typeof(TexturesGUI)) window.editorMode = AvatarUtils.EditorMode.Textures;
            else if(type == typeof(MaterialsGUI)) window.editorMode = AvatarUtils.EditorMode.Materials;
            else if(type == typeof(AnimationClipGUI)) window.editorMode = AvatarUtils.EditorMode.Animations;
            else if(type == typeof(RenderersGUI)) window.editorMode = AvatarUtils.EditorMode.Renderers;
            #if LIL_VRCSDK3_AVATARS
            else if(type == typeof(PhysBonesGUI)) window.editorMode = AvatarUtils.EditorMode.PhysBones;
            else if(type == typeof(PhysBoneCollidersGUI)) window.editorMode = AvatarUtils.EditorMode.PBColliders;
            #endif
            else if(type == typeof(LightingTestGUI)) window.editorMode = AvatarUtils.EditorMode.Lighting;
            else if(type == typeof(UtilsGUI)) window.editorMode = AvatarUtils.EditorMode.Utils;

            window.position = new Rect(10, 10, width, height-1);
            windowQueue.Add((type, window, lang));
        }
    }
}
