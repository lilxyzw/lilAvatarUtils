using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.avatarutils
{
    internal class MaterialCleaner
    {
        internal static HashSet<Material> RemoveUnusedProperties(IEnumerable<Material> materials)
        {
            var propMap = materials.Select(m => m.shader).Distinct().Where(s => s).ToDictionary(s => s, s => new ShaderPropertyContainer(s));
            var cleanedMaterials = new HashSet<Material>();
            foreach(var material in materials)
            {
                RemoveUnusedProperties(material, cleanedMaterials, propMap);
            }
            return cleanedMaterials;
        }

        // シェーダーで使われていないプロパティを除去
        private static void RemoveUnusedProperties(Material material, HashSet<Material> cleanedMaterials, Dictionary<Shader, ShaderPropertyContainer> propMap)
        {
            using var so = new SerializedObject(material);
            so.Update();
            using var savedProps = so.FindProperty("m_SavedProperties");
            if(material.shader)
            {
                var dic = propMap[material.shader];
                DeleteUnused(savedProps, "m_TexEnvs", dic.textures);
                DeleteUnused(savedProps, "m_Floats", dic.floats);
                DeleteUnused(savedProps, "m_Colors", dic.vectors);

                var shaderKeywords = material.shader.keywordSpace.keywordNames.ToHashSet();
                DeleteUnusedKeywords(so.FindProperty("m_ValidKeywords"), shaderKeywords);
                DeleteUnusedKeywords(so.FindProperty("m_InvalidKeywords"), shaderKeywords);
            }

            if(so.ApplyModifiedProperties())
            {
                Debug.Log($"[lilAvatarUtils] Clean up {material.name}", material);
                cleanedMaterials.Add(material);
            }

            if(material.parent)
            {
                RemoveUnusedProperties(material.parent, cleanedMaterials, propMap);
            }
        }

        private static void DeleteUnused(SerializedProperty prop, string name, HashSet<string> names)
        {
            using var props = prop.FindPropertyRelative(name);
            if(props.arraySize == 0) return;
            int i = 0;
            var size = props.arraySize;
            var p = props.GetArrayElementAtIndex(i);
            void DeleteUnused()
            {
                if(!names.Contains(GetStringInProperty(p, "first")))
                {
                    p.DeleteCommand();
                    if(i < --size)
                    {
                        p = props.GetArrayElementAtIndex(i);
                        DeleteUnused();
                    }
                }
                else if(p.NextVisible(false) && ++i < size) DeleteUnused();
            }
            DeleteUnused();
        }

        internal static string GetStringInProperty(SerializedProperty serializedProperty, string name)
        {
            using var prop = serializedProperty.FindPropertyRelative(name);
            return prop.stringValue;
        }

        private static void DeleteUnusedKeywords(SerializedProperty props, HashSet<string> shaderKeywords)
        {
            for(int i = props.arraySize - 1; i >= 0; i--)
                if(!shaderKeywords.Contains(props.GetArrayElementAtIndex(i).stringValue))
                    props.DeleteArrayElementAtIndex(i);
        }

        // シェーダーのプロパティを検索して保持するクラス
        private class ShaderPropertyContainer
        {
            internal HashSet<string> textures;
            internal HashSet<string> floats;
            internal HashSet<string> vectors;

            internal ShaderPropertyContainer(Shader shader)
            {
                textures = new HashSet<string>();
                floats = new HashSet<string>();
                vectors = new HashSet<string>();

                var count = shader.GetPropertyCount();

                for(int i = 0; i < count; i++)
                {
                    var t = shader.GetPropertyType(i);
                    var name = shader.GetPropertyName(i);
                    if(t == ShaderPropertyType.Texture) textures.Add(name);
                    else if(t == ShaderPropertyType.Color || t == ShaderPropertyType.Vector) vectors.Add(name);
                    else floats.Add(name);
                }
            }
        }
    }
}
