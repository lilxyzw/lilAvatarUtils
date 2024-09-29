using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace lilAvatarUtils.Analyzer
{
    public class AnimationAnalyzer
    {
        internal static void Analyze(Dictionary<AnimationClip, AnimationClipData> acds)
        {
            foreach(var kv in acds)
            {
                var clip = kv.Key;
                var clipData = kv.Value;
                foreach(var kv2 in clipData.ads)
                {
                    if(kv2.Key is not AnimatorController ac) continue;
                    foreach(var layer in ac.layers)
                    {
                        kv2.Value.states.UnionWith(AnalyzeStateMachine(clip, layer.stateMachine).Select(s => (s,layer)));
                    }
                }
                var bindings = AnimationUtility.GetCurveBindings(clip);
                foreach(var binding in AnimationUtility.GetCurveBindings(clip))
                    CheckBindingType(binding, clipData);
                foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    CheckBindingType(binding, clipData);
                clipData.hasHumanoid = clip.humanMotion;
            }
        }

        private static void CheckBindingType(EditorCurveBinding binding, AnimationClipData clipData)
        {
            if(binding.type == typeof(Animator))
            {
            }
            else if(binding.type == typeof(SkinnedMeshRenderer))
            {
                if(binding.propertyName.StartsWith("blendShape.")) clipData.hasBlendShape = true;
                else if(binding.propertyName.StartsWith("m_Materials.Array.data["))  clipData.hasMaterialReplace = true;
                else if(binding.propertyName.StartsWith("material."))  clipData.hasMaterialReplace = true;
            }
            else if(binding.type == typeof(GameObject) && binding.propertyName.StartsWith("m_IsActive"))
            {
                clipData.hasToggleActive = true;
            }
            else if(binding.type.IsSubclassOf(typeof(Component)) && binding.propertyName.StartsWith("m_Enabled"))
            {
                clipData.hasToggleEnabled = true;
            }
            else if(binding.type == typeof(Transform))
            {
                clipData.hasTransform = true;
            }
            else
            {
                clipData.hasOther = true;
            }
        }

        private static IEnumerable<AnimatorState> AnalyzeStateMachine(AnimationClip clip, AnimatorStateMachine machine)
        {
            var states = machine.stateMachines.SelectMany(s => AnalyzeStateMachine(clip, s.stateMachine));
            return states.Union(machine.states.Select(s => s.state).Where(s => s.motion == clip));
        }
    }
}
