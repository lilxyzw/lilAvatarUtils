#if LIL_VRCSDK3_AVATARS
using System.Collections.Generic;
using System.Linq;
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
            out Dictionary<VRCPhysBoneCollider, VRCPhysBone[]> pbcs
        )
        {
            pbs = new HashSet<VRCPhysBone>(gameObject.GetBuildComponents<VRCPhysBone>());
            pbcs = new Dictionary<VRCPhysBoneCollider, VRCPhysBone[]>();

            foreach(var pbc in gameObject.GetBuildComponents<VRCPhysBoneCollider>())
                pbcs[pbc] = pbs.Where(p => p && p.colliders.Contains(pbc)).Distinct().ToArray();
        }
    }
}
#endif