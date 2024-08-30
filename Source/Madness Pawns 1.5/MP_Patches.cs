using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.Networking;

namespace Madness_Pawns
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static readonly FieldInfo story = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
        private static readonly MethodInfo getGruntHead = AccessTools.Method(typeof(MP_Utility), "getGruntHead", new Type[] { typeof(Verse.Pawn) });
        private static readonly MethodInfo getGruntHeadGraphic = AccessTools.Method(typeof(MP_Utility), "getGruntHeadGraphic", new Type[] { typeof(Verse.PawnRenderNode_Head), typeof(Verse.Pawn) });
        private static readonly MethodInfo getGruntBody = AccessTools.Method(typeof(MP_Utility), "getGruntBody", new Type[] { typeof(Verse.Pawn) });

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
            public static void SkinColor_Postfix(ref Pawn_StoryTracker __instance, ref Color __result)
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

        //Use the grunt head instead of the pawn's actual head
        [HarmonyPatch(typeof(PawnRenderNode_Head), "GraphicFor")]
        public static class GraphicForHead_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                int startIndex = -1;

                for (int i = 0; i < code.Count - 1; i++)
                {
                    if (code[i].opcode == OpCodes.Callvirt && code[i+1].opcode == OpCodes.Call && code[i+2].opcode == OpCodes.Ret)
                    {
                        startIndex = i+1;
                    }
                }
                
                if (startIndex != -1)
                {
                    code[startIndex - 4].opcode = OpCodes.Pop;
                    code[startIndex - 1] = new CodeInstruction(OpCodes.Nop);
                    code[startIndex] = new CodeInstruction(OpCodes.Call, getGruntHeadGraphic);
                }

                return code;
            }
        }

        //Use the grunt head data for the beard offset
        [HarmonyPatch(typeof(PawnRenderNodeWorker_Beard), "OffsetFor")]
        public static class OffsetFor_Beard_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                bool over = false;
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldfld && over != true)
                    {
                        over = true;
                        yield return code[i];
                        i += 2;
                        yield return new CodeInstruction(OpCodes.Call, getGruntHead);
                    }
                    else
                        yield return code[i];
                }
            }
        }
        
        //Use the grunt head data for the hair mesh size
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeHairSetForPawn")]
        public static class GetHumanlikeHairSetForPawn_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldfld && i + 3 < code.Count && code[i + 3].opcode == OpCodes.Ldarg_1)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntHead);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the grunt head data for the beard mesh size
        [HarmonyPatch(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeBeardSetForPawn")]
        public static class GetHumanlikeBeardSetForPawn_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldfld && i + 3 < code.Count && code[i + 3].opcode == OpCodes.Ldarg_1)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntHead);
                        i++;
                    }
                    else
                        yield return code[i];
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

                return toPatch;
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
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
        //[HarmonyPatch(typeof(Dialog_NamePawn), MethodType.Constructor)]
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
                    if (code[i].opcode == OpCodes.Dup && code[i-1].LoadsField(story))
                    {
                        yield return new CodeInstruction(OpCodes.Nop);
                        yield return code[++i];
                        yield return new CodeInstruction(OpCodes.Nop);
                        i++;
                    }
                    else if (code[i].opcode == OpCodes.Br_S && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                    }
                    else
                        yield return code[i];
                }
            }
        }

        /*
        //Use the grunt body instead of the pawn's actual body
        [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
        public static class GraphicForBody_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }
        
        //Use the furred grunt body instead for furred pawns
        [HarmonyPatch(typeof(FurDef), "GetFurBodyGraphicPath")]
        class GetFurBodyGraphicPath_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the grunt body type for clothes
        [HarmonyPatch]
        public static class PawnRenderNode_Apparely_Patch
        {
            public static MethodBase TargetMethod()
            {
                Type type = AccessTools.FirstInner(typeof(PawnRenderNode_Apparel), t => t.Name.Contains("<GraphicsFor>d__5"));
                return AccessTools.FirstMethod(type, method => method.Name.Contains("MoveNext"));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }
        
        //Use the grunt body's offsets for apparel
        [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body), "OffsetFor")]
        public static class OffsetFor_Apparel_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }
        
        //Use the grunt body's scale for apparel
        [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body), "ScaleFor")]
        public static class ScaleFor_Apparel_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the grunt body's scale for attachments
        [HarmonyPatch(typeof(PawnRenderNodeWorker_AttachmentBody), "ScaleFor")]
        public static class ScaleFor_AttachmentBody_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }

        //Use the grunt body's wound anchors
        [HarmonyPatch(typeof(PawnRenderNodeWorker_Eye), "TryGetWoundAnchor")]
        public static class TryGetWoundAnchor_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].LoadsField(story) && code[i + 1].opcode == OpCodes.Ldfld)
                    {
                        yield return new CodeInstruction(OpCodes.Call, getGruntBody);
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }
        */
    }
}
