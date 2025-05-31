using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.avatarutils
{
    internal static class ObjectHelper
    {
        internal static string GetName(this Object obj)
        {
            return obj ? obj.name : "";
        }

        // EditorOnly
        internal static bool IsEditorOnly(Transform obj)
        {
            if(obj.CompareTag("EditorOnly")) return true;
            if(!obj.transform.parent) return false;
            return IsEditorOnly(obj.transform.parent);
        }

        internal static bool IsEditorOnly(this GameObject obj)
        {
            return IsEditorOnly(obj.transform);
        }

        internal static bool IsEditorOnly(this Component com)
        {
            return IsEditorOnly(com.transform);
        }

        internal static IEnumerable<T> GetBuildComponents<T>(this GameObject obj) where T : Component
        {
            return obj.GetComponentsInChildren<T>(true).Where(c => !c.IsEditorOnly());
        }

        // References
        internal static void RemoveReferences(Object parent, Object target)
        {
            using var so = new SerializedObject(parent);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue == target)
                {
                    iter.objectReferenceValue = null;
                }
            }
            so.ApplyModifiedProperties();

            // Handle Material Variant references for texture removal
            # if UNITY_2022_1_OR_NEWER
            if (parent is Material material && material.parent != null && target is Texture tex)
            {
                RemoveTextureFromMaterialVariant(material, tex);
            }
            #endif
        }

        internal static void ReplaceReferences(Object parent, Object from, Object to)
        {
            using var so = new SerializedObject(parent);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue == from)
                {
                    iter.objectReferenceValue = to;
                }
            }
            so.ApplyModifiedProperties();

            // Handle Material Variant references for texture replacement
            #if UNITY_2022_1_OR_NEWER
            if (parent is Material material && material.parent != null && from is Texture fromTex && to is Texture toTex)
            {
                ReplaceTextureInMaterialVariant(material, fromTex, toTex);
            }
            #endif
        }

        #if UNITY_2022_1_OR_NEWER
        private static void RemoveTextureFromMaterialVariant(Material material, Texture tex)
        {
            ProcessMaterialVariantTexture(material, tex, null);
        }

        private static void ReplaceTextureInMaterialVariant(Material material, Texture from, Texture to)
        {
            ProcessMaterialVariantTexture(material, from, to);
        }

        private static void ProcessMaterialVariantTexture(Material material, Texture from, Texture to)
        {
            // Create a flattened material to check all properties including inherited ones
            var flattened = new Material(material);
            flattened.parent = null;

            using var flattenedSO = new SerializedObject(flattened);
            var flattenedProps = flattenedSO.FindProperty("m_SavedProperties")?.FindPropertyRelative("m_TexEnvs");

            if (flattenedProps == null)
            {
                Object.DestroyImmediate(flattened);
                return;
            }

            // Check if the texture exists in the flattened material (including inherited properties)
            for (int i = 0; i < flattenedProps.arraySize; i++)
            {
                var flattenedTexProp = flattenedProps.GetArrayElementAtIndex(i)?.FindPropertyRelative("second")?.FindPropertyRelative("m_Texture");
                if (flattenedTexProp?.objectReferenceValue != from)
                    continue;

                // Found the texture in flattened material, now set it explicitly in the variant
                var propertyName = flattenedProps.GetArrayElementAtIndex(i)?.FindPropertyRelative("first")?.stringValue;
                if (string.IsNullOrEmpty(propertyName))
                    continue;

                // Set the texture property explicitly on the variant material
                // This will override the inherited value (to=null for removal, to=newTexture for replacement)
                material.SetTexture(propertyName, to);
            }

            Object.DestroyImmediate(flattened);
        }
        #endif

        // Sort
        internal static HashSet<T> Sort<T, TP>(this HashSet<T> src, Func<T, TP> func, bool isDescending)
        {
            if(isDescending) return new HashSet<T>(src.OrderByDescending(func));
            else             return new HashSet<T>(src.OrderBy(func));
        }

        internal static Dictionary<TKey, TValue> Sort<TKey, TValue, TP>(this Dictionary<TKey, TValue> src, Func<KeyValuePair<TKey, TValue>, TP> func, bool isDescending)
        {
            if(isDescending) return src.OrderByDescending(func).ToDictionary(kv=>kv.Key,kv=>kv.Value);
            else             return src.OrderBy(func).ToDictionary(kv=>kv.Key,kv=>kv.Value);
        }
    }
}
