using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Madness_Pawns
{
    public static class MP_Utility
    {
        /*public GraphicMeshSet MP_GetHumanlikeHairSetForPawn(Pawn pawn, float wFactor = 1f, float hFactor = 1f)
        {
            Vector2 vector = new Vector2(1.3f, 1.5f) * new Vector2(wFactor, hFactor);
            if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor != null)
            {
                vector *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
            }
            return MeshPool.GetMeshSetForSize(vector.x, vector.y);
        }*/

        public static HeadTypeDef getGruntHead(Pawn pawn)
        {
            HeadTypeDef origHeadType = pawn.story.headType;
            HeadTypeDef newHeadType;

            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().differentFemaleHead)
            {
                if (MP_Cache.HeadTypeCacheFemale.TryGetValue(origHeadType, out newHeadType))
                    return newHeadType;
                return origHeadType;
            }
            if (MP_Cache.HeadTypeCacheMale.TryGetValue(origHeadType, out newHeadType))
                return newHeadType;
            return origHeadType;
        }

        public static Graphic_Multi getGruntHeadGraphic (PawnRenderNode_Head node, Pawn pawn)
        {
            return getGruntHead(pawn).GetGraphic(pawn, node.ColorFor(pawn));
        }
        /*
        public Graphic_Multi getGruntHead(PawnRenderNode_Head node, Pawn pawn)
        {
            string headName = pawn.story.headType.defName;

            //Conditional female head
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().differentFemaleHead)
            {
                if (headName.Contains("Furskin"))
                {
                    if (headName.Contains("Heavy"))
                        return MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy.GetGraphic(pawn, node.ColorFor(pawn));
                    if (headName.Contains("Gaunt"))
                        return MP_HeadTypeDefOf.Grunt_Female_Furskin_Gaunt.GetGraphic(pawn, node.ColorFor(pawn));
                    return MP_HeadTypeDefOf.Grunt_Female_Furskin.GetGraphic(pawn, node.ColorFor(pawn));
                }

                if (headName.Contains("Heavy"))
                    return MP_HeadTypeDefOf.Grunt_Female_Heavy.GetGraphic(pawn, node.ColorFor(pawn));
                if (headName.Contains("Gaunt"))
                    return MP_HeadTypeDefOf.Grunt_Female_Gaunt.GetGraphic(pawn, node.ColorFor(pawn));
                return MP_HeadTypeDefOf.Grunt_Female.GetGraphic(pawn, node.ColorFor(pawn));
            }

            //Standard choice
            if (headName.Contains("Furskin"))
            {
                if (headName.Contains("Heavy"))
                    return MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy.GetGraphic(pawn, node.ColorFor(pawn));
                if (headName.Contains("Gaunt"))
                    return MP_HeadTypeDefOf.Grunt_Male_Furskin_Gaunt.GetGraphic(pawn, node.ColorFor(pawn));
                return MP_HeadTypeDefOf.Grunt_Male_Furskin.GetGraphic(pawn, node.ColorFor(pawn));
            }
            if (headName.Contains("Heavy"))
                return MP_HeadTypeDefOf.Grunt_Male_Heavy.GetGraphic(pawn, node.ColorFor(pawn));
            if (headName.Contains("Gaunt"))
                return MP_HeadTypeDefOf.Grunt_Male_Gaunt.GetGraphic(pawn, node.ColorFor(pawn));
            return MP_HeadTypeDefOf.Grunt_Male.GetGraphic(pawn, node.ColorFor(pawn));
        }*/
    }
}
