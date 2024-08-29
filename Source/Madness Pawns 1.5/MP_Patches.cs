using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;

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
                    code[startIndex] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MP_Utility), "getGruntHeadGraphic", new Type[] { typeof(Verse.PawnRenderNode_Head), typeof(Verse.Pawn)}));
                }

                return code;
            }
        }

        //Use the grunt head data for the beard offset
        [HarmonyPatch(typeof(PawnRenderNodeWorker_Beard), "OffsetFor")]
        public static class OffsetFor_Patch
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
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MP_Utility), "getGruntHead", new Type[] { typeof(Verse.Pawn) }));
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
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MP_Utility), "getGruntHead", new Type[] { typeof(Verse.Pawn) }));
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
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MP_Utility), "getGruntHead", new Type[] { typeof(Verse.Pawn) }));
                        i++;
                    }
                    else
                        yield return code[i];
                }
            }
        }
    }
}
