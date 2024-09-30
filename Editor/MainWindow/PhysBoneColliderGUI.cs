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
    internal class PhysBoneCollidersGUI : AbstractTabelGUI
    {
        public string empName   = ""; private const int indName   =  0;
        public string empRoot   = ""; private const int indRoot   =  1;
        public int    empRef    = 20;  private const int indRef   =  2;
        public int    empShape  = 0;  private const int indShape  =  3;
        public float  empRadius = 0;  private const int indRadius =  4;
        public float  empHeight = 0;  private const int indHeight =  5;
        public string empPos    = ""; private const int indPos    =  6;
        public string empRot    = ""; private const int indRot    =  7;
        public int    empInside = 0;  private const int indInside =  8;
        public int    empAsSphr = 0;  private const int indAsSphr =  9;

        internal bool[] showReferences = {false};
        internal Dictionary<VRCPhysBoneCollider, VRCPhysBone[]> pbcs = new Dictionary<VRCPhysBoneCollider, VRCPhysBone[]>();

        internal override void Draw(EditorWindow window)
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw(window);

            GUIUtils.DrawLine();
            UpdateRects();

            empName   = (string)libs[indName  ].emphasize;
            empRoot   = (string)libs[indRoot  ].emphasize;
            empRef    = (int   )libs[indRef   ].emphasize;
            empShape  = (int)   libs[indShape ].emphasize;
            empRadius = (float) libs[indRadius].emphasize;
            empHeight = (float) libs[indHeight].emphasize;
            empPos    = (string)libs[indPos   ].emphasize;
            empRot    = (string)libs[indRot   ].emphasize;
            empInside = (int)   libs[indInside].emphasize;
            empAsSphr = (int)   libs[indAsSphr].emphasize;
        }

        protected override void LineGUIEx(int count)
        {
            showReferences[count] = GUIUtils.Foldout(new Rect(libs[0].rect.x - 16, libs[0].rect.y, 16, libs[0].rect.height), showReferences[count]);
            if(showReferences[count])
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Referenced from");
                var pbc = (VRCPhysBoneCollider)libs[indName].items[count];
                var pbs = pbcs[pbc];
                foreach(var pb in pbs) GUIUtils.LabelFieldWithSelection(pb);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal override void Set()
        {
            isModified = false;
            var shapeLabs = Enum.GetNames(typeof(VRCPhysBoneColliderBase.ShapeType));

            var transType = typeof(Transform);
            //                                items               label        rect                 isEdit type       scene  isMask emp        labs       empGUI empCon mainGUI
            var names   = new TableProperties(new List<object>(), "Name"     , new Rect(0,0,200,0), false, null     , false, false, empName  , null     , null,  null,  null);
            var roots   = new TableProperties(new List<object>(), "Root"     , new Rect(0,0,100,0), true , transType, true , false, empRoot  , null     , null,  null,  null);
            var refs    = new TableProperties(new List<object>(), "Refs"     , new Rect(0,0, 50,0), false, null     , false, false, empRef   , null     , null,  null,  null);
            var shapes  = new TableProperties(new List<object>(), "Shape"    , new Rect(0,0, 70,0), true , null     , false, true , empShape , shapeLabs, null,  null,  null);
            var radiuss = new TableProperties(new List<object>(), "Radius"   , new Rect(0,0, 50,0), true , null     , false, false, empRadius, null     , null,  null,  null);
            var heights = new TableProperties(new List<object>(), "Height"   , new Rect(0,0, 50,0), true , null     , false, false, empHeight, null     , null,  null,  null);
            var poss    = new TableProperties(new List<object>(), "Position" , new Rect(0,0,120,0), false, null     , false, false, empPos   , null     , null,  null,  null);
            var rots    = new TableProperties(new List<object>(), "Rotation" , new Rect(0,0,120,0), false, null     , false, false, empRot   , null     , null,  null,  null);
            var insides = new TableProperties(new List<object>(), "Inside"   , new Rect(0,0, 50,0), true , null     , false, true , empInside, null     , null,  null,  null);
            var asSphrs = new TableProperties(new List<object>(), "As Sphere", new Rect(0,0, 60,0), true , null     , false, true , empAsSphr, null     , null,  null,  null);

            Sort();
            foreach(var pbc in pbcs)
            {
                names  .items.Add(pbc.Key                                );
                roots  .items.Add(pbc.Key.rootTransform                  );
                refs   .items.Add(pbc.Value.Length                       );
                shapes .items.Add(pbc.Key.shapeType                      );
                radiuss.items.Add(pbc.Key.radius                         );
                heights.items.Add(pbc.Key.height                         );
                poss   .items.Add(pbc.Key.position.ToString()            );
                rots   .items.Add(pbc.Key.rotation.eulerAngles.ToString());
                insides.items.Add(pbc.Key.insideBounds                   );
                asSphrs.items.Add(pbc.Key.bonesAsSpheres                 );
            }

            libs = new []{
                names  ,
                roots  ,
                refs   ,
                shapes ,
                radiuss,
                heights,
                poss   ,
                rots   ,
                insides,
                asSphrs
            };
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indName   : pbcs = pbcs.Sort(pb => pb.Key.name                   , isDescending); break;
                case indRoot   : pbcs = pbcs.Sort(pb => pb.Key.rootTransform.GetName(), isDescending); break;
                case indRef    : pbcs = pbcs.Sort(pb => pb.Value.Length               , isDescending); break;
                case indShape  : pbcs = pbcs.Sort(pb => pb.Key.shapeType              , isDescending); break;
                case indRadius : pbcs = pbcs.Sort(pb => pb.Key.radius                 , isDescending); break;
                case indHeight : pbcs = pbcs.Sort(pb => pb.Key.height                 , isDescending); break;
                case indPos    : pbcs = pbcs.Sort(pb => pb.Key.position.ToString()    , isDescending); break;
                case indRot    : pbcs = pbcs.Sort(pb => pb.Key.rotation.ToString()    , isDescending); break;
                case indInside : pbcs = pbcs.Sort(pb => pb.Key.insideBounds           , isDescending); break;
                case indAsSphr : pbcs = pbcs.Sort(pb => pb.Key.bonesAsSpheres         , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(pbcs.Keys.ToArray());
        }

        protected override void ApplyModification()
        {
            for(int count = 0; count < libs[0].items.Count; count++)
            {
                var pbc = (VRCPhysBoneCollider)libs[indName].items[count];
                pbc.rootTransform = (Transform)                        libs[indRoot  ].items[count];
                pbc.shapeType     = (VRCPhysBoneColliderBase.ShapeType)libs[indShape ].items[count];
                pbc.radius        = (float)                            libs[indRadius].items[count];
                pbc.height        = (float)                            libs[indHeight].items[count];
                pbc.insideBounds  = (bool)                             libs[indInside].items[count];
                pbc.bonesAsSpheres= (bool)                             libs[indAsSphr].items[count];
            }
        }
    }
}
#endif