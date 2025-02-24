﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.avatarutils
{
    [Docs(T_Title,T_Description)][DocsHowTo(T_HowTo)]
    [Serializable]
    internal class RenderersGUI : AbstractTabelGUI
    {
        internal const string T_Title = "Renderers";
        internal const string T_Description = "This is a list of all renderers included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.";
        internal const string T_HowTo = "You can check the difference in anchor overrides and root bone settings to resolve avatar lighting issues, or identify meshes with high polygon counts and material slots to help make your avatar lighter.";
        internal static readonly string[] T_TD = {T_Title, T_Description};

        public string empNames      = "";       private const int indNames      = 0;
        public string empAnchors    = "";       private const int indAnchors    = 1;
        public string empRootBones  = "";       private const int indRootBones  = 2;
        public string empBounds     = "";       private const int indBounds     = 3;
        public int    empShapes     = 100;      private const int indShapes     = 4;
        public int    empSlots      = 10;       private const int indSlots      = 5;
        public int    empPolys      = 10000;    private const int indPolys      = 6;
        public int    empVerts      = 10000;    private const int indVerts      = 7;
        public int    empQualitys   = 0;        private const int indQualitys   = 8;
        public int    empUpdates    = 0;        private const int indUpdates    = 9;
        public int    empCasts      = 0;        private const int indCasts      = 10;
        public int    empReceives   = 0;        private const int indReceives   = 11;
        public int    empLPs        = 0;        private const int indLPs        = 12;
        public int    empRPs        = 0;        private const int indRPs        = 13;
        public int    empVectors    = 0;        private const int indVectors    = 14;
        public int    empOcclusions = 0;        private const int indOcclusions = 15;

        internal HashSet<SkinnedMeshRenderer> smrs;
        internal HashSet<(MeshRenderer,MeshFilter)> mrs;
        internal HashSet<ParticleSystemRenderer> psrs;
        private int assetType = -1;

        [DocsField] private static readonly string[] L_Names      = {"Name"                 , "Object name. Clicking this will select the corresponding object in the Hierarchy window."};
        [DocsField] private static readonly string[] L_Anchors    = {"Anchor Override"      , "This is the reference point when calculating lighting. It is recommended to set it the same for all meshes in one avatar, as it can cause different brightnesses for each mesh. It is used to calculate light probes and reflection probes."};
        [DocsField] private static readonly string[] L_RootBones  = {"Root Bone"            , "The bone or transform that is the origin of the mesh. Bounds move according to this transform. It may also affect shader calculations."};
        [DocsField] private static readonly string[] L_Bounds     = {"Bounds"               , "This is the volume used to determine if a mesh is outside the range of the screen or lights. If the mesh does not fit within this range, the mesh that should be in the screen may suddenly disappear or the light may suddenly become dark, so you need to set it appropriately."};
        [DocsField] private static readonly string[] L_Shapes     = {"BlendShapes"          , "The number of BlendShapes for the mesh. The larger the number, the larger the mesh size will be."};
        [DocsField] private static readonly string[] L_Slots      = {"Materials"            , "The number of material slots for the mesh. The larger this number is, the larger the rendering load will be."};
        [DocsField] private static readonly string[] L_Polys      = {"Polys"                , "The number of polygons in the mesh. The larger this number is, the higher the load on the geometry shader and tessellation will be."};
        [DocsField] private static readonly string[] L_Verts      = {"Vertices"             , "The number of vertices in the mesh. The larger this number is, the higher the load on the vertex shader and deformation by bones and blendshapes will be."};
        [DocsField] private static readonly string[] L_Qualitys   = {"Quality"              , "This is the number of bones that can operate one vertex. The larger the number, the more calculations will be required, which may result in poorer performance. Basically, this is set to Auto, and if you want to limit it, it is recommended to change the model import settings."};
        [DocsField] private static readonly string[] L_Updates    = {"Update When Offscreen", "If this is on, bounds will be calculated even if the mesh is off screen. This setting is off by default to reduce the load."};
        [DocsField] private static readonly string[] L_Casts      = {"Cast Shadows"         , "Whether the mesh casts shadows on other objects."};
        [DocsField] private static readonly string[] L_Receives   = {"Receive Shadows"      , "Whether the mesh receives shadows from other objects."};
        [DocsField] private static readonly string[] L_LPs        = {"Light Probes"         , "How to calculate light probes."};
        [DocsField] private static readonly string[] L_RPs        = {"Reflection Probes"    , "How to calculate reflection probes."};
        [DocsField] private static readonly string[] L_Vectors    = {"Motion Vector"        , "Whether to output motion vectors, used for post-processing such as motion blur."};
        [DocsField] private static readonly string[] L_Occlusions = {"Dynamic Occlusion"    , "If the mesh is hidden by other static objects, culling will be performed and rendering will be skipped. It is usually recommended to turn it on for better performance, but turn it off if you want to use special effects such as displaying objects hidden by walls."};

        internal override void Draw()
        {
            if(IsEmptyLibs()) return;
            assetType = -1;
            base.Draw();

            GUIUtils.DrawLine();
            UpdateRects();
            var rectTotal = GetShiftedRects();
            var sumSlots = libs[indSlots].items.Where(item => item != null).Sum(item => (int)item);
            var sumPolys = libs[indPolys].items.Where(item => item != null).Sum(item => (int)item);
            var sumVerts = libs[indVerts].items.Where(item => item != null).Sum(item => (int)item);
            L10n    .LabelField(rectTotal[indNames], "Total"            );
            GUIUtils.LabelField(rectTotal[indSlots], sumSlots.ToString());
            GUIUtils.LabelField(rectTotal[indPolys], sumPolys.ToString());
            GUIUtils.LabelField(rectTotal[indVerts], sumVerts.ToString());

            empNames      = (string)libs[indNames     ].emphasize;
            empAnchors    = (string)libs[indAnchors   ].emphasize;
            empRootBones  = (string)libs[indRootBones ].emphasize;
            empBounds     = (string)libs[indBounds    ].emphasize;
            empShapes     = (int   )libs[indShapes    ].emphasize;
            empSlots      = (int   )libs[indSlots     ].emphasize;
            empPolys      = (int   )libs[indPolys     ].emphasize;
            empVerts      = (int   )libs[indVerts     ].emphasize;
            empQualitys   = (int   )libs[indQualitys  ].emphasize;
            empUpdates    = (int   )libs[indUpdates   ].emphasize;
            empCasts      = (int   )libs[indCasts     ].emphasize;
            empReceives   = (int   )libs[indReceives  ].emphasize;
            empLPs        = (int   )libs[indLPs       ].emphasize;
            empRPs        = (int   )libs[indRPs       ].emphasize;
            empVectors    = (int   )libs[indVectors   ].emphasize;
            empOcclusions = (int   )libs[indOcclusions].emphasize;
        }

        private bool OnLine(int count, bool[] emphasizes)
        {
            var assetType2 = 0;
            switch(libs[indNames].items[count])
            {
                case SkinnedMeshRenderer _   : assetType2 = 0; break;
                case MeshRenderer _          : assetType2 = 1; break;
                case ParticleSystemRenderer _: assetType2 = 2; break;
            }
            if(assetType != assetType2)
            {
                assetType = assetType2;
                var rect = new Rect(libs[0].rect.x-16, libs[0].rect.y, libs[libs.Length-1].rect.xMax - libs[0].rect.x+16, libs[0].rect.height);
                switch(assetType)
                {
                    case 0: EditorGUI.LabelField(rect, "SkinnedMeshRenderer"); break;
                    case 1: EditorGUI.LabelField(rect, "MeshRenderer"); break;
                    case 2: EditorGUI.LabelField(rect, "ParticleSystemRenderer"); break;
                }
                UpdateRects();
            }
            return true;
        }

        internal override void Set()
        {
            isModified = false;
            var qlLabs = Enum.GetNames(typeof(SkinQuality));
            var lpLabs = Enum.GetNames(typeof(LightProbeUsage));
            var rpLabs = Enum.GetNames(typeof(ReflectionProbeUsage));
            var transType = typeof(Transform);
            //                                   items               label         rect                 isEdit type        scene isMask emp            labs     empGUI empCon mainGUI
            var names      = new TableProperties(new List<object>(), L_Names     , new Rect(0,0,200,0), false, null     , false, false, empNames     , null  , null,  null,  null);
            var anchors    = new TableProperties(new List<object>(), L_Anchors   , new Rect(0,0,100,0), true , transType, true , false, empAnchors   , null  , null,  null,  null);
            var rootBones  = new TableProperties(new List<object>(), L_RootBones , new Rect(0,0,100,0), true , transType, true , false, empRootBones , null  , null,  null,  MainGUIRootBones);
            var bounds     = new TableProperties(new List<object>(), L_Bounds    , new Rect(0,0,250,0), false, null     , false, false, empBounds    , null  , null,  null,  null);
            var shapes     = new TableProperties(new List<object>(), L_Shapes    , new Rect(0,0, 40,0), false, null     , false, false, empShapes    , null  , null,  null,  null);
            var slots      = new TableProperties(new List<object>(), L_Slots     , new Rect(0,0, 40,0), false, null     , false, false, empSlots     , null  , null,  null,  null);
            var polys      = new TableProperties(new List<object>(), L_Polys     , new Rect(0,0, 50,0), false, null     , false, false, empPolys     , null  , null,  null,  null);
            var verts      = new TableProperties(new List<object>(), L_Verts     , new Rect(0,0, 50,0), false, null     , false, false, empVerts     , null  , null,  null,  null);
            var qualitys   = new TableProperties(new List<object>(), L_Qualitys  , new Rect(0,0, 50,0), true , null     , false, true , empQualitys  , qlLabs, null,  null,  null);
            var updates    = new TableProperties(new List<object>(), L_Updates   , new Rect(0,0,140,0), true , null     , false, true , empUpdates   , null  , null,  null,  null);
            var casts      = new TableProperties(new List<object>(), L_Casts     , new Rect(0,0, 90,0), true , null     , false, true , empCasts     , null  , null,  null,  null);
            var receives   = new TableProperties(new List<object>(), L_Receives  , new Rect(0,0,110,0), true , null     , false, true , empReceives  , null  , null,  null,  null);
            var lps        = new TableProperties(new List<object>(), L_LPs       , new Rect(0,0,110,0), true , null     , false, true , empLPs       , lpLabs, null,  null,  null);
            var rps        = new TableProperties(new List<object>(), L_RPs       , new Rect(0,0,110,0), true , null     , false, true , empRPs       , rpLabs, null,  null,  null);
            var vectors    = new TableProperties(new List<object>(), L_Vectors   , new Rect(0,0, 90,0), true , null     , false, true , empVectors   , null  , null,  null,  null);
            var occlusions = new TableProperties(new List<object>(), L_Occlusions, new Rect(0,0,110,0), true , null     , false, true , empOcclusions, null  , null,  null,  null);

            Sort();
            foreach(var smr in smrs)
            {
                names     .items.Add(smr);
                anchors   .items.Add(smr.probeAnchor);
                rootBones .items.Add(smr.rootBone);
                bounds    .items.Add(smr.bounds.ToString());
                shapes    .items.Add(smr.sharedMesh?.blendShapeCount);
                slots     .items.Add(smr.sharedMaterials.Length);
                polys     .items.Add(smr.sharedMesh?.triangles.Length / 3);
                verts     .items.Add(smr.sharedMesh?.vertexCount);
                qualitys  .items.Add(smr.quality);
                updates   .items.Add(smr.updateWhenOffscreen);
                casts     .items.Add(smr.shadowCastingMode);
                receives  .items.Add(smr.receiveShadows);
                lps       .items.Add(smr.lightProbeUsage);
                rps       .items.Add(smr.reflectionProbeUsage);
                vectors   .items.Add(smr.skinnedMotionVectors);
                occlusions.items.Add(smr.allowOcclusionWhenDynamic);
            }

            foreach(var mr in mrs)
            {
                names     .items.Add(mr.Item1);
                anchors   .items.Add(mr.Item1.probeAnchor);
                rootBones .items.Add(null);
                bounds    .items.Add(mr.Item1.bounds.ToString());
                shapes    .items.Add(mr.Item2.sharedMesh?.blendShapeCount);
                slots     .items.Add(mr.Item1.sharedMaterials.Length);
                polys     .items.Add(mr.Item2.sharedMesh?.triangles.Length / 3);
                verts     .items.Add(mr.Item2.sharedMesh?.vertexCount);
                qualitys  .items.Add(null);
                updates   .items.Add(null);
                casts     .items.Add(mr.Item1.shadowCastingMode);
                receives  .items.Add(mr.Item1.receiveShadows);
                lps       .items.Add(mr.Item1.lightProbeUsage);
                rps       .items.Add(mr.Item1.reflectionProbeUsage);
                vectors   .items.Add(null);
                occlusions.items.Add(mr.Item1.allowOcclusionWhenDynamic);
            }

            foreach(var psr in psrs)
            {
                names     .items.Add(psr);
                anchors   .items.Add(psr.probeAnchor);
                rootBones .items.Add(null);
                bounds    .items.Add(psr.bounds.ToString());
                shapes    .items.Add(null);
                slots     .items.Add(psr.sharedMaterials.Length);
                polys     .items.Add(null);
                verts     .items.Add(null);
                qualitys  .items.Add(null);
                updates   .items.Add(null);
                casts     .items.Add(psr.shadowCastingMode);
                receives  .items.Add(psr.receiveShadows);
                lps       .items.Add(psr.lightProbeUsage);
                rps       .items.Add(psr.reflectionProbeUsage);
                vectors   .items.Add(null);
                occlusions.items.Add(psr.allowOcclusionWhenDynamic);
            }

            libs = new []{
                names     ,
                anchors   ,
                rootBones ,
                bounds    ,
                shapes    ,
                slots     ,
                polys     ,
                verts     ,
                qualitys  ,
                updates   ,
                casts     ,
                receives  ,
                lps       ,
                rps       ,
                vectors   ,
                occlusions
            };

            lineGUIOverride = OnLine;
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indNames     : smrs = smrs.Sort(smr => smr.name                        , isDescending); break;
                case indAnchors   : smrs = smrs.Sort(smr => smr.probeAnchor.GetName()       , isDescending); break;
                case indRootBones : smrs = smrs.Sort(smr => smr.rootBone.GetName()          , isDescending); break;
                case indBounds    : smrs = smrs.Sort(smr => smr.bounds.ToString()           , isDescending); break;
                case indShapes    : smrs = smrs.Sort(smr => smr.sharedMesh?.blendShapeCount , isDescending); break;
                case indSlots     : smrs = smrs.Sort(smr => smr.sharedMaterials.Length      , isDescending); break;
                case indPolys     : smrs = smrs.Sort(smr => smr.sharedMesh?.triangles.Length, isDescending); break;
                case indVerts     : smrs = smrs.Sort(smr => smr.sharedMesh?.vertexCount     , isDescending); break;
                case indQualitys  : smrs = smrs.Sort(smr => smr.quality                     , isDescending); break;
                case indUpdates   : smrs = smrs.Sort(smr => smr.updateWhenOffscreen         , isDescending); break;
                case indCasts     : smrs = smrs.Sort(smr => smr.shadowCastingMode           , isDescending); break;
                case indReceives  : smrs = smrs.Sort(smr => smr.receiveShadows              , isDescending); break;
                case indLPs       : smrs = smrs.Sort(smr => smr.lightProbeUsage             , isDescending); break;
                case indRPs       : smrs = smrs.Sort(smr => smr.reflectionProbeUsage        , isDescending); break;
                case indVectors   : smrs = smrs.Sort(smr => smr.skinnedMotionVectors        , isDescending); break;
                case indOcclusions: smrs = smrs.Sort(smr => smr.allowOcclusionWhenDynamic   , isDescending); break;
            }
            switch(sortIndex)
            {
                case indNames     : mrs = mrs.Sort(mr => mr.Item1.name                        , isDescending); break;
                case indAnchors   : mrs = mrs.Sort(mr => mr.Item1.probeAnchor.GetName()       , isDescending); break;
                //case indRootBones : mrs = mrs.Sort(mr => mr.Item1.rootBone.GetName()          , isDescending); break;
                case indBounds    : mrs = mrs.Sort(mr => mr.Item1.bounds.ToString()           , isDescending); break;
                case indShapes    : mrs = mrs.Sort(mr => mr.Item2.sharedMesh?.blendShapeCount , isDescending); break;
                case indSlots     : mrs = mrs.Sort(mr => mr.Item1.sharedMaterials.Length      , isDescending); break;
                case indPolys     : mrs = mrs.Sort(mr => mr.Item2.sharedMesh?.triangles.Length, isDescending); break;
                case indVerts     : mrs = mrs.Sort(mr => mr.Item2.sharedMesh?.vertexCount     , isDescending); break;
                //case indQualitys  : mrs = mrs.Sort(mr => mr.Item1.quality                     , isDescending); break;
                //case indUpdates   : mrs = mrs.Sort(mr => mr.Item1.updateWhenOffscreen         , isDescending); break;
                case indCasts     : mrs = mrs.Sort(mr => mr.Item1.shadowCastingMode           , isDescending); break;
                case indReceives  : mrs = mrs.Sort(mr => mr.Item1.receiveShadows              , isDescending); break;
                case indLPs       : mrs = mrs.Sort(mr => mr.Item1.lightProbeUsage             , isDescending); break;
                case indRPs       : mrs = mrs.Sort(mr => mr.Item1.reflectionProbeUsage        , isDescending); break;
                //case indVectors   : mrs = mrs.Sort(mr => mr.Item1.skinnedMotionVectors        , isDescending); break;
                case indOcclusions: mrs = mrs.Sort(mr => mr.Item1.allowOcclusionWhenDynamic   , isDescending); break;
            }
            switch(sortIndex)
            {
                case indNames     : psrs = psrs.Sort(psr => psr.name                       , isDescending); break;
                case indAnchors   : psrs = psrs.Sort(psr => psr.probeAnchor.GetName()      , isDescending); break;
                //case indRootBones : psrs = psrs.Sort(psr => psr.rootBone.GetName()         , isDescending); break;
                case indBounds    : psrs = psrs.Sort(psr => psr.bounds.ToString()          , isDescending); break;
                //case indShapes    : psrs = psrs.Sort(psr => psr.sharedMesh.blendShapeCount , isDescending); break;
                case indSlots     : psrs = psrs.Sort(psr => psr.sharedMaterials.Length     , isDescending); break;
                //case indPolys     : psrs = psrs.Sort(psr => psr.sharedMesh.triangles.Length, isDescending); break;
                //case indVerts     : psrs = psrs.Sort(psr => psr.sharedMesh.vertexCount     , isDescending); break;
                //case indQualitys  : psrs = psrs.Sort(psr => psr.quality                    , isDescending); break;
                //case indUpdates   : psrs = psrs.Sort(psr => psr.updateWhenOffscreen        , isDescending); break;
                case indCasts     : psrs = psrs.Sort(psr => psr.shadowCastingMode          , isDescending); break;
                case indReceives  : psrs = psrs.Sort(psr => psr.receiveShadows             , isDescending); break;
                case indLPs       : psrs = psrs.Sort(psr => psr.lightProbeUsage            , isDescending); break;
                case indRPs       : psrs = psrs.Sort(psr => psr.reflectionProbeUsage       , isDescending); break;
                //case indVectors   : psrs = psrs.Sort(psr => psr.skinnedMotionVectors       , isDescending); break;
                case indOcclusions: psrs = psrs.Sort(psr => psr.allowOcclusionWhenDynamic  , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            var keys = new HashSet<Object>();
            keys.UnionWith(smrs);
            keys.UnionWith(mrs.Select(mr => mr.Item1));
            keys.UnionWith(psrs);
            SortLibs(keys.ToArray());
        }

        protected override void ApplyModification()
        {
            for(int count = 0; count < libs[indNames].items.Count; count++)
            {
                if(libs[indNames].items[count] == null) continue;
                var item = libs[indNames].items[count];
                if(item is SkinnedMeshRenderer smr)
                {
                    smr.probeAnchor               = (Transform           )libs[indAnchors   ].items[count];
                    smr.rootBone                  = (Transform           )libs[indRootBones ].items[count];
                    smr.quality                   = (SkinQuality         )libs[indQualitys  ].items[count];
                    smr.updateWhenOffscreen       = (bool                )libs[indUpdates   ].items[count];
                    smr.shadowCastingMode         = (ShadowCastingMode   )libs[indCasts     ].items[count];
                    smr.receiveShadows            = (bool                )libs[indReceives  ].items[count];
                    smr.lightProbeUsage           = (LightProbeUsage     )libs[indLPs       ].items[count];
                    smr.reflectionProbeUsage      = (ReflectionProbeUsage)libs[indRPs       ].items[count];
                    smr.skinnedMotionVectors      = (bool                )libs[indVectors   ].items[count];
                    smr.allowOcclusionWhenDynamic = (bool                )libs[indOcclusions].items[count];
                }
                else if(item is MeshRenderer mr)
                {
                    mr.probeAnchor               = (Transform           )libs[indAnchors   ].items[count];
                    mr.shadowCastingMode         = (ShadowCastingMode   )libs[indCasts     ].items[count];
                    mr.receiveShadows            = (bool                )libs[indReceives  ].items[count];
                    mr.lightProbeUsage           = (LightProbeUsage     )libs[indLPs       ].items[count];
                    mr.reflectionProbeUsage      = (ReflectionProbeUsage)libs[indRPs       ].items[count];
                    mr.allowOcclusionWhenDynamic = (bool                )libs[indOcclusions].items[count];
                }
                if(item is ParticleSystemRenderer psr)
                {
                    psr.probeAnchor               = (Transform           )libs[indAnchors   ].items[count];
                    psr.shadowCastingMode         = (ShadowCastingMode   )libs[indCasts     ].items[count];
                    psr.receiveShadows            = (bool                )libs[indReceives  ].items[count];
                    psr.lightProbeUsage           = (LightProbeUsage     )libs[indLPs       ].items[count];
                    psr.reflectionProbeUsage      = (ReflectionProbeUsage)libs[indRPs       ].items[count];
                    psr.allowOcclusionWhenDynamic = (bool                )libs[indOcclusions].items[count];
                }
            }
        }

        private bool MainGUIRootBones(int i, int count, bool emp)
        {
            return libs[indNames].items[count] is SkinnedMeshRenderer;
        }
    }
}
