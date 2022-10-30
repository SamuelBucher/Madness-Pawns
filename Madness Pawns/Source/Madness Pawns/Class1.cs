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
                    __result = ShaderDatabase.Transparent;
            }
        }

        /*[HarmonyPatch(typeof(PawnGraphicSet))]
        [HarmonyPatch("HairMeshSet", MethodType.Getter)]
        public static class HairMeshSetPatch
        {
            [HarmonyPostfix]
            public static GraphicMeshSet HairMeshSetPostfix()
            {
                return MeshPool.GetMeshSetForWidth(MeshPool.HumanlikeHeadNarrowWidth);
            }
        }*/

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
                //var pawn = typeof(PawnRenderer).GetField("pawn");
                //var pawn = PawnRenderer.GetType().GetField("MethodName");
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

        /*[HarmonyPatch(typeof(PawnRenderer), "OffsetBeardLocationForHead")]
        class OffsetBeardLocationForHeadPatch
        {
            [HarmonyPostfix]
            public static void OffsetBeardLocationForHeadPostfix(ref Vector3 __result)
            {
                __result += Vector3.back * 0.03f;
            }
        }*/

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

                    //__instance.headGraphic = __instance.pawn.story.headType.GetGraphic(__instance.pawn.story.SkinColor, false, __instance.pawn.story.SkinColorOverriden);
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
            public static bool GetFurBodyGraphicPathPrefix(ref FurDef __instance, ref string __result, Pawn pawn)
            {
                for (int i = 0; i < __instance.bodyTypeGraphicPaths.Count; i++)
                {
                    if (/*__instance.bodyTypeGraphicPaths[i].bodyType == pawn.story.bodyType*/ __instance.bodyTypeGraphicPaths[i].bodyType == Misc.getPawnBodyType(pawn))
                    {
                        __result =  __instance.bodyTypeGraphicPaths[i].texturePath; ;
                        return false;
                    }
                }
                __result = null;
                return false;
            }
        }
    }

    /*[HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
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
    }*/

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

        /*public static BodyTypeDef getPawnHeadType(Pawn pawn)
        {
            if (pawn.story.bodyType == BodyTypeDefOf.Baby)
                return BodyTypeDefOf.Baby;
            if (pawn.story.bodyType == BodyTypeDefOf.Child)
                return BodyTypeDefOf.Child;
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<Settings>().enableFemaleBodyType)
                return BodyTypeDefOf.Female;

            return BodyTypeDefOf.Male;
        }*/
    }

    public class Settings : ModSettings
    {
        public bool renderMaleHair;
        public bool renderFemaleHair;
        public bool enableFemaleBodyType;
        public bool thinBodies;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderMaleHair, "renderMaleHair", false);
            Scribe_Values.Look(ref renderFemaleHair, "renderFemaleHair", false);
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
