using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System;

namespace BloodDripping
{
    public static class MyDefs
    {
        public static HediffDef Human_Red_BloodDripping_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "Hediff_Red_BloodDripping").First();
        public static HediffDef Human_Stain_Footprint_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "Hediff_Human_Stain_Footprint").First();

        public static IEnumerable<HediffDef> Custom_BloodDripping_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => (b.defName.Contains("Hediff_") && b.defName.Contains("BloodDripping")) );

        /*
        public static HediffDef Human_Red_BloodDripping_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "BNS_Hediff_BloodDripping_Red").First();
        public static HediffDef Human_Stain_Footprint_HediffDef = DefDatabase<HediffDef>.AllDefs.Where((HediffDef b) => b.defName == "BNS_Hediff_Stain_Footprint_Human").First();
        */

        public static float HumanSpeed = DefDatabase<ThingDef>.AllDefs.Where((ThingDef h) => h.defName == "Human").First().statBases.GetStatOffsetFromList(StatDefOf.MoveSpeed);
    }
}
