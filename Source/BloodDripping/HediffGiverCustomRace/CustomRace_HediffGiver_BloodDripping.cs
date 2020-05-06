using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace BloodDripping
{
    public class CustomRace_HediffGiver_BloodDripping : HediffGiver
    {
        bool SafeRemoval = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().SafeRemoval;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (SafeRemoval) return;

            if (pawn == null || pawn.Map == null || !pawn.Spawned)
                return;

            string myPawnResume = pawn.LabelShort + "(" + pawn.def.defName + ")";
            string myHediffDesc = "custom blood dripping";

            bool Is_BloodDripping_Hediff = ToolsComp.Get_BloodDripping_HediffComp_debug_and_race(hediff, out bool debug, out string raceDefName);

            if (!Is_BloodDripping_Hediff)
            {
                Tools.Warn(myPawnResume + " calling hediff(" + hediff?.defName + ") is wrong ", debug);
                return;
            }

            if (!pawn.IsRaceMember(raceDefName))
            {
                Tools.Warn(myPawnResume + " is not race member of " + raceDefName, debug);
                return;
            }

            if (pawn.HasHediff(hediff))
            {
                //Tools.Warn(myPawnResume + " already has " + myHediffDesc, debug);
                return;
            }

            Tools.Warn(myPawnResume + " => got " + myHediffDesc + " applied on ", debug);
            HealthUtility.AdjustSeverity(pawn, hediff, .1f);
        }
    }
}
