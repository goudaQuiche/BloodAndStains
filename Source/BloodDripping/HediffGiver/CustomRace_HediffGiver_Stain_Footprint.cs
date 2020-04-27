using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace BloodDripping
{
    public class CustomRace_HediffGiver_Stain_Footprint : HediffGiver
    {

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            //bool flag = pawn != null && pawn.Map != null && pawn.Spawned && pawn.IsRaceMember(letterLa) &&  !pawn.HasHediff(hediff);
            bool flag = pawn != null && pawn.Map != null && pawn.Spawned && !pawn.HasHediff(hediff);
            if (flag)
            {
                //Tools.Warn("Applying custom stain footprint on " + pawn.LabelShort + "(" + pawn.def.defName + ")", true);
                HealthUtility.AdjustSeverity(pawn, this.hediff, .1f);
            }
        }
        
    }
}
