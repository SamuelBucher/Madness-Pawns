using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Madness_Pawns
{
    [StaticConstructorOnStartup]
    public static class MP_Cache
    {
        public static Dictionary<HeadTypeDef, HeadTypeDef> HeadTypeCacheMale = new Dictionary<HeadTypeDef, HeadTypeDef>()
        { };

        public static Dictionary<HeadTypeDef, HeadTypeDef> HeadTypeCacheFemale = new Dictionary<HeadTypeDef, HeadTypeDef>()
        { };

        static MP_Cache()
        {
            HeadTypeCacheMale = new Dictionary<HeadTypeDef, HeadTypeDef>()
            {
                { MP_HeadTypeDefOf.Male_AverageNormal, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Male_AveragePointy, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Male_AverageWide, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Male_NarrowNormal, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Male_NarrowPointy, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Male_NarrowWide, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_AverageNormal, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_AveragePointy, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_AverageWide, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_NarrowNormal, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_NarrowPointy, MP_HeadTypeDefOf.Grunt_Male },
                { MP_HeadTypeDefOf.Female_NarrowWide, MP_HeadTypeDefOf.Grunt_Male }
            };

            HeadTypeCacheFemale = new Dictionary<HeadTypeDef, HeadTypeDef>()
            {
                { MP_HeadTypeDefOf.Female_AverageNormal, MP_HeadTypeDefOf.Grunt_Female },
                { MP_HeadTypeDefOf.Female_AveragePointy, MP_HeadTypeDefOf.Grunt_Female },
                { MP_HeadTypeDefOf.Female_AverageWide, MP_HeadTypeDefOf.Grunt_Female },
                { MP_HeadTypeDefOf.Female_NarrowNormal, MP_HeadTypeDefOf.Grunt_Female },
                { MP_HeadTypeDefOf.Female_NarrowPointy, MP_HeadTypeDefOf.Grunt_Female },
                { MP_HeadTypeDefOf.Female_NarrowWide, MP_HeadTypeDefOf.Grunt_Female }
            };

            if (ModsConfig.BiotechActive)
            {
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Gaunt, MP_HeadTypeDefOf.Grunt_Male_Gaunt);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Male_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Male_Heavy);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Female_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Male_Heavy);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Average1, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Average2, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Average3, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Narrow1, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Narrow2, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Narrow3, MP_HeadTypeDefOf.Grunt_Male_Furskin);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Gaunt, MP_HeadTypeDefOf.Grunt_Male_Furskin_Gaunt);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Heavy1, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Heavy2, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Furskin_Heavy3, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy);

                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Gaunt, MP_HeadTypeDefOf.Grunt_Female_Gaunt);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Female_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Female_Heavy);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Average1, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Average2, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Average3, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Narrow1, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Narrow2, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Narrow3, MP_HeadTypeDefOf.Grunt_Female_Furskin);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Gaunt, MP_HeadTypeDefOf.Grunt_Female_Furskin_Gaunt);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Heavy1, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Heavy2, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Furskin_Heavy3, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy);
            }

            if (ModsConfig.AnomalyActive)
            {
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Ghoul_Normal, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Ghoul_Heavy, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Ghoul_Narrow, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Ghoul_Wide, MP_HeadTypeDefOf.Grunt_Male_Heavy);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.CultEscapee, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.TimelessOne, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.DarkScholar_Female, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.DarkScholar_Male, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Leathery_Female, MP_HeadTypeDefOf.Grunt_Male);
                HeadTypeCacheMale.Add(MP_HeadTypeDefOf.Leathery_Male, MP_HeadTypeDefOf.Grunt_Male);

                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Ghoul_Normal, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Ghoul_Heavy, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Ghoul_Narrow, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Ghoul_Wide, MP_HeadTypeDefOf.Grunt_Female_Heavy);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.CultEscapee, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.TimelessOne, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.DarkScholar_Female, MP_HeadTypeDefOf.Grunt_Female);
                HeadTypeCacheFemale.Add(MP_HeadTypeDefOf.Leathery_Female, MP_HeadTypeDefOf.Grunt_Female);
            }
        }

    }
}
