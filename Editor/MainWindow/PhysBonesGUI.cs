#if LIL_VRCSDK3_AVATARS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.Dynamics;
using lilAvatarUtils.Utils;

namespace lilAvatarUtils.MainWindow
{
    [Serializable]
    internal class PhysBonesGUI : AbstractTabelGUI
    {
        public string empName = "";     private const int indName      =  0;
        public string empRoot = "";     private const int indRoot      =  1;
        public string empParent = "";   private const int indParent    =  2;
        public int empMCType = 0;       private const int indMCType    =  3;
        public int empBones = 32;       private const int indBones     =  4;
        public int empColliders = 8;    private const int indColliders =  5;
        public int empCollision = 64;   private const int indCollision =  6;
        public int empImType = 0;       private const int indImType    =  7;
        public int empAllow = 0;        private const int indAllow     =  8;
        public int empGrab = 0;         private const int indGrab      =  9;
        public int empPose = 0;         private const int indPose      = 10;

        internal HashSet<VRCPhysBone> pbs = new HashSet<VRCPhysBone>();
        internal Dictionary<VRCPhysBoneCollider, VRCPhysBone[]> pbcs = new Dictionary<VRCPhysBoneCollider, VRCPhysBone[]>();

        internal override void Draw(EditorWindow window)
        {
            if(IsEmptyLibs()) return;
            base.Draw(window);

            GUIUtils.DrawLine();
            UpdateRects();
            var rectTotal = GetShiftedRects();
            int sumBones = libs[indBones].items.Sum(item => (int)item);
            int sumCollisions = libs[indCollision].items.Sum(item => (int)item);
            if(labelMasks[indName     ]) GUIUtils.LabelField(rectTotal[indName     ], "Total"                 , false);
            if(labelMasks[indBones    ]) GUIUtils.LabelField(rectTotal[indBones    ], sumBones.ToString()     , false);
            if(labelMasks[indColliders]) GUIUtils.LabelField(rectTotal[indColliders], pbcs.Count.ToString()   , false);
            if(labelMasks[indCollision]) GUIUtils.LabelField(rectTotal[indCollision], sumCollisions.ToString(), false);

            empName      = (string)libs[indName     ].emphasize;
            empRoot      = (string)libs[indRoot     ].emphasize;
            empParent    = (string)libs[indParent   ].emphasize;
            empMCType    = (int)   libs[indMCType   ].emphasize;
            empBones     = (int)   libs[indBones    ].emphasize;
            empColliders = (int)   libs[indColliders].emphasize;
            empCollision = (int)   libs[indCollision].emphasize;
            empImType    = (int)   libs[indImType   ].emphasize;
            empAllow     = (int)   libs[indAllow    ].emphasize;
            empGrab      = (int)   libs[indGrab     ].emphasize;
            empPose      = (int)   libs[indPose     ].emphasize;
        }

