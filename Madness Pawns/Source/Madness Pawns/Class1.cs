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

        [HarmonyPatch(typeof(PawnGraphicSet))]
        [HarmonyPatch("HairMeshSet", MethodType.Getter)]
        public static class HairMeshSetPatch
        {
            [HarmonyPostfix]
            public static GraphicMeshSet HairMeshSetPostfix(GraphicMeshSet result)
            {
                return MeshPool.GetMeshSetForWidth(MeshPool.HumanlikeHeadNarrowWidth);
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "OffsetBeardLocationForCrownType")]
        class OffsetBeardLocationForCrownTypePatch
        {
            [HarmonyPrefix]
            public static bool OffsetBeardLocationForCrownTypePrefix(ref Vector3 __result, Vector3 beardLoc, Rot4 headFacing)
            {
                __result = beardLoc;

                if (headFacing == Rot4.East)
                {
                    __result += Vector3.right * -0.034f;
                    __result += Vector3.forward * 0.03f;
                }
                else if (headFacing == Rot4.West)
                {
                    __result += Vector3.right * 0.034f;
                    __result += Vector3.forward * 0.03f;
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
                FieldInfo fld = typeof(PawnRenderer).GetField("pawn");
                Pawn instancePawn = (Pawn)fld.GetValue(__instance);

                BodyTypeDef bodyType = Misc.getPawnBodyType(instancePawn);

                Vector2 vector = bodyType.headOffset * Mathf.Sqrt(instancePawn.ageTracker.CurLifeStage.bodySizeFactor);
                switch (rotation.AsInt)
                {
                    case 0:
                        __result =  new Vector3(0f, 0f, vector.y);
                        return false;
                    case 1:
                        __result = new Vector3(vector.x, 0f, vector.y);
                        return false;
                    case 2:
                        __result = new Vector3(0f, 0f, vector.y);
                        return false;
                    case 3:
                        __result = new Vector3(-vector.x, 0f, vector.y);
                        return false;
                    default:
                        Log.Error("BaseHeadOffsetAt error in " + instancePawn);
                        __result = Vector3.zero;
                        return false;
                }
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

            return BodyTypeDefOf.Male;
        }
    }

    public class Settings : ModSettings
    {
        public bool renderMaleHair;
        public bool renderFemaleHair;
        public bool enableFemaleBodyType;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderMaleHair, "renderMaleHair", false);
            Scribe_Values.Look(ref renderFemaleHair, "renderFemaleHair", false);
            Scribe_Values.Look(ref enableFemaleBodyType, "enableFemaleBodyType", true);
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
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Madness Pawns";
        }
    }
}
