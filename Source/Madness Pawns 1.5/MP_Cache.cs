using System;
using System.Collections.Generic;
using Verse;

namespace Madness_Pawns
{
    public static class MP_Cache
    {
        public static Dictionary<HeadTypeDef,HeadTypeDef> HeadTypeCacheMale = new Dictionary<HeadTypeDef, HeadTypeDef>()
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
            { MP_HeadTypeDefOf.Female_NarrowWide, MP_HeadTypeDefOf.Grunt_Male },
            { MP_HeadTypeDefOf.Gaunt, MP_HeadTypeDefOf.Grunt_Male_Gaunt },
            { MP_HeadTypeDefOf.Male_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Male_Heavy },
            { MP_HeadTypeDefOf.Female_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Male_Heavy },
            { MP_HeadTypeDefOf.Furskin_Average1, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Average2, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Average3, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow1, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow2, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow3, MP_HeadTypeDefOf.Grunt_Male_Furskin },
            { MP_HeadTypeDefOf.Furskin_Gaunt, MP_HeadTypeDefOf.Grunt_Male_Furskin_Gaunt },
            { MP_HeadTypeDefOf.Furskin_Heavy1, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy },
            { MP_HeadTypeDefOf.Furskin_Heavy2, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy },
            { MP_HeadTypeDefOf.Furskin_Heavy3, MP_HeadTypeDefOf.Grunt_Male_Furskin_Heavy }
        };

        public static Dictionary<HeadTypeDef,HeadTypeDef> HeadTypeCacheFemale = new Dictionary<HeadTypeDef, HeadTypeDef>()
        {
            { MP_HeadTypeDefOf.Female_AverageNormal, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Female_AveragePointy, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Female_AverageWide, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Female_NarrowNormal, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Female_NarrowPointy, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Female_NarrowWide, MP_HeadTypeDefOf.Grunt_Female },
            { MP_HeadTypeDefOf.Gaunt, MP_HeadTypeDefOf.Grunt_Female_Gaunt },
            { MP_HeadTypeDefOf.Female_HeavyJawNormal, MP_HeadTypeDefOf.Grunt_Female_Heavy },
            { MP_HeadTypeDefOf.Furskin_Average1, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Average2, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Average3, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow1, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow2, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Narrow3, MP_HeadTypeDefOf.Grunt_Female_Furskin },
            { MP_HeadTypeDefOf.Furskin_Gaunt, MP_HeadTypeDefOf.Grunt_Female_Furskin_Gaunt },
            { MP_HeadTypeDefOf.Furskin_Heavy1, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy },
            { MP_HeadTypeDefOf.Furskin_Heavy2, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy },
            { MP_HeadTypeDefOf.Furskin_Heavy3, MP_HeadTypeDefOf.Grunt_Female_Furskin_Heavy }
        };
    }
}
