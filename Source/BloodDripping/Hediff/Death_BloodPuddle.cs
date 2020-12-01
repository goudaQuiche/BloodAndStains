using System;
using Verse;
using RimWorld;

namespace BloodDripping
{
    public static class UponDeath
	{
		public static void DeathPuddle(Corpse corpse)
		{
            bool myDebug = false;

            if (corpse == null || corpse.Map == null || !corpse.Spawned)
            {
                Tools.Warn("DeathPuddle - no corpse", myDebug);
                return;
            }

            Tools.Warn(corpse.InnerPawn.LabelShort + " entering DeathActionWorker_BloodPuddle.PawnDied", myDebug);

            Hediff BloodDrippingHediff = ToolsPawn.Get_Human_BloodDripping(corpse.InnerPawn);
            if (BloodDrippingHediff == null)
            {
                Tools.Warn("Could not find human bloodripping Hediff", myDebug);
                // not human pawn
                BloodDrippingHediff = ToolsPawn.Get_Custom_BloodDripping_Hediff(corpse.InnerPawn);
                if(BloodDrippingHediff == null)
                {
                    Tools.Warn("Could not find back up bloodripping Hediff: ", myDebug);
                    return;
                }
            }

            HediffComp_BloodDripping hediffComp_BloodDripping = BloodDrippingHediff.TryGetComp<HediffComp_BloodDripping>();
            if (BloodDrippingHediff == null)
            {
                Tools.Warn("Could not find HediffComp_BloodDripping", myDebug);
                return;
            }

            myDebug = hediffComp_BloodDripping.Props.debug;

            bool WasBleedingBeforeDeath = hediffComp_BloodDripping.WasBleeding;
            Tools.Warn(corpse.InnerPawn.LabelShort + " was "+(WasBleedingBeforeDeath?"":"not ")+"bleeding before death", myDebug);

            if (WasBleedingBeforeDeath)
                hediffComp_BloodDripping.TryPlaceDeathMote();
        }
	}
}