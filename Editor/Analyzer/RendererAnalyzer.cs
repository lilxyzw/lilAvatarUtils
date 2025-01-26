using System.Collections.Generic;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    internal class RendererAnalyzer
    {
        internal static void Analyze(
            GameObject gameObject,
            out HashSet<SkinnedMeshRenderer> smrs,
            out HashSet<(MeshRenderer,MeshFilter)> mrs,
            out HashSet<ParticleSystemRenderer> psrs
        )
        {
            smrs = new HashSet<SkinnedMeshRenderer>(gameObject.GetBuildComponents<SkinnedMeshRenderer>());
            psrs = new HashSet<ParticleSystemRenderer>(gameObject.GetBuildComponents<ParticleSystemRenderer>());
            mrs = new HashSet<(MeshRenderer,MeshFilter)>();
            foreach(var mr in gameObject.GetBuildComponents<MeshRenderer>())
            {
                var mf = mr.gameObject.GetComponent<MeshFilter>();
                mrs.Add((mr, mf));
            }
        }
    }
}
