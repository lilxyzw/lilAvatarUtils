#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace lilAvatarUtils.Utils
{
    internal static class ObjectHelper
    {
        internal static string GetName(this Object obj)
        {
            if(obj == null) return "";
            else            return obj.name;
        }

        // EditorOnly
        internal static bool IsEditorOnly(Transform obj)
        {
            if(obj.tag == "EditorOnly") return true;
            if(obj.transform.parent == null) return false;
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

        internal static IEnumerable<T> SelectNonEditorOnly<T>(this IEnumerable<T> objs)
        {
            return objs.Where(c => !(c as GameObject).IsEditorOnly());
        }

        internal static IEnumerable<T> GetBuildComponents<T>(this GameObject obj)
        {
            return obj.GetComponentsInChildren<T>(true).Where(c => !(c as Component).IsEditorOnly());
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
#endif