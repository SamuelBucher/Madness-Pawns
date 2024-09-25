using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using WAYCAN;

namespace Madness_Pawns
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static readonly FieldInfo story = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
        private static readonly FieldInfo headType = AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.headType));
        private static readonly FieldInfo bodyType = AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.bodyType));

        private static readonly MethodInfo get_IsHorizontal = AccessTools.Method(typeof(Rot4), "get_IsHorizontal", new Type[] { });
        private static readonly MethodInfo get_Props = AccessTools.Method(typeof(PawnRenderNode), "get_Props", new Type[] { });

        private static readonly MethodInfo getGruntHead = AccessTools.Method(typeof(MP_Utility), nameof(MP_Utility.GetGruntHead), new Type[] { typeof(Verse.Pawn) });
        private static readonly MethodInfo getGruntBody = AccessTools.Method(typeof(MP_Utility), nameof(MP_Utility.GetGruntBody), new Type[] { typeof(Verse.Pawn) });
        private static readonly MethodInfo checkGraphicPath = AccessTools.Method(typeof(MP_Utility), nameof(MP_Utility.CheckGraphicPath), new Type[] { typeof(RimWorld.Apparel), typeof(RimWorld.BodyTypeDef) });

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.SamBucher.MadnessPawns");
            harmony.PatchAll();
        }

        //Make baseline skin color gray
        [HarmonyPatch(typeof(Pawn_StoryTracker))]
        [HarmonyPatch("SkinColorBase", MethodType.Getter)]
        public static class SkinColor_Patch
        {
            [HarmonyPostfix]
            public static void SkinColor_Postfix(ref Color __result)
            {
                __result = new Color(192f / 255f, 190f / 255f, 188f / 255f);
            }
        }

        //Remove the skin shader from the heads of living human pawns (fuck you, Tynan, this should have been an XML job)
        [HarmonyPatch(typeof(ShaderUtility), "GetSkinShaderAbstract")]
        public static class GetSkinShaderAbstract_Patch
        {
            [HarmonyPostfix]
            public static void GetSkinShaderAbstract_Postfix(ref Shader __result)
            {
                if (__result == ShaderDatabase.CutoutSkin)
                    __result = ShaderDatabase.Cutout;
            }
        }

        //Remove hair (optional)
        [HarmonyPatch(typeof(PawnRenderNode_Hair), "GraphicFor")]
        public static class GraphicForHair_Patch
        {
            [HarmonyPostfix]
            public static void GraphicForHair_Postfix(ref Pawn pawn, ref Graphic __result)
            {
                if ((!LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().renderMaleHair && (pawn.gender == Gender.Male)) ||
                    (!LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().renderFemaleHair && (pawn.gender == Gender.Female)))
                    __result = null;
            }
        }

        [HarmonyPatch]
        class HeadGetterPatches
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                List<MethodBase> toPatch = new List<MethodBase>();

                //Use the grunt head data for the hair mesh size
                toPatch.Add(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn)));
                //Use the grunt head data for the beard mesh size
                toPatch.Add(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn)));
                //Use the grunt head data for the beard offset
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Beard), nameof(PawnRenderNodeWorker_Beard.OffsetFor)));
                //Use the grunt head data for the head offset
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Head), nameof(PawnRenderNodeWorker_Head.OffsetFor)));
                //Eye wound anchors
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), "TryGetWoundAnchor"));
                //Tattoo graphic
                toPatch.Add(AccessTools.Method(typeof(TattooDef), nameof(TattooDef.GraphicFor)));
                //Anchor again?
                toPatch.Add(AccessTools.Method(typeof(PawnDrawUtility), nameof(PawnDrawUtility.AnchorUsable)));
                //Anchor 3
                toPatch.Add(AccessTools.Method(typeof(PawnDrawUtility), nameof(PawnDrawUtility.CalcAnchorData)));
                //WAYCAN patch
                toPatch.Add(AccessTools.Method(typeof(WAYCAN_Method), nameof(WAYCAN_Method.ifNarrowMesh)));

                return toPatch;
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].LoadsField(headType))
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntHead);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the grunt head instead of the pawn's actual head
        [HarmonyPatch(typeof(PawnRenderNode_Head), "GraphicFor")]
        public static class GraphicFor_Head_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    yield return code[i];
                    if (code[i].LoadsField(headType))
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, getGruntHead);
                    }
                }
            }
        }

        [HarmonyPatch]
        class BodyGetterPatches
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                List<MethodBase> toPatch = new List<MethodBase>();

                //Use the grunt body instead of the pawn's actual body
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNode_Body), nameof(PawnRenderNode_Body.GraphicFor)));
                //Use the furred grunt body instead for furred pawns
                toPatch.Add(AccessTools.Method(typeof(FurDef), nameof(FurDef.GetFurBodyGraphicPath)));
                //Use the grunt body type for clothes
                Type weirdType = AccessTools.FirstInner(typeof(PawnRenderNode_Apparel), t => t.Name.Contains("<GraphicsFor>d__5"));
                toPatch.Add(AccessTools.FirstMethod(weirdType, method => method.Name.Contains("MoveNext")));
                //Use the grunt body's offsets for apparel
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Apparel_Body), nameof(PawnRenderNodeWorker_Apparel_Body.OffsetFor)));
                //Use the grunt body's scale for apparel
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Apparel_Body), nameof(PawnRenderNodeWorker_Apparel_Body.ScaleFor)));
                //Use the grunt body's scale for attachments
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_AttachmentBody), nameof(PawnRenderNodeWorker_AttachmentBody.ScaleFor)));
                //Use the grunt body's wound anchors
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), "TryGetWoundAnchor"));
                //Not entirely shure what this one does
                toPatch.Add(AccessTools.Method(typeof(DrawData), nameof(DrawData.ScaleFor)));
                //Might not need this one
                toPatch.Add(AccessTools.Method(typeof(PawnRenderTree), "ProcessApparel"));
                //Body position for the bed?
                toPatch.Add(AccessTools.Method(typeof(PawnRenderer), "GetBodyPos"));
                //Use grunt body head offset
                toPatch.Add(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)));
                //Everything seemed to work fine without this, but just to be certain
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNode), "TexPathFor"));
                //Use grunt body grahpic scale, whatever that entails
                toPatch.Add(AccessTools.Method(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.OffsetFor)));
                //Use grunt attach points
                toPatch.Add(AccessTools.PropertyGetter(typeof(HediffComp_AttachPoints), "Points"));
                //Silhouette grunt bed offset
                toPatch.Add(AccessTools.Method(typeof(Graphic_PawnBodySilhouette), nameof(Graphic_PawnBodySilhouette.DrawWorker)));
                //Deathrest effect
                toPatch.Add(AccessTools.Method(typeof(JobDriver_Deathrest), "<MakeNewToils>b__9_2"));
                //Tattoo graphic
                toPatch.Add(AccessTools.Method(typeof(TattooDef), nameof(TattooDef.GraphicFor)));
                //Wound anchors 1
                toPatch.Add(AccessTools.Method(typeof(PawnWoundDrawer), "DebugDraw"));
                //Wound anchors 2
                Type weirdType2 = AccessTools.FirstInner(typeof(PawnDrawUtility), t => t.Name.Contains("<FindAnchors>d__3"));
                toPatch.Add(AccessTools.FirstMethod(weirdType2, method => method.Name.Contains("MoveNext")));
                //Mutant bodies
                toPatch.Add(AccessTools.Method(typeof(MutantDef), nameof(MutantDef.GetBodyGraphicPath)));
                //Creepjoiner bodies
                toPatch.Add(AccessTools.Method(typeof(CreepJoinerFormKindDef), nameof(CreepJoinerFormKindDef.GetBodyGraphicPath)));

                return toPatch;
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].LoadsField(bodyType))
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Dialog height because hulk isn't a thing anymore
        [HarmonyPatch]
        public static class Dialog_NamePawn_Patch
        {
            public static MethodBase TargetMethod()
            {
                return AccessTools.GetDeclaredConstructors(typeof(Dialog_NamePawn))[0];
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Dup && code[i - 1].LoadsField(story))
                    {
                        yield return new CodeInstruction(OpCodes.Nop);
                        yield return code[++i];
                        yield return new CodeInstruction(OpCodes.Nop);
                        i++;
                    }
                    else if (code[i].opcode == OpCodes.Br_S && code[i + 1].LoadsField(bodyType))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the male textures for apparel is the grunt ones aren't found
        [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
        public static class TryGetGraphicApparel_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldarg_0 && code[i + 2].operand is string oprnd && oprnd == "_")
                    {
                        yield return code[i];
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, checkGraphicPath);
                        i += 5;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Proper belt apparel scale (male, custom in unfeasible)
        [HarmonyPatch(typeof(WornGraphicData), "BeltScaleAt")]
        public static class BeltScaleAt_Patch
        {
            [HarmonyPrefix]
            public static bool BeltScaleAt_Prefix(ref Rot4 facing, ref BodyTypeDef bodyType, ref WornGraphicData __instance, ref Vector2 __result)
            {
                Vector2 scale1 = __instance.GetDirectionalData(facing).Scale;
                if (bodyType == MP_BodyTypeDefOf.Grunt)
                {
                    scale1 *= __instance.male.Scale;

                    __result = scale1;
                    return false;
                }
                return true;
            }
        }

        //Proper belt apparel offset (male, custom in unfeasible)
        [HarmonyPatch(typeof(WornGraphicData), "BeltOffsetAt")]
        public static class BeltOffsetAt_Patch
        {
            [HarmonyPrefix]
            public static bool BeltOffsetAt_Prefix(ref Rot4 facing, ref BodyTypeDef bodyType, ref WornGraphicData __instance, ref Vector2 __result)
            {
                WornGraphicDirectionData directionalData1 = __instance.GetDirectionalData(facing);
                Vector2 offset1 = directionalData1.offset;
                if (bodyType == MP_BodyTypeDefOf.Grunt)
                {
                    offset1 += directionalData1.male.offset;

                    __result = offset1;
                    return false;
                }
                return true;
            }
        }

    }
}
