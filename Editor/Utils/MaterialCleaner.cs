using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
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
                    isCleaned |= DeleteUnusedKeywords(so.FindProperty("m_ValidKeywords"), shaderKeywords);
                    isCleaned |= DeleteUnusedKeywords(so.FindProperty("m_InvalidKeywords"), shaderKeywords);
                }
            }

            if(isCleaned)
            {
                so.ApplyModifiedProperties();
                if(isCleaned) Debug.Log($"[AvatarUtils] Clean up {material.name}", material);
                cleanedMaterials.Add(material);
            }

            if(material.parent != null)
            {
                RemoveUnusedProperties(material.parent, cleanedMaterials);
            }
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

        // Get keywords from shader
        private static HashSet<string> GetKeywords(Shader shader)
        {
            return new HashSet<string>(shader.keywordSpace.keywordNames);
        }
    }
}
