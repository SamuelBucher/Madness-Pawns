using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Madness_Pawns
{
    public class MP_Settings : ModSettings
    {
        public bool renderMaleHair;
        public bool renderFemaleHair;
        public bool differentFemaleHead;
        public bool FemaleBodyType;
        public bool DefaultAdultBodyTypes;
        public bool DefaultChildBodyType;
        public bool thinBodies;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref renderMaleHair, "renderMaleHair", false);
            Scribe_Values.Look(ref renderFemaleHair, "renderFemaleHair", false);
            Scribe_Values.Look(ref differentFemaleHead, "differentFemaleHead", false);
            Scribe_Values.Look(ref FemaleBodyType, "FemaleBodyType", false);
            Scribe_Values.Look(ref DefaultAdultBodyTypes, "DefaultAdultBodyTypes", false);
            Scribe_Values.Look(ref DefaultChildBodyType, "DefaultChildBodyType", false);
            Scribe_Values.Look(ref thinBodies, "thinBodies", false);
            base.ExposeData();
        }
    }

    public class MadnessPawns : Mod
    {
        MP_Settings settings;

        public MadnessPawns(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MP_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Render hair on men", ref settings.renderMaleHair, "Requires reload");
            listingStandard.CheckboxLabeled("Render hair on women", ref settings.renderFemaleHair, "Requires reload");
            listingStandard.CheckboxLabeled("Enable alt head for women", ref settings.differentFemaleHead, "Requires reload");
            listingStandard.CheckboxLabeled("Use the vanilla RimWorld body types for child pawns", ref settings.DefaultChildBodyType, "Requires reload");
            listingStandard.CheckboxLabeled("Use the vanilla RimWorld body types for adult pawns (overrides the 2 options below)", ref settings.DefaultAdultBodyTypes, "Requires reload");
            listingStandard.CheckboxLabeled("Use the vanilla female body type for women", ref settings.FemaleBodyType, "Requires reload");
            listingStandard.CheckboxLabeled("Use the vanilla thin body instead of standard grunt body (for modded apparel compatability)", ref settings.thinBodies, "Requires reload");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Madness Pawns";
        }
    }
}
