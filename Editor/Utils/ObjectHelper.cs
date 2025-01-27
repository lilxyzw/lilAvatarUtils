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
        }

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
