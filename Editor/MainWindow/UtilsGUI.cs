using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    [Serializable]
    internal class UtilsGUI
    {
        internal GameObject gameObject;

        internal void Draw(AvatarUtils window)
        {
            if(gameObject == null) return;
            if(L10n.Button("Clean up Materials"))
            {
                var cleanedMaterials = MaterialCleaner.RemoveUnusedProperties(window.materialsGUI.mds.Keys);
                L10n.DisplayDialog(
                    AvatarUtils.TEXT_WINDOW_NAME,
                    "Removed unused properties on {0} materials.",
                    "OK",
                    cleanedMaterials.Count
                );
            }
            if(L10n.Button("Clean up AnimatorControllers"))
            {
                var controllers = new HashSet<RuntimeAnimatorController>(
                    gameObject.GetBuildComponents<Animator>().Select(a => a.runtimeAnimatorController)
                );

                var scaned = new HashSet<UnityEngine.Object>();
                controllers.UnionWith(gameObject.GetComponentsInChildren<MonoBehaviour>(true).SelectMany(c => ObjectHelper.GetReferenceFromObject<RuntimeAnimatorController>(scaned, c)));

                var cleanedControllers = SubAssetCleaner.RemoveUnusedSubAssets(controllers.Where(ac => ac is AnimatorController));
                L10n.DisplayDialog(
                    AvatarUtils.TEXT_WINDOW_NAME,
                    "Removed unused sub-assets in {0} AnimatorControllers.",
                    "OK",
                    cleanedControllers.Count
                );
            }
            if(L10n.Button("Remove Missing Components"))
            {
                int count = 0;
                foreach(var t in gameObject.GetComponentsInChildren<Transform>(true))
                {
                    count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                }
                L10n.DisplayDialog(
                    AvatarUtils.TEXT_WINDOW_NAME,
                    "Removed {0} missing components.",
                    "OK",
                    count
                );
            }
        }
    }
}
