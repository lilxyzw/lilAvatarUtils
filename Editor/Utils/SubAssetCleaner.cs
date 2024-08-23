using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils.Utils
{
    internal class SubAssetCleaner
    {
        internal static HashSet<Object> RemoveUnusedSubAssets(IEnumerable<Object> objs)
        {
            var cleanedAssets = new HashSet<Object>();
            foreach(var obj in objs)
            {
                if(RemoveUnusedSubAssets(obj, false)) cleanedAssets.Add(obj);
            }
            if(cleanedAssets.Count > 0) AssetDatabase.SaveAssets();
            return cleanedAssets;
        }

        internal static bool RemoveUnusedSubAssets(Object obj, bool shouldSave = true)
        {
            bool isCleaned = false;
            var path = AssetDatabase.GetAssetPath(obj);
            while(true)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(path).Where(asset => asset != null);
                var usedAssetsTemp = new HashSet<Object>();
                foreach(var asset in assets)
                {
                    var so = new SerializedObject(asset);
                    var prop = so.GetIterator();
                    while(prop.Next(true))
                    {
                        if(prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null)
                        {
                            usedAssetsTemp.Add(prop.objectReferenceValue);
                        }
                    }
                }
                bool shouldContinue = false;
                foreach(var asset in assets.Where(asset => !usedAssetsTemp.Contains(asset)))
                {
                    Debug.Log($"[AvatarUtils] Remove from {obj.name}: {asset.name}");
                    AssetDatabase.RemoveObjectFromAsset(asset);
                    shouldContinue = true;
                }
                if(!shouldContinue) break;
                isCleaned = true;
            }
            if(shouldSave) AssetDatabase.SaveAssets();
            return isCleaned;
        }
    }
}
