using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Madness_Pawns
{
    public static class MP_Utility
    {
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

        public static BodyTypeDef getGruntBody(Pawn pawn)
        {
            if (LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().DefaultAdultBodyTypes)
                return pawn.story.bodyType;
            if (pawn.story.bodyType == BodyTypeDefOf.Baby)
                return BodyTypeDefOf.Baby;
            if (pawn.story.bodyType == BodyTypeDefOf.Child)
                if (LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().DefaultChildBodyType)
                    return BodyTypeDefOf.Child;
                else
                    return MP_BodyTypeDefOf.GruntChild;
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().FemaleBodyType)
                return BodyTypeDefOf.Female;
            if (LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().thinBodies)
                return BodyTypeDefOf.Thin;
            return MP_BodyTypeDefOf.Grunt;
        }

        public static string checkGraphicPath(Apparel apparel, BodyTypeDef bodyType)
        {
            string path = apparel.WornGraphicPath + "_" + bodyType.defName;
            if (bodyType == MP_BodyTypeDefOf.Grunt && ContentFinder<Texture2D>.Get(path + "_south", reportFailure: false) == null)
                return apparel.WornGraphicPath + "_" + BodyTypeDefOf.Male;
            return path;
        }
    }
}
