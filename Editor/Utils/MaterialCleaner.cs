using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils.Utils
{
    internal class MaterialCleaner
    {
        internal static HashSet<Material> RemoveUnusedProperties(IEnumerable<Material> materials)
        {
            var cleanedMaterials = new HashSet<Material>();
            foreach(var material in materials)
            {
                RemoveUnusedProperties(material, cleanedMaterials);
            }
            return cleanedMaterials;
        }

        private static void RemoveUnusedProperties(Material material, HashSet<Material> cleanedMaterials)
        {
            // https://light11.hatenadiary.com/entry/2018/12/04/224253
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");
            bool isCleaned = false;
            isCleaned |= DeleteUnused(savedProps.FindPropertyRelative("m_TexEnvs"), material);
            isCleaned |= DeleteUnused(savedProps.FindPropertyRelative("m_Floats"), material);
            isCleaned |= DeleteUnused(savedProps.FindPropertyRelative("m_Colors"), material);

            if(material.shader != null)
            {
                var shaderKeywords = GetKeywords(material.shader);
                if(shaderKeywords != null)
                {
                    #if UNITY_2021_2_OR_NEWER
                    isCleaned |= DeleteUnusedKeywords(so.FindProperty("m_ValidKeywords"), shaderKeywords);
                    isCleaned |= DeleteUnusedKeywords(so.FindProperty("m_InvalidKeywords"), shaderKeywords);
                    #else
                    isCleaned |= DeleteUnusedKeywords(so.FindProperty("m_ShaderKeywords"), shaderKeywords);
                    #endif
                }
            }

            if(isCleaned)
            {
                so.ApplyModifiedProperties();
                if(isCleaned) Debug.Log($"[AvatarUtils] Clean up {material.name}", material);
                cleanedMaterials.Add(material);
            }

            #if UNITY_2022_1_OR_NEWER
            if(material.parent != null)
            {
                RemoveUnusedProperties(material.parent, cleanedMaterials);
            }
            #endif
        }

        private static bool DeleteUnused(SerializedProperty props, Material material)
        {
            bool isCleaned = false;
            for(int i = props.arraySize - 1; i >= 0; i--)
            {
                if(!material.HasProperty(props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue))
                {
                    props.DeleteArrayElementAtIndex(i);
                    isCleaned = true;
                }
            }
            return isCleaned;
        }

        #if UNITY_2021_2_OR_NEWER
        private static bool DeleteUnusedKeywords(SerializedProperty props, HashSet<string> shaderKeywords)
        {
            bool isCleaned = false;
            for(int i = props.arraySize - 1; i >= 0; i--)
            {
                if(!shaderKeywords.Contains(props.GetArrayElementAtIndex(i).stringValue))
                {
                    props.DeleteArrayElementAtIndex(i);
                    isCleaned = true;
                }
            }
            return isCleaned;
        }
        #else
        private static bool DeleteUnusedKeywords(SerializedProperty props, HashSet<string> shaderKeywords)
        {
            var newKeywords = string.Join(" ", props.stringValue.Split(' ').Where(k => shaderKeywords.Contains(k)));
            if(props.stringValue != newKeywords)
            {
                props.stringValue = newKeywords;
                return true;
            }
            return false;
        }
        #endif

        // Get keywords from shader
        private static HashSet<string> GetKeywords(Shader shader)
        {
            #if UNITY_2021_2_OR_NEWER
            return new HashSet<string>(shader.keywordSpace.keywordNames);
            #else
            try
            {
                var methodGlobal = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
                var methodLocal = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
                var keywords = new HashSet<string>((string[])methodGlobal.Invoke(null, new[]{shader}));
                keywords.UnionWith((string[])methodLocal.Invoke(null, new[]{shader}));
                return keywords;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            #endif
        }
    }
}
