#if LIL_VRCSDK3_AVATARS
using System.Collections.Generic;
using lilAvatarUtils.Utils;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace lilAvatarUtils.Analyzer
{
    internal class PhysBonesAnalyzer
    {
        internal static void Analyze(
            GameObject gameObject,
            out HashSet<VRCPhysBone> pbs,
            out HashSet<VRCPhysBoneCollider> pbcs
        )
        {
            pbs = new HashSet<VRCPhysBone>(gameObject.GetBuildComponents<VRCPhysBone>());
            pbcs = new HashSet<VRCPhysBoneCollider>(gameObject.GetBuildComponents<VRCPhysBoneCollider>());
        }
    }
}
#endif