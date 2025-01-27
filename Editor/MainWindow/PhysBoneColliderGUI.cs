#if LIL_VRCSDK3_AVATARS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.Dynamics;

namespace jp.lilxyzw.avatarutils
{
    [Docs(T_Title,T_Description)][DocsHowTo(T_HowTo)]
    [Serializable]
    internal class PhysBoneCollidersGUI : AbstractTabelGUI
    {
        internal const string T_Title = "PBColliders";
        internal const string T_Description = "This is a list of all PhysBone Colliders included in the avatar. Some properties can be edited, and the changed properties will be applied all at once by pressing the Apply button in the upper left.";
        internal const string T_HowTo = "This helps to identify the source of collider references and eliminate unnecessary colliders that are not referenced anywhere.";
        internal static readonly string[] T_TD = {T_Title, T_Description};

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
        internal HashSet<VRCPhysBoneCollider> pbcs;

        [DocsField] private static readonly string[] L_Name   = {"Name"          , "Object name. Clicking this will select the corresponding object in the Hierarchy window."};
        [DocsField] private static readonly string[] L_Root   = {"Root Transform", "The Transform used to calculate the collider position."};
        [DocsField] private static readonly string[] L_Ref    = {"References"    , "The number of PhysBones referencing this collider."};
        [DocsField] private static readonly string[] L_Shape  = {"Shape"         , "The shape of the collider."};
        [DocsField] private static readonly string[] L_Radius = {"Radius"        , "The radius of the collider."};
        [DocsField] private static readonly string[] L_Height = {"Height"        , "The height of the collider."};
        [DocsField] private static readonly string[] L_Pos    = {"Position"      , "The offset of the collider's position from the root transform."};
        [DocsField] private static readonly string[] L_Rot    = {"Rotation"      , "The amount of offset of the collider's rotation from the root bone."};
        [DocsField] private static readonly string[] L_Inside = {"Inside"        , "Turning this on will act to push the PhysBone inside the collider."};
        [DocsField] private static readonly string[] L_AsSphr = {"As Sphere"     , "When this is turned on, the shape of the collision detection for the PhysBone itself will be calculated as a sphere instead of a capsule."};

        internal override void Draw()
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw();

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
                L10n.LabelField(L_ReferencedFrom);
                ReferencesGUI((VRCPhysBoneCollider)libs[indName].items[count]);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal override void Set()
        {
            isModified = false;
            var shapeLabs = Enum.GetNames(typeof(VRCPhysBoneColliderBase.ShapeType));

            var transType = typeof(Transform);
            //                                items               label     rect                 isEdit type       scene  isMask emp        labs       empGUI empCon mainGUI
            var names   = new TableProperties(new List<object>(), L_Name  , new Rect(0,0,200,0), false, null     , false, false, empName  , null     , null,  null,  null);
            var roots   = new TableProperties(new List<object>(), L_Root  , new Rect(0,0,100,0), true , transType, true , false, empRoot  , null     , null,  null,  null);
            var refs    = new TableProperties(new List<object>(), L_Ref   , new Rect(0,0, 50,0), false, null     , false, false, empRef   , null     , null,  null,  null);
            var shapes  = new TableProperties(new List<object>(), L_Shape , new Rect(0,0, 70,0), true , null     , false, true , empShape , shapeLabs, null,  null,  null);
            var radiuss = new TableProperties(new List<object>(), L_Radius, new Rect(0,0, 50,0), true , null     , false, false, empRadius, null     , null,  null,  null);
            var heights = new TableProperties(new List<object>(), L_Height, new Rect(0,0, 50,0), true , null     , false, false, empHeight, null     , null,  null,  null);
            var poss    = new TableProperties(new List<object>(), L_Pos   , new Rect(0,0,120,0), false, null     , false, false, empPos   , null     , null,  null,  null);
            var rots    = new TableProperties(new List<object>(), L_Rot   , new Rect(0,0,120,0), false, null     , false, false, empRot   , null     , null,  null,  null);
            var insides = new TableProperties(new List<object>(), L_Inside, new Rect(0,0, 50,0), true , null     , false, true , empInside, null     , null,  null,  null);
            var asSphrs = new TableProperties(new List<object>(), L_AsSphr, new Rect(0,0, 60,0), true , null     , false, true , empAsSphr, null     , null,  null,  null);

            Sort();
            foreach(var pbc in pbcs)
            {
                names  .items.Add(pbc                                );
                roots  .items.Add(pbc.rootTransform                  );
                refs   .items.Add(m_window.refs[pbc].Count           );
                shapes .items.Add(pbc.shapeType                      );
                radiuss.items.Add(pbc.radius                         );
                heights.items.Add(pbc.height                         );
                poss   .items.Add(pbc.position.ToString()            );
                rots   .items.Add(pbc.rotation.eulerAngles.ToString());
                insides.items.Add(pbc.insideBounds                   );
                asSphrs.items.Add(pbc.bonesAsSpheres                 );
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
                case indName   : pbcs = pbcs.Sort(pb => pb.name                   , isDescending); break;
                case indRoot   : pbcs = pbcs.Sort(pb => pb.rootTransform.GetName(), isDescending); break;
                case indRef    : pbcs = pbcs.Sort(pb => m_window.refs[pb].Count   , isDescending); break;
                case indShape  : pbcs = pbcs.Sort(pb => pb.shapeType              , isDescending); break;
                case indRadius : pbcs = pbcs.Sort(pb => pb.radius                 , isDescending); break;
                case indHeight : pbcs = pbcs.Sort(pb => pb.height                 , isDescending); break;
                case indPos    : pbcs = pbcs.Sort(pb => pb.position.ToString()    , isDescending); break;
                case indRot    : pbcs = pbcs.Sort(pb => pb.rotation.ToString()    , isDescending); break;
                case indInside : pbcs = pbcs.Sort(pb => pb.insideBounds           , isDescending); break;
                case indAsSphr : pbcs = pbcs.Sort(pb => pb.bonesAsSpheres         , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(pbcs.ToArray());
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