        internal override void Set()
        {
            isModified = false;
            var mcLabs = Enum.GetNames(typeof(VRCPhysBoneBase.MultiChildType));
            var imTypeLabs = Enum.GetNames(typeof(VRCPhysBoneBase.ImmobileType));

            #if LIL_VRCSDK3_AVATARS_1_3_12_OR_NEWER
            string[] abTypeLabs = Enum.GetNames(typeof(VRCPhysBoneBase.AdvancedBool));
            #else
            string[] abTypeLabs = null;
            #endif

            var transType = typeof(Transform);
            //                                   items               label               rect                 isEdit type       scene  isMask emp           labs        empGUI empCon mainGUI
            var names      = new TableProperties(new List<object>(), "Name"            , new Rect(0,0,200,0), false, null     , false, false, empName     , null      , null,  null,  null);
            var roots      = new TableProperties(new List<object>(), "Root"            , new Rect(0,0,100,0), true , transType, true , false, empRoot     , null      , null,  null,  null);
            var parents    = new TableProperties(new List<object>(), "Parent"          , new Rect(0,0,100,0), false, null     , false, false, empParent   , null      , null,  null,  null);
            var mcTypes    = new TableProperties(new List<object>(), "Multi Child Type", new Rect(0,0,100,0), true , null     , false, true , empMCType   , mcLabs    , null,  null,  null);
            var bones      = new TableProperties(new List<object>(), "Bones"           , new Rect(0,0, 40,0), false, null     , false, false, empBones    , null      , null,  null,  null);
            var colliders  = new TableProperties(new List<object>(), "Colliders"       , new Rect(0,0, 50,0), false, null     , false, false, empColliders, null      , null,  null,  null);
            var collisions = new TableProperties(new List<object>(), "Collision"       , new Rect(0,0, 50,0), false, null     , false, false, empCollision, null      , null,  null,  null);
            var imTypes    = new TableProperties(new List<object>(), "Immobile Type"   , new Rect(0,0, 90,0), true , null     , false, true , empImType   , imTypeLabs, null,  null,  null);
            var allows     = new TableProperties(new List<object>(), "Allow Collision" , new Rect(0,0, 90,0), true , null     , false, true , empAllow    , abTypeLabs, null,  null,  null);
            var grabs      = new TableProperties(new List<object>(), "Grabbing"        , new Rect(0,0, 60,0), true , null     , false, true , empGrab     , abTypeLabs, null,  null,  null);
            var poses      = new TableProperties(new List<object>(), "Posing"          , new Rect(0,0, 40,0), true , null     , false, true , empPose     , abTypeLabs, null,  null,  null);

            Sort();
            foreach(var pb in pbs)
            {
                var root = GetRoot(pb);
                int transformsCount = GetPBTransformsCount(pb);
                int collidersCount = pb.colliders.Count;
                int collisionCount = (transformsCount - 1) * collidersCount;
                names     .items.Add(pb               );
                roots     .items.Add(pb.rootTransform );
                parents   .items.Add(root.parent      );
                mcTypes   .items.Add(pb.multiChildType);
                bones     .items.Add(transformsCount  );
                colliders .items.Add(collidersCount   );
                collisions.items.Add(collisionCount   );
                imTypes   .items.Add(pb.immobileType  );
                allows    .items.Add(pb.allowCollision);
                grabs     .items.Add(pb.allowGrabbing );
                poses     .items.Add(pb.allowPosing   );
            }

            libs = new []{
                names     ,
                roots     ,
                parents   ,
                mcTypes   ,
                bones     ,
                colliders ,
                collisions,
                imTypes   ,
                allows    ,
                grabs     ,
                poses     
            };
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indName     : pbs = pbs.Sort(pb => pb.name                                            , isDescending); break;
                case indRoot     : pbs = pbs.Sort(pb => pb.rootTransform.GetName()                         , isDescending); break;
                case indParent   : pbs = pbs.Sort(pb => GetRoot(pb).parent.GetName()                       , isDescending); break;
                case indMCType   : pbs = pbs.Sort(pb => pb.multiChildType                                  , isDescending); break;
                case indBones    : pbs = pbs.Sort(pb => GetPBTransformsCount(pb)                           , isDescending); break;
                case indColliders: pbs = pbs.Sort(pb => pb.colliders.Count                                 , isDescending); break;
                case indCollision: pbs = pbs.Sort(pb => (GetPBTransformsCount(pb) - 1) * pb.colliders.Count, isDescending); break;
                case indImType   : pbs = pbs.Sort(pb => pb.immobileType                                    , isDescending); break;
                case indAllow    : pbs = pbs.Sort(pb => pb.allowCollision                                  , isDescending); break;
                case indGrab     : pbs = pbs.Sort(pb => pb.allowGrabbing                                   , isDescending); break;
                case indPose     : pbs = pbs.Sort(pb => pb.allowPosing                                     , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(pbs.ToArray());
        }

        protected override void ApplyModification()
        {
            for(int count = 0; count < libs[0].items.Count; count++)
            {
                var pb = (VRCPhysBone)libs[indName].items[count];
                pb.rootTransform  = (Transform                     )libs[indRoot  ].items[count];
                pb.multiChildType = (VRCPhysBoneBase.MultiChildType)libs[indMCType].items[count];
                pb.immobileType   = (VRCPhysBoneBase.ImmobileType  )libs[indImType].items[count];

                #if LIL_VRCSDK3_AVATARS_1_3_12_OR_NEWER
                pb.allowCollision = (VRCPhysBoneBase.AdvancedBool  )libs[indAllow ].items[count];
                pb.allowGrabbing  = (VRCPhysBoneBase.AdvancedBool  )libs[indGrab  ].items[count];
                pb.allowPosing    = (VRCPhysBoneBase.AdvancedBool  )libs[indPose  ].items[count];
                #else
                pb.allowCollision = (bool                          )libs[indAllow ].items[count];
                pb.allowGrabbing  = (bool                          )libs[indGrab  ].items[count];
                pb.allowPosing    = (bool                          )libs[indPose  ].items[count];
                #endif
            }
        }

        private Transform GetRoot(VRCPhysBone pb)
        {
            if(pb.rootTransform != null) return pb.rootTransform;
            else                         return pb.transform;
        }

        private Dictionary<int, HashSet<Transform>> GetPBTransforms(VRCPhysBone pb, bool ignoreRoot)
        {
            var root = pb.transform;
            if(pb.rootTransform != null) root = pb.rootTransform;
            var ignores = pb.ignoreTransforms;
            var transforms = new Dictionary<int, HashSet<Transform>>
            {
                [0] = new HashSet<Transform> { root }
            };
            int layer = 0;
            while(transforms[layer].Count != 0)
            {
                layer++;
                transforms[layer] = new HashSet<Transform>();
                foreach(var t in transforms[layer-1])
                {
                    for(int i = 0; i < t.childCount; i++)
                    {
                        var child = t.GetChild(i);
                        if(!ignores.Contains(child)) transforms[layer].Add(child);
                    }
                }
            }
            if(ignoreRoot) transforms.Remove(0);
            return transforms;
        }

        private int GetPBTransformsCount(VRCPhysBone pb, bool ignoreRoot = false)
        {
            return GetPBTransforms(pb, ignoreRoot).SelectMany(p => p.Value).Count();
        }
    }
}
#endif