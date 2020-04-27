using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace BloodDripping
{
    public class CustomRace_HediffGiver_BloodDripping : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            //bool flag = (pawn != null && pawn.Map != null && pawn.Spawned && pawn.IsHuman() && !pawn.HasHediff(hediff));
            bool flag = pawn != null && pawn.Map != null && pawn.Spawned && !pawn.HasHediff(hediff);
            if (flag)
            {
                //Tools.Warn("Applying blood dripping on " + pawn.LabelShort, true);
                HealthUtility.AdjustSeverity(pawn, this.hediff, .1f);
            }
        }
    }
}
