using System;
using System.Collections.Generic;
using System.Linq;
using lilAvatarUtils.Analyzer;
using lilAvatarUtils.Utils;
using UnityEditor;
using UnityEngine;

namespace lilAvatarUtils.MainWindow
{
    [Serializable]
    internal class TexturesGUI : AbstractTabelGUI
    {
        public string empNames   = "";      private const int indNames   =  0;
        public string empReps    = "";      private const int indReps    =  1;
        public int    empTypes   = 0;       private const int indTypes   =  2;
        public float  empVrams   = 10f;     private const int indVrams   =  3;
        public int    empRess    = 4096;    private const int indRess    =  4;
        public int    empResMaxs = 4096;    private const int indResMaxs =  5;
        public int    empComps   = 0;       private const int indComps   =  6;
        public string empFormats = "";      private const int indFormats =  7;
        public int    empCrunchs = 0;       private const int indCrunchs =  8;
        public int    empCompQs  = 100;     private const int indCompQs  =  9;
        public int    empSrgbs   = 0;       private const int indSrgbs   = 10;
        public int    empASrcs   = 0;       private const int indASrcs   = 11;
        public int    empAlphas  = 0;       private const int indAlphas  = 12;
        public int    empMips    = 0;       private const int indMips    = 13;
        public int    empStreams = 0;       private const int indStreams = 14;
        public int    empReads   = 0;       private const int indReads   = 15;

        internal bool[] showReferences = {false};
        internal Dictionary<Texture, TextureData> tds = new Dictionary<Texture, TextureData>();

