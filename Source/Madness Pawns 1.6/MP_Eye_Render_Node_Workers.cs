using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Verse;

namespace Madness_Pawns
{
    public class MP_PawnRenderNode_Eye : PawnRenderNode_AttachmentHead
    {
        public MP_PawnRenderNode_Eye(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
        { }

        public override Mesh GetMesh(PawnDrawParms parms)
        {
            if (parms.facing.IsHorizontal)
                parms.facing = Rot4.East;
            return base.GetMesh(parms);
        }

        protected override string TexPathFor(Pawn pawn)
        {
            if (pawn.gender == Gender.Female && LoadedModManager.GetMod<MadnessPawns>().GetSettings<MP_Settings>().differentFemaleHead)
            {
                if (!props.texPathsFemale.NullOrEmpty())
                {
                    using (new RandBlock(TexSeedFor(pawn)))
                    {
                        return props.texPathsFemale.RandomElement();
                    }
                }
                if (!props.texPathFemale.NullOrEmpty())
                {
                    return props.texPathFemale;
                }
            }
            if (!props.texPaths.NullOrEmpty())
            {
                using (new RandBlock(TexSeedFor(pawn)))
                {
                    return props.texPaths.RandomElement();
                }
            }
            return props.texPath;
        }
    }

    public class MP_PawnRenderNodeWorker_Eye : PawnRenderNodeWorker_Eye
    {
        protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            Graphic graphic = GetGraphic(node, parms);
            if (graphic == null)
            {
                return null;
            }
            if (parms.facing.IsHorizontal && node.Props.anchorTag == "LeftEye")
            {
                parms.facing = parms.facing.Opposite;
            }
            Material material = graphic.NodeGetMat(parms);
            if (material != null && !parms.Portrait && parms.flags.FlagSet(PawnRenderFlags.Invisible))
            {
                material = InvisibilityMatPool.GetInvisibleMat(material);
            }
            return material;
        }
    }

    public class MP_PawnRenderNodeWorker_HediffEye : PawnRenderNodeWorker_HediffEye
    {
        protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            Graphic graphic = GetGraphic(node, parms);
            if (graphic == null)
            {
                return null;
            }
            if (parms.facing.IsHorizontal && node.hediff.Part.woundAnchorTag == "LeftEye")
            {
                parms.facing = parms.facing.Opposite;
            }
            Material material = graphic.NodeGetMat(parms);
            if (material != null && !parms.Portrait && parms.flags.FlagSet(PawnRenderFlags.Invisible))
            {
                material = InvisibilityMatPool.GetInvisibleMat(material);
            }
            return material;
        }
    }
}
