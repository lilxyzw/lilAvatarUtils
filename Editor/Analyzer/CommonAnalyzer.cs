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
                GetReferenceFromObject(refs, o, null);
            return refs;
        }

        private static void GetReferenceFromObject(Dictionary<Object, HashSet<Object>> refs, Object obj, Object parent)
        {
            if(!obj ||
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
            using var iter = so.GetIterator();
            var enterChildren = true;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.name != "m_CorrespondingSourceObject")
                {
                    GetReferenceFromObject(refs, iter.objectReferenceValue, obj);
                }
            }

            #if UNITY_2022_1_OR_NEWER
            // Handle Material Variant references
            if(obj is Material material && material.parent != null)
            {
                GetMaterialVariantReferences(refs, material, obj);
            }
            #endif
        }

        #if UNITY_2022_1_OR_NEWER
        private static void GetMaterialVariantReferences(Dictionary<Object, HashSet<Object>> refs, Material material, Object parent)
        {
            // Create a flattened material to get all properties including those from parent
            var flattened = new Material(material);
            flattened.parent = null;

            using var so = new SerializedObject(flattened);
            var props = so.FindProperty("m_SavedProperties")?.FindPropertyRelative("m_TexEnvs");
            if (props != null)
            {
                for (int i = 0; i < props.arraySize; i++)
                {
                    var texprop = props.GetArrayElementAtIndex(i)?.FindPropertyRelative("second")?.FindPropertyRelative("m_Texture");
                    if (texprop?.objectReferenceValue is Texture tex)
                    {
                        GetReferenceFromObject(refs, tex, parent);
                    }
                }
            }

            Object.DestroyImmediate(flattened);
        }
        #endif
    }
}
