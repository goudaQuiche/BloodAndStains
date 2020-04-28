using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace BloodDripping
{
    public class Human_HediffGiver_Stain_Footprint : HediffGiver
    {
        
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            bool flag = pawn != null && pawn.Map != null && pawn.Spawned && pawn.IsHuman() && !pawn.Has_Human_Stain_Footprint();

            if (flag)
            {
                //Tools.Warn("Applying blood footprint on " + pawn.LabelShort, true);
                HealthUtility.AdjustSeverity(pawn, this.hediff, .1f);
            }
        }
        
    }
}
