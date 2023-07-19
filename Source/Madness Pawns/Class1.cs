using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Madness_Pawns
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.SamBucher.MadnessPawns");
            harmony.PatchAll();
        }
        
        [HarmonyPatch(typeof(PawnGraphicSet), "CalculateHairMats")]
        class CalculateHairMatsPatch
        {
            [HarmonyPostfix]
            public static void CalculateHairMatsPostfix(ref PawnGraphicSet __instance)
            {
                if ((!LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().renderMaleHair && (__instance.pawn.gender == Gender.Male)) ||
                    (!LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().renderFemaleHair && (__instance.pawn.gender == Gender.Female)))
                    __instance.hairGraphic = null;
            }
        }

        [HarmonyPatch(typeof(Pawn_StoryTracker))]
        [HarmonyPatch("SkinColor", MethodType.Getter)]
        public static class SkinColorPatch
        {
            [HarmonyPrefix]
            public static bool SkinColorPrefix(ref Pawn_StoryTracker __instance, ref Color __result)
            {
                Color? color = __instance.skinColorOverride;
                if (color == null)
                {
                    __result = new Color(192f / 255f, 190f / 255f, 188f / 255f);
                    return false;
                }
                __result = color.GetValueOrDefault();
                return false;
            }
        }

        [HarmonyPatch(typeof(ShaderUtility), "GetSkinShader")]
        class GetSkinShaderPatch
        {
            [HarmonyPostfix]
            public static void GetSkinShaderPostfix(ref Shader __result, bool skinColorOverriden)
            {
                if (!skinColorOverriden)
                    __result = ShaderDatabase.Cutout;
            }
        }

        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeHairSetForPawn")]
        public static class GetHumanlikeHairSetForPawnPatch
        {
            [HarmonyPrefix]
            public static bool GetHumanlikeHairSetForPawnPrefix(ref GraphicMeshSet __result, Pawn pawn)
            {
                Vector2 vector = new Vector2(1.3f, 1.5f);
                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor != null)
                {
                    vector *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
                }
                __result = MeshPool.GetMeshSetForWidth(vector.x, vector.y);

                return false;
            }
        }

        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeBeardSetForPawn")]
        public static class GetHumanlikeBeardSetForPawnPatch
        {
            [HarmonyPrefix]
            public static bool GetHumanlikeBeardSetForPawnPrefix(ref GraphicMeshSet __result, Pawn pawn)
            {
                Vector2 vector = new Vector2(1.3f, 1.5f);
                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor != null)
                {
                    vector *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
                }
                __result = MeshPool.GetMeshSetForWidth(vector.x, vector.y);

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "OffsetBeardLocationForHead")]
        class OffsetBeardLocationForHeadPatch
        {
            [HarmonyPrefix]
            public static bool OffsetBeardLocationForHeadPrefix(ref PawnRenderer __instance, ref Vector3 __result, HeadTypeDef head, Rot4 headFacing, Vector3 beardLoc)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;

                if (headFacing == Rot4.East)
                {
                    beardLoc += Vector3.right * -0.05f;
                }
                else if (headFacing == Rot4.West)
                {
                    beardLoc += Vector3.left * -0.05f;
                }
                beardLoc.y += 0.026061773f;
                beardLoc += new Vector3(0, 0, -0.05f);
                beardLoc += pawn.style.beardDef.GetOffset(pawn.story.headType, headFacing);
                __result = beardLoc;

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "DrawBodyGenes")]
        class DrawBodyGenesPatch
        {
            [HarmonyPrefix]
            public static bool DrawBodyGenesPrefix(ref PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;

                MethodInfo OverrideMaterialIfNeeded = __instance.GetType().GetMethod("OverrideMaterialIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);

                Vector2 bodyGraphicScale = Misc.getPawnBodyType(pawn).bodyGraphicScale;
                float num = (bodyGraphicScale.x + bodyGraphicScale.y) / 2f;
                foreach (GeneGraphicRecord geneGraphicRecord in __instance.graphics.geneGraphics)
                {
                    GeneGraphicData graphicData = geneGraphicRecord.sourceGene.def.graphicData;
                    if (graphicData.drawLoc == GeneDrawLoc.Tailbone && (bodyDrawType != RotDrawMode.Dessicated || geneGraphicRecord.sourceGene.def.graphicData.drawWhileDessicated))
                    {
                        Vector3 v = graphicData.DrawOffsetAt(bodyFacing);
                        v.x *= bodyGraphicScale.x;
                        v.z *= bodyGraphicScale.y;
                        Vector3 s = new Vector3(graphicData.drawScale * num, 1f, graphicData.drawScale * num);
                        Matrix4x4 matrix = Matrix4x4.TRS(rootLoc + v.RotatedBy(angle), quat, s);
                        Material material = geneGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : (Material)OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material, pawn, flags.FlagSet(PawnRenderFlags.Portrait)}));
                        GenDraw.DrawMeshNowOrLater((bodyFacing == Rot4.West) ? MeshPool.GridPlaneFlip(Vector2.one) : MeshPool.GridPlane(Vector2.one), matrix, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
        class BaseHeadOffsetAtPatch
        {
            [HarmonyPrefix]
            public static bool BaseHeadOffsetAtPrefix(ref PawnRenderer __instance, ref Vector3 __result, Rot4 rotation)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;

                Vector2 vector = Misc.getPawnBodyType(pawn).headOffset * Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
                switch (rotation.AsInt)
                {
                    case 0:
                        __result = new Vector3(0f, 0f, vector.y);
                        return false;
                    case 1:
                        __result = new Vector3(vector.x + 0.05f, 0f, vector.y);
                        return false;
                    case 2:
                        __result = new Vector3(0f, 0f, vector.y);
                        return false;
                    case 3:
                        __result = new Vector3(-vector.x - 0.05f, 0f, vector.y);
                        return false;
                    default:
                        Log.Error("BaseHeadOffsetAt error in " + pawn);
                        __result = Vector3.zero;
                        return false;
                }
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "HeadGeneDrawLocation")]
        class HeadGeneDrawLocationPatch
        {
            [HarmonyPrefix]
            public static bool HeadGeneDrawLocationPrefix(ref PawnRenderer __instance, ref Vector3 __result, GeneDef geneDef, Rot4 headFacing, Vector3 geneLoc, GeneDrawLayer layer)
            {
                if (layer != GeneDrawLayer.PostSkin)
                {
                    if (layer - GeneDrawLayer.PostHair <= 1)
                    {
                        geneLoc.y += 0.03335328f;
                    }
                    else
                    {
                        geneLoc.y += 0.028957527f;
                    }
                }
                else
                {
                    geneLoc.y += 0.026061773f;
                }
                geneLoc += geneDef.graphicData.DrawOffsetAt(headFacing);
                float narrowCrownHorizontalOffset = geneDef.graphicData.narrowCrownHorizontalOffset;
                if (narrowCrownHorizontalOffset != 0f && headFacing.IsHorizontal)
                {
                    if (headFacing == Rot4.East)
                    {
                        geneLoc += Vector3.right * -narrowCrownHorizontalOffset;
                    }
                    else if (headFacing == Rot4.West)
                    {
                        geneLoc += Vector3.right * narrowCrownHorizontalOffset;
                    }
                    geneLoc += Vector3.forward * -narrowCrownHorizontalOffset;
                }
                __result = geneLoc;

                return false;
            }
        }

        [HarmonyPatch(typeof(BeardDef), "GetOffset")]
        class GetOffsetPatch
        {
            [HarmonyPrefix]
            public static bool GetOffsetPrefix(ref BeardDef __instance, ref Vector3 __result, Rot4 rot)
            {
                if (rot == Rot4.North)
                {
                    __result = Vector3.zero;
                    return false;
                }
                if (rot == Rot4.South)
                {
                    __result = __instance.offsetNarrowSouth;
                    return false;
                }
                if (rot == Rot4.East)
                {
                    __result = __instance.offsetNarrowEast;
                    return false;
                }
                __result = new Vector3(-__instance.offsetNarrowEast.x, 0f, __instance.offsetNarrowEast.z);

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
        class ResolveApparelGraphicsPatch
        {
            [HarmonyPrefix]
            public static bool ResolveApparelGraphicsPrefix(ref PawnGraphicSet __instance)
            {
                __instance.ClearCache();
                __instance.apparelGraphics.Clear();

                BodyTypeDef bodyType = Misc.getPawnBodyType(__instance.pawn);

                using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ApparelGraphicRecord item;
                        if (ApparelGraphicRecordGetter.TryGetGraphicApparel(enumerator.Current, bodyType, out item))
                        {
                            __instance.apparelGraphics.Add(item);
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
        class ResolveAllGraphicsPatch
        {
            [HarmonyPostfix]
            public static void ResolveAllGraphicsPostfix(ref PawnGraphicSet __instance)
            {
                if (__instance.pawn.RaceProps.Humanlike)
                {
                    BodyTypeDef bodyType = Misc.getPawnBodyType(__instance.pawn);

                    Color color = __instance.pawn.story.SkinColorOverriden ? (PawnGraphicSet.RottingColorDefault * __instance.pawn.story.SkinColor) : PawnGraphicSet.RottingColorDefault;

                    __instance.headGraphic = Misc.getPawnHeadType(__instance.pawn).GetGraphic(__instance.pawn.story.SkinColor, false, __instance.pawn.story.SkinColorOverriden);
                    __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, __instance.pawn.story.SkinColor);
                    __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, color);
                    __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
                    __instance.headStumpGraphic = null;

                    if ((__instance.pawn.style != null && ModsConfig.IdeologyActive) && (__instance.pawn.style.BodyTattoo != null && __instance.pawn.style.BodyTattoo != TattooDefOf.NoTattoo_Body))
                    {
                        Color skinColor = __instance.pawn.story.SkinColor;
                        skinColor.a *= 0.8f;
                        __instance.bodyTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.style.BodyTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, bodyType.bodyNakedGraphicPath);
                    }
                    else
                        __instance.bodyTattooGraphic = null;
                }
            }
        }

        [HarmonyPatch(typeof(FurDef), "GetFurBodyGraphicPath")]
        class GetFurBodyGraphicPathPatch
        {
            [HarmonyPrefix]
            public static bool GetFurBodyGraphicPathPrefix(ref FurDef __instance, ref string __result, ref Pawn pawn)
            {
                for (int i = 0; i < __instance.bodyTypeGraphicPaths.Count; i++)
                {
                    if (__instance.bodyTypeGraphicPaths[i].bodyType == Misc.getPawnBodyType(pawn))
                    {
                        __result =  __instance.bodyTypeGraphicPaths[i].texturePath; ;
                        return false;
                    }
                }
                __result = null;
                return false;
            }
        }

        [HarmonyPatch]
        class DrawExtraEyeGraphicPatch
        {
            public static MethodBase TargetMethod()
            {
                var type = AccessTools.FirstInner(typeof(PawnRenderer), t => t.Name.Contains("c__DisplayClass54_0"));
                return AccessTools.FirstMethod(type, method => method.Name.Contains("g__DrawExtraEyeGraphic"));
            }

            [HarmonyPrefix]
            public static bool DrawExtraEyeGraphicPrefix(ref object __instance, ref Graphic graphic, ref float scale, ref float yOffset, ref bool drawLeft, ref bool drawRight)
            {
                //From PawnRenderer
                PawnRenderer parent = Traverse.Create(__instance).Field("<>4__this").GetValue() as PawnRenderer;
                Pawn pawn = Traverse.Create(parent).Field("pawn").GetValue() as Pawn;

                //From <>c__DisplayClass54_0
                Vector3? rootLoc = Traverse.Create(__instance).Field("rootLoc").GetValue() as Vector3?;
                Vector3? headOffset = Traverse.Create(__instance).Field("headOffset").GetValue() as Vector3?;
                Quaternion? quat = Traverse.Create(__instance).Field("quat").GetValue() as Quaternion?;
                Rot4? headFacing = Traverse.Create(__instance).Field("headFacing").GetValue() as Rot4?;
                PawnRenderFlags? flags = Traverse.Create(__instance).Field("flags").GetValue() as PawnRenderFlags?;


                Vector3 a = (Vector3)(rootLoc + headOffset + new Vector3(0f, 0.026061773f + yOffset, 0f) + quat * new Vector3(0f, 0f, -0.25f));

                BodyTypeDef.WoundAnchor woundAnchorLeft = pawn.story.bodyType.woundAnchors.FirstOrDefault((BodyTypeDef.WoundAnchor w) => 
                                                                                                        w.tag == "LeftEye" 
                                                                                                        && w.rotation == headFacing);
                BodyTypeDef.WoundAnchor woundAnchorRight = pawn.story.bodyType.woundAnchors.FirstOrDefault((BodyTypeDef.WoundAnchor w) => 
                                                                                                        w.tag == "RightEye" 
                                                                                                        && w.rotation == headFacing);
                Material mat1, mat2;

                if ((drawLeft != drawRight) && (drawRight && ((Rot4)headFacing).AsInt == Rot4.West.AsInt || drawLeft && ((Rot4)headFacing).AsInt == Rot4.East.AsInt))
                {
                    graphic = GraphicDatabase.Get<Graphic_Single>(graphic.path.Replace("_east", "_west"));
                }

                mat1 = graphic.MatAt((Rot4)headFacing, null);
                mat2 = graphic.MatAt(((Rot4)headFacing).Opposite, null);                

                if (headFacing == Rot4.South)
                {
                    if (woundAnchorLeft == null || woundAnchorRight == null)
                    {
                        return false;
                    }
                    mat2 = mat1;
                    if (drawLeft)
                    {
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlaneFlip(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * woundAnchorLeft.offset), (Quaternion)quat, Vector3.one), mat1, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                    if (drawRight)
                    {
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * woundAnchorRight.offset), (Quaternion)quat, Vector3.one), mat2, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
                if (headFacing == Rot4.East)
                {
                    if (woundAnchorLeft == null || woundAnchorRight == null)
                    {
                        return false;
                    }
                    if (drawRight)
                    {
                        Vector3 point = woundAnchorRight.offset;
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * point), (Quaternion)quat, Vector3.one), mat1, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                    if (drawLeft)
                    {
                        Vector3 point = woundAnchorLeft.offset;
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlaneFlip(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * point), (Quaternion)quat, Vector3.one), mat2, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
                if (headFacing == Rot4.West)
                {
                    if (woundAnchorLeft == null || woundAnchorRight == null)
                    {
                        return false;
                    }
                    if (drawLeft)
                    {
                        Vector3 point2 = woundAnchorLeft.offset;
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlaneFlip(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * point2), (Quaternion)quat, Vector3.one), mat2, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                    if (drawRight)
                    {
                        Vector3 point2 = woundAnchorRight.offset;
                        GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * scale), Matrix4x4.TRS((Vector3)(a + quat * point2), (Quaternion)quat, Vector3.one), mat1, ((PawnRenderFlags)flags).FlagSet(PawnRenderFlags.DrawNow));
                    }
                }

                return false;
            }
        }
    }

    public class Misc
    {
        public static BodyTypeDef getPawnBodyType(Pawn pawn)
        {
            if (pawn.story.bodyType == BodyTypeDefOf.Baby)
                return BodyTypeDefOf.Baby;
            if (pawn.story.bodyType == BodyTypeDefOf.Child)
                return BodyTypeDefOf.Child;
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().enableFemaleBodyType)
                return BodyTypeDefOf.Female;
            if (LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().thinBodies)
                return BodyTypeDefOf.Thin;
            return BodyTypeDefOf.Male;
        }

        public static HeadTypeDef getPawnHeadType(Pawn pawn)
        {
            List<HeadTypeDef> headDefList = DefDatabase<HeadTypeDef>.AllDefsListForReading;

            //Conditional female head
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().differentFemaleHead)
            {
                if (pawn.story.headType.defName.Contains("Furskin"))
                {
                    if (pawn.story.headType.defName.Contains("Heavy"))
                        return headDefList.Find(x => x.defName == "Furskin_Heavy2");
                    if (pawn.story.headType.defName.Contains("Gaunt"))
                        return headDefList.Find(x => x.defName == "Furskin_Narrow1");
                    return headDefList.Find(x => x.defName == "Furskin_Average2");
                }

                if (pawn.story.headType.defName.Contains("Heavy"))
                    return headDefList.Find(x => x.defName == "Female_HeavyJawNormal");
                if (pawn.story.headType.defName.Contains("Gaunt"))
                    return headDefList.Find(x => x.defName == "Female_NarrowNormal");
                return headDefList.Find(x => x.defName == "Female_AverageNormal");
            }

            //Standard choice
            if (pawn.story.headType.defName.Contains("Furskin"))
            {
                if (pawn.story.headType.defName.Contains("Heavy"))
                    return headDefList.Find(x => x.defName == "Furskin_Heavy1");
                if (pawn.story.headType.defName.Contains("Gaunt"))
                    return pawn.story.headType;
                return headDefList.Find(x => x.defName == "Furskin_Average1");
            }
            if (pawn.story.headType.defName.Contains("Heavy"))
                return headDefList.Find(x => x.defName == "Male_HeavyJawNormal");
            if (pawn.story.headType.defName.Contains("Gaunt"))
                return pawn.story.headType;
            return headDefList.Find(x => x.defName == "Male_AverageNormal");
        }
    }

    public class Settings : ModSettings
    {
        public bool renderMaleHair;
        public bool renderFemaleHair;
        public bool differentFemaleHead;
        public bool enableFemaleBodyType;
        public bool thinBodies;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderMaleHair, "renderMaleHair", false);
            Scribe_Values.Look(ref renderFemaleHair, "renderFemaleHair", false);
            Scribe_Values.Look(ref differentFemaleHead, "differentFemaleHead", false);
            Scribe_Values.Look(ref enableFemaleBodyType, "enableFemaleBodyType", false);
            Scribe_Values.Look(ref thinBodies, "thinBodies", false);
            base.ExposeData();
        }
    }

    public class MadnessPawns : Mod
    {
        Settings settings;

        public MadnessPawns(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Render hair on men", ref settings.renderMaleHair, "Requires reload");
            listingStandard.CheckboxLabeled("Render hair on women", ref settings.renderFemaleHair, "Requires reload");
            listingStandard.CheckboxLabeled("Enable alt head for women", ref settings.differentFemaleHead, "Requires reload");
            listingStandard.CheckboxLabeled("Enable different body type for women", ref settings.enableFemaleBodyType, "Requires reload");
            listingStandard.CheckboxLabeled("Use vanilla thin body instead of standard grunt body (for modded apparel compatability)", ref settings.thinBodies, "Requires reload");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Madness Pawns";
        }
    }
}
