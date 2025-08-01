using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarutils
{
    internal static class CommonAnalyzer
    {
        internal static Dictionary<Object, HashSet<Object>> GetObjectReferences(GameObject gameObject)
        {
            var refs = new Dictionary<Object, HashSet<Object>>();
            foreach(var o in gameObject.GetComponentsInChildren<Component>(true))
                GetReferenceFromObject(refs, null, o, null);
            return refs;
        }

        private static void GetReferenceFromObject(Dictionary<Object, HashSet<Object>> refs, HashSet<string> scannedMaterialProperties, Object obj, Object parent)
        {
            if (!obj ||
                obj is AnimatorTransitionBase ||
                obj is GameObject go && go.IsEditorOnly() ||
                obj is Component c && c.IsEditorOnly()) return;

            if(refs.ContainsKey(obj))
            {
                if(parent) refs[obj].Add(parent);
                return;
            }

            refs[obj] = new HashSet<Object>();
            if(parent) refs[obj].Add(parent);
            if(obj is GameObject ||
                // Skip - Component
                obj is Transform ||
                obj is Cloth ||
                obj is IConstraint ||
                obj is Rigidbody ||
                obj is Joint ||
                // Skip - Asset
                obj is Mesh ||
                obj is Texture ||
                obj is Shader ||
                obj is TextAsset ||
                obj.GetType() == typeof(Object)
            ) return;

            using var so = new SerializedObject(obj);
            var enterChildren = true;

            // Material
            if (obj is Material m)
            {
                scannedMaterialProperties ??= new();
                using var texs = so.FindProperty("m_SavedProperties.m_TexEnvs");
                var size = texs.arraySize;
                for (int i = 0; i < size; i++)
                {
                    var data = texs.GetArrayElementAtIndex(i);
                    var name = data.FindPropertyRelative("first").stringValue;
                    if (scannedMaterialProperties.Add(name))
                    {
                        using var tex = data.FindPropertyRelative("second.m_Texture");
                        GetReferenceFromObject(refs, scannedMaterialProperties, tex.objectReferenceValue, obj);
                    }
                }

                // 何故かSerializedObject経由でマテリアルのm_Parentが見れないので特別措置
                GetReferenceFromObject(refs, scannedMaterialProperties, m.parent, obj);
                return;
            }
            else
            {
                // 一般Object
                using var iter = so.GetIterator();
                while (iter.Next(enterChildren))
                {
                    enterChildren = iter.propertyType != SerializedPropertyType.String;
                    if (iter.propertyType == SerializedPropertyType.ObjectReference && iter.name != "m_CorrespondingSourceObject")
                    {
                        GetReferenceFromObject(refs, scannedMaterialProperties, iter.objectReferenceValue, obj);
                    }
                }
            }

        }
    }
}
