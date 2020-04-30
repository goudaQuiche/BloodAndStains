using System;
using Verse;
using RimWorld;

namespace BloodDripping
{
    // <deathActionWorkerClass>BloodDripping.DeathActionWorker_BloodPuddle</deathActionWorkerClass>
    public class DeathActionWorker_BloodPuddle : DeathActionWorker
	{
        bool myDebug = false;
        
		public override void PawnDied(Corpse corpse)
		{
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