        internal override void Draw(EditorWindow window)
        {
            if(IsEmptyLibs()) return;

            if(showReferences.Length != libs[0].items.Count) showReferences = Enumerable.Repeat(false, libs[0].items.Count).ToArray();
            base.Draw(window);

            GUIUtils.DrawLine();
            UpdateRects();
            var rectTotal = GetShiftedRects();
            long sumVRAM = libs[indVrams].items.Sum(item => (long)item);
            if(labelMasks[indNames]) GUIUtils.LabelField(rectTotal[indNames], "Total"                           , false);
            if(labelMasks[indVrams]) GUIUtils.LabelField(rectTotal[indVrams], EditorUtility.FormatBytes(sumVRAM), false);

            empNames   = (string)libs[indNames  ].emphasize;
            //empReps    = (string)libs[indReps   ].emphasize;
            empTypes   = (int   )libs[indTypes  ].emphasize;
            empVrams   = (float )libs[indVrams  ].emphasize;
            empRess    = (int   )libs[indRess   ].emphasize;
            empResMaxs = (int   )libs[indResMaxs].emphasize;
            empComps   = (int   )libs[indComps  ].emphasize;
            empFormats = (string)libs[indFormats].emphasize;
            empCrunchs = (int   )libs[indCrunchs].emphasize;
            empCompQs  = (int   )libs[indCompQs ].emphasize;
            empSrgbs   = (int   )libs[indSrgbs  ].emphasize;
            empASrcs   = (int   )libs[indASrcs  ].emphasize;
            empAlphas  = (int   )libs[indAlphas ].emphasize;
            empMips    = (int   )libs[indMips   ].emphasize;
            empStreams = (int   )libs[indStreams].emphasize;
            empReads   = (int   )libs[indReads  ].emphasize;
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
                var tex = (Texture)libs[indNames].items[count];
                var td = tds[tex];
                foreach(KeyValuePair<Material, MaterialData> md in td.mds)
                {
                    GUIUtils.LabelFieldWithSelection(md.Key);
                    EditorGUI.indentLevel++;
                    if(md.Value.gameObjects != null)
                    {
                        foreach(GameObject obj in md.Value.gameObjects)
                        {
                            GUIUtils.LabelFieldWithSelection(obj);
                        }
                    }
                    if(md.Value.acds != null)
                    {
                        foreach(KeyValuePair<AnimationClip, AnimationClipData> acd in md.Value.acds)
                        {
                            GUIUtils.LabelFieldWithSelection(acd.Key);
                            EditorGUI.indentLevel++;
                            foreach(KeyValuePair<RuntimeAnimatorController, AnimatorData> ad in acd.Value.ads)
                            {
                                GUIUtils.LabelFieldWithSelection(ad.Key);
                                EditorGUI.indentLevel++;
                                foreach(GameObject obj in ad.Value.gameObjects)
                                {
                                    GUIUtils.LabelFieldWithSelection(obj);
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                }

                if(GUIUtils.UnchangeButton("Remove references") && EditorUtility.DisplayDialog("AvatarUtils", "Are you sure you want to remove it?", "Yes", "Cancel"))
                {
                    foreach(KeyValuePair<Material, MaterialData> md in td.mds)
                    {
                        if(md.Key == null) continue;
                        RemoveTextureReference(md.Key, tex);
                    }
                    TextureAnalyzer.Analyze(gameObject, out tds);
                    Set();
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private void RemoveTextureReference(Material m, Texture tex)
        {
            var so = new SerializedObject(m);
            so.Update();
            var props = so.FindProperty("m_SavedProperties").FindPropertyRelative("m_TexEnvs");
            for(int i = 0; i < props.arraySize; i++)
            {
                var texprop = props.GetArrayElementAtIndex(i).FindPropertyRelative("second").FindPropertyRelative("m_Texture");
                if(texprop.objectReferenceValue == tex)
                {
                    texprop.objectReferenceValue = null;
                }
            }
            so.ApplyModifiedProperties();

            #if UNITY_2022_1_OR_NEWER
            if(m.parent != null)
            {
                RemoveTextureReference(m.parent, tex);
            }
            #endif
        }

        internal override void Set()
        {
            isModified = false;
            var typeTex = typeof(Texture);
            var typeLabs = Enum.GetNames(typeof(TextureType));
            var asrcLabs = Enum.GetNames(typeof(TextureImporterAlphaSource));
            var maxLabs = new[]{"32","64","128","256","512","1024","2048","4096","8192"};
            var compLabs = new[]{"None","Low Quality","Normal Quality","High Quality"};

            //                                items               label                    rect                 isEdit type  scene  isMask emp         labs      empGUI      empCon      mainGUI
            var names   = new TableProperties(new List<object>(), "Name"                 , new Rect(0,0,200,0), false, null, false, false, empNames  , null    , null      , null      , null);
            var reps    = new TableProperties(new List<object>(), "Replace"              , new Rect(0,0,200,0), true , typeTex, false, false, null   , null    , null      , EmpConReps, null);
            var types   = new TableProperties(new List<object>(), "Type"                 , new Rect(0,0,100,0), false, null, false, true , empTypes  , typeLabs, null      , null      , null);
            var vrams   = new TableProperties(new List<object>(), "VRAM Size"            , new Rect(0,0, 70,0), false, null, false, false, empVrams  , null    , EmpGUIVRAM, EmpConVRAM, MainGUIVRAM);
            var ress    = new TableProperties(new List<object>(), "Resolution"           , new Rect(0,0, 80,0), false, null, false, false, empRess   , null    , null      , EmpConRes , MainGUIRes);
            var resMaxs = new TableProperties(new List<object>(), "Max Resolution"       , new Rect(0,0,100,0), true , null, false, false, empResMaxs, maxLabs , null      , null      , MainGUIResMax);
            var comps   = new TableProperties(new List<object>(), "Compression"          , new Rect(0,0, 90,0), true , null, false, true , empComps  , compLabs, null      , EmpConComp, MainGUIComp);
            var formats = new TableProperties(new List<object>(), "Format"               , new Rect(0,0,110,0), false, null, false, false, empFormats, null    , null      , EmpConForm, MainGUIForm);
            var crunchs = new TableProperties(new List<object>(), "Crunch Compression"   , new Rect(0,0,120,0), true , null, false, true , empCrunchs, null    , null      , null      , MainGUICrunch);
            var compQs  = new TableProperties(new List<object>(), "Compression Quality"  , new Rect(0,0,130,0), true , null, false, false, empCompQs , null    , null      , null      , MainGUICompQ);
            var srgbs   = new TableProperties(new List<object>(), "sRGB"                 , new Rect(0,0, 50,0), true , null, false, true , empSrgbs  , null    , null      , null      , null);
            var asrcs   = new TableProperties(new List<object>(), "Alpha Source"         , new Rect(0,0,130,0), true , null, false, true , empASrcs  , asrcLabs, null      , null      , null);
            var alphas  = new TableProperties(new List<object>(), "Alpha Is Transparency", new Rect(0,0,130,0), true , null, false, true , empAlphas , null    , null      , null      , MainGUIAlpha);
            var mips    = new TableProperties(new List<object>(), "MipMap"               , new Rect(0,0, 50,0), true , null, false, true , empMips   , null    , null      , null      , null);
            var streams = new TableProperties(new List<object>(), "Streaming MipMaps"    , new Rect(0,0,120,0), true , null, false, true , empStreams, null    , EmpGUISM  , EmpConSM  , null);
            var reads   = new TableProperties(new List<object>(), "Read/Write"           , new Rect(0,0, 70,0), true , null, false, true , empReads  , null    , null      , null      , null);

            Sort();
            foreach(var td in tds)
            {
                switch(td.Key)
                {
                    case Texture2D o          : formats.items.Add(o.format); break;
                    case Cubemap o            : formats.items.Add(o.format); break;
                    case Texture3D o          : formats.items.Add(o.format); break;
                    case Texture2DArray o     : formats.items.Add(o.format); break;
                    case CubemapArray o       : formats.items.Add(o.format); break;
                    case CustomRenderTexture o: formats.items.Add(o.format); break;
                    case RenderTexture o      : formats.items.Add(o.format); break;
                    case Texture _            : formats.items.Add(null); break;
                }
                names.items.Add(td.Key);
                reps.items.Add(td.Key);
                types.items.Add(td.Value.type);
                vrams.items.Add(td.Value.vramSize);
                ress.items.Add((td.Key.width, td.Key.height));
                string path = AssetDatabase.GetAssetPath(td.Key);
                var textureImporter = AssetImporter.GetAtPath(path);

                if(textureImporter is TextureImporter)
                {
                    resMaxs.items.Add(td.Value.maxTextureSize);
                    comps.items.Add(td.Value.textureCompression);
                    crunchs.items.Add(td.Value.crunchedCompression);
                    compQs.items.Add(td.Value.compressionQuality);
                    srgbs.items.Add(td.Value.sRGBTexture);
                    asrcs.items.Add(td.Value.alphaSource);
                    alphas.items.Add(td.Value.alphaIsTransparency);
                    mips.items.Add(td.Value.mipmapEnabled);
                    streams.items.Add(td.Value.streamingMipmaps);
                    reads.items.Add(td.Value.isReadable);
                }
                else
                {
                    resMaxs.items.Add(null);
                    comps.items.Add(null); // Used in OnLine()
                    crunchs.items.Add(null);
                    compQs.items.Add(null);
                    srgbs.items.Add(null);
                    mips.items.Add(td.Key.mipmapCount > 1);
                    reads.items.Add(td.Key.isReadable);
                    asrcs.items.Add(null);
                    switch(td.Key)
                    {
                        case Texture2D o: alphas.items.Add(o.alphaIsTransparency); streams.items.Add(o.streamingMipmaps); break;
                        case Cubemap o  : alphas.items.Add(null);                  streams.items.Add(o.streamingMipmaps); break;
                        case Texture _  : alphas.items.Add(null);                  streams.items.Add(null);               break;
                    }
                }
            }

            libs = new []{
                names  ,
                reps   ,
                types  ,
                vrams  ,
                ress   ,
                resMaxs,
                comps  ,
                formats,
                crunchs,
                compQs ,
                srgbs  ,
                asrcs  ,
                alphas ,
                mips   ,
                streams,
                reads  
            };

            lineGUIOverride = (count, emp) =>
            {
                GUI.enabled = libs[indComps].items[count] != null;
                return true;
            };
        }

        protected override void Sort()
        {
            switch(sortIndex)
            {
                case indNames  : tds = tds.Sort(td => td.Key.name                 , isDescending); break;
                case indReps   : tds = tds.Sort(td => td.Key.name                 , isDescending); break;
                case indTypes  : tds = tds.Sort(td => td.Value.type.ToString()    , isDescending); break;
                case indVrams  : tds = tds.Sort(td => td.Value.vramSize           , isDescending); break;
                case indRess   : tds = tds.Sort(td => td.Key.width * td.Key.height, isDescending); break;
                case indResMaxs: tds = tds.Sort(td => td.Value.maxTextureSize     , isDescending); break;
                case indComps  : tds = tds.Sort(td => td.Value.textureCompression , isDescending); break;
                case indFormats: tds = tds.Sort(td => td.Value.format             , isDescending); break;
                case indCrunchs: tds = tds.Sort(td => td.Value.crunchedCompression, isDescending); break;
                case indCompQs : tds = tds.Sort(td => td.Value.compressionQuality , isDescending); break;
                case indSrgbs  : tds = tds.Sort(td => td.Value.sRGBTexture        , isDescending); break;
                case indASrcs  : tds = tds.Sort(td => td.Value.alphaSource        , isDescending); break;
                case indAlphas : tds = tds.Sort(td => td.Value.alphaIsTransparency, isDescending); break;
                case indMips   : tds = tds.Sort(td => td.Value.mipmapEnabled      , isDescending); break;
                case indStreams: tds = tds.Sort(td => td.Value.streamingMipmaps   , isDescending); break;
                case indReads  : tds = tds.Sort(td => td.Value.isReadable         , isDescending); break;
            }
        }

        protected override void SortLibs()
        {
            SortLibs(tds.Keys.ToArray());
        }

        protected override void ApplyModification()
        {
            for(int count = 0; count < libs[indNames].items.Count; count++)
            {
                var tex = libs[indNames].items[count] as Texture;
                if(!tex) continue;
                string path = AssetDatabase.GetAssetPath(tex);
                var textureImporter = AssetImporter.GetAtPath(path);
                if(textureImporter is TextureImporter ti)
                {
                    bool isChanged = false;
                    if(ti.maxTextureSize      != (int                       )libs[indResMaxs].items[count]){ti.maxTextureSize      = (int                       )libs[indResMaxs].items[count]; isChanged = true;}
                    if(ti.textureCompression  != (TextureImporterCompression)libs[indComps  ].items[count]){ti.textureCompression  = (TextureImporterCompression)libs[indComps  ].items[count]; isChanged = true;}
                    if(ti.crunchedCompression != (bool                      )libs[indCrunchs].items[count]){ti.crunchedCompression = (bool                      )libs[indCrunchs].items[count]; isChanged = true;}
                    if(ti.compressionQuality  != (int                       )libs[indCompQs ].items[count]){ti.compressionQuality  = (int                       )libs[indCompQs ].items[count]; isChanged = true;}
                    if(ti.sRGBTexture         != (bool                      )libs[indSrgbs  ].items[count]){ti.sRGBTexture         = (bool                      )libs[indSrgbs  ].items[count]; isChanged = true;}
                    if(ti.alphaSource         != (TextureImporterAlphaSource)libs[indASrcs  ].items[count]){ti.alphaSource         = (TextureImporterAlphaSource)libs[indASrcs  ].items[count]; isChanged = true;}
                    if(ti.alphaIsTransparency != (bool                      )libs[indAlphas ].items[count]){ti.alphaIsTransparency = (bool                      )libs[indAlphas ].items[count]; isChanged = true;}
                    if(ti.mipmapEnabled       != (bool                      )libs[indMips   ].items[count]){ti.mipmapEnabled       = (bool                      )libs[indMips   ].items[count]; isChanged = true;}
                    if(ti.streamingMipmaps    != (bool                      )libs[indStreams].items[count]){ti.streamingMipmaps    = (bool                      )libs[indStreams].items[count]; isChanged = true;}
                    if(ti.isReadable          != (bool                      )libs[indReads  ].items[count]){ti.isReadable          = (bool                      )libs[indReads  ].items[count]; isChanged = true;}
                    if(isChanged)
                    {
                        ti.SaveAndReimport();
                    }
                }

                var rep = libs[indReps].items[count] as Texture;
                if(tex != rep)
                {
                    var td = tds[tex];
                    foreach(var material in td.mds.Keys)
                    using(var so = new SerializedObject(material))
                    using(var iter = so.FindProperty("m_SavedProperties").FindPropertyRelative("m_TexEnvs"))
                    using(var end = iter.Copy())
                    {
                        end.Next(false);
                        var enterChildren = true;
                        while(iter.Next(enterChildren) && !SerializedProperty.EqualContents(iter, end))
                        {
                            enterChildren = iter.propertyType != SerializedPropertyType.String;
                            if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue && iter.objectReferenceValue == tex)
                            {
                                iter.objectReferenceValue = rep;
                            }
                        }
                        so.ApplyModifiedProperties();
                    }
                }
            }
        }

        private object EmpGUIVRAM(int i, Rect rect)
        {
            var rectEmpVRAM = new Rect(rect.x, rect.y, rect.width-24, rect.height);
            var rectEmpVRAMMB = new Rect(rectEmpVRAM.xMax, rect.y, 24, rect.height);
            EditorGUI.LabelField(rectEmpVRAMMB, " MB");
            return EditorGUI.FloatField(rectEmpVRAM, (float)libs[i].emphasize);
        }

        private object EmpGUISM(int i, Rect rect)
        {
            return EditorGUI.MaskField(rect, (int)libs[i].emphasize, new[]{"False","True","!= MipMap"});
        }

        private bool EmpConVRAM(int i, int count)
        {
            var th = (float)libs[i].emphasize;
            var size = (long)libs[i].items[count];
            return size > (th * 1024 * 1024);
        }

        private bool EmpConRes(int i, int count)
        {
            var pair = ((int,int))libs[i].items[count];
            var th = (int)libs[i].emphasize;
            return pair.Item1 >= th || pair.Item2 >= th;
        }

        private bool EmpConComp(int i, int count)
        {
            if(libs[i].items[count] == null) return false;
            var comp = (TextureImporterCompression)libs[i].items[count];
            var mask = (int)libs[i].emphasize;
            switch(comp)
            {
                case TextureImporterCompression.CompressedLQ: return MathHelper.BitMask(mask, 1);
                case TextureImporterCompression.Compressed  : return MathHelper.BitMask(mask, 2);
                case TextureImporterCompression.CompressedHQ: return MathHelper.BitMask(mask, 3);
                default:                                      return MathHelper.BitMask(mask, 0);
            }
        }

        private bool EmpConForm(int i, int count)
        {
            if(libs[i].items[count] == null) return false;
            return FilterString(FormatToString(libs[i].items[count]), (string)libs[i].emphasize);
        }

        private bool EmpConSM(int i, int count)
        {
            if(libs[i].items[count] == null) return false;

            bool mip = false;
            if(libs[indMips].items[count] != null) mip = (bool)libs[indMips].items[count];
            var sm = (bool)libs[i].items[count];
            var mask = (int)libs[i].emphasize;

            int val = sm ? 0x00000002 : 0x00000001;
            if(mip != sm) val |= 0x00000004;
            return (mask & val) != 0x00000000;
        }

        private bool MainGUIVRAM(int i, int count, bool emp)
        {
            GUIUtils.LabelField(libs[i].rect, EditorUtility.FormatBytes((long)libs[i].items[count]), emp);
            return false;
        }

        private bool MainGUIRes(int i, int count, bool emp)
        {
            var pair = ((int,int))libs[i].items[count];
            GUIUtils.LabelField(libs[i].rect, $"{pair.Item1}x{pair.Item2}", emp);
            return false;
        }

        private bool MainGUIResMax(int i, int count, bool emp)
        {
            if(libs[i].items[count] == null) return false;
            var maxres = (int)libs[i].items[count];
            int index = 2048;
            switch(maxres)
            {
                case 32  : index = 0; break;
                case 64  : index = 1; break;
                case 128 : index = 2; break;
                case 256 : index = 3; break;
                case 512 : index = 4; break;
                case 1024: index = 5; break;
                case 2048: index = 6; break;
                case 4096: index = 7; break;
                case 8192: index = 8; break;
            }
            index = GUIUtils.PopupField(libs[i].rect, index, libs[i].emphasizeLabels, emp);
            switch(index)
            {
                case 0: libs[i].items[count] = 32  ; break;
                case 1: libs[i].items[count] = 64  ; break;
                case 2: libs[i].items[count] = 128 ; break;
                case 3: libs[i].items[count] = 256 ; break;
                case 4: libs[i].items[count] = 512 ; break;
                case 5: libs[i].items[count] = 1024; break;
                case 6: libs[i].items[count] = 2048; break;
                case 7: libs[i].items[count] = 4096; break;
                case 8: libs[i].items[count] = 8192; break;
            }
            return false;
        }

        private bool MainGUIComp(int i, int count, bool emp)
        {
            if(libs[i].items[count] == null) return false;
            if(libs[i].isEditable)
            {
                int val = 0;
                switch((TextureImporterCompression)libs[i].items[count])
                {
                    case TextureImporterCompression.Uncompressed: val = 0; break;
                    case TextureImporterCompression.CompressedLQ: val = 1; break;
                    case TextureImporterCompression.Compressed  : val = 2; break;
                    case TextureImporterCompression.CompressedHQ: val = 3; break;
                }
                val = GUIUtils.PopupField(libs[i].rect, val, libs[i].emphasizeLabels, emp);
                switch(val)
                {
                    case 0: libs[i].items[count] = TextureImporterCompression.Uncompressed; break;
                    case 1: libs[i].items[count] = TextureImporterCompression.CompressedLQ; break;
                    case 2: libs[i].items[count] = TextureImporterCompression.Compressed  ; break;
                    case 3: libs[i].items[count] = TextureImporterCompression.CompressedHQ; break;
                }
            }
            else
            {
                GUIUtils.LabelField(libs[i].rect, GUIUtils.QualityToString((TextureImporterCompression)libs[i].items[count]), emp);
            }
            return false;
        }

        private bool MainGUIForm(int i, int count, bool emp)
        {
            if(libs[i].items[count] == null) return false;
            GUIUtils.LabelField(libs[i].rect, FormatToString(libs[i].items[count]), emp);
            return false;
        }

        private bool MainGUICrunch(int i, int count, bool emp)
        {
            return libs[i].items[count] != null && CanUseCrunch(count);
        }

        private bool MainGUICompQ(int i, int count, bool emp)
        {
            if(
                libs[i].items[count] == null ||
                !CanUseCrunch(count) ||
                libs[indCrunchs].items[count] == null ||
                !(bool)libs[indCrunchs].items[count]
            ) return false;

            if(libs[i].isEditable)
            {
                libs[i].items[count] = Mathf.Clamp(GUIUtils.IntField(libs[i].rect, (int)libs[i].items[count], emp), 0, 100);
            }
            else
            {
                GUIUtils.LabelField(libs[i].rect, libs[i].items.ToString(), emp);
            }
            return false;
        }

        private bool MainGUIAlpha(int i, int count, bool emp)
        {
            return libs[i].items[count] != null &&
                libs[indASrcs].items[count] != null &&
                (TextureImporterAlphaSource)libs[indASrcs].items[count] != TextureImporterAlphaSource.None;
        }

        private bool CanUseCrunch(int count)
        {
            if(libs[indComps].items[count] is TextureImporterCompression q)
            {
                switch(q)
                {
                    case TextureImporterCompression.Uncompressed: return false;
                    case TextureImporterCompression.CompressedLQ: return true;
                    case TextureImporterCompression.Compressed  : return true;
                    case TextureImporterCompression.CompressedHQ: return false;
                }
            }
            return false;
        }

        private string FormatToString(object format)
        {
            switch(format)
            {
                case TextureFormat f      : return $"{f} ({MathHelper.FormatToBPP(f,true)} bpp)";
                case RenderTextureFormat f: return $"{f} ({MathHelper.FormatToBPP(f,true)} bpp)";
                default                   : return format.ToString();
            }
        }

        private bool EmpConReps(int i, int count)
        {
            return libs[i].items[count] != libs[indNames].items[count];
        }
    }
}
