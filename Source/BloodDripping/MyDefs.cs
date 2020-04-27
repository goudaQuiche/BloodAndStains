using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace BloodDripping
{
    public static class MyDefs
    {
        public static HediffDef Human_Red_BloodDripping_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "Hediff_Red_BloodDripping").First();
        public static HediffDef Human_Human_Stain_Footprint_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "Hediff_Human_Stain_Footprint").First();

        //public static ThingCategoryDef FilthCategoryDef = DefDatabase<ThingCategoryDef>.AllDefs.Where((ThingCategoryDef b) => b.defName == "Filth").First();
    }
}
