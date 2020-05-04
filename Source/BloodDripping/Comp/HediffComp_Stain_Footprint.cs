using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using UnityEngine;

using Verse;
using Verse.AI;

namespace BloodDripping
{
    public class HediffComp_Stain_Footprint : HediffComp
    {
        public Pawn myPawn = null;
        public Map myMap = null;

        public ThingDef moteFootprintDef = null;

        public  Vector3 lastFootprintPlacePos;
        public  bool lastFootprintRight;

        private int ticksUntilFootPrint;
        private int footPrintTicksLeft;
        private int stainedLengthTicks;
        public static readonly int StainedTicksLimit = 1800;
        public static readonly int LengthPerBloodFilth = 300;

        private IntVec3 lastCell;
        private IntVec3 lastDrawnFootprintCell;

        int FilthPerSteppedInItMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().FilthPerSteppedInItMultiplier;
        int MaxFilthCarriedMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().MaxFilthCarriedMultiplier;
        bool RedFootprintOnlyIfInjured = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().RedFootprintOnlyIfInjured;

        bool shouldSkip = false;

        public HediffCompProperties_Stain_Footprint Props
        {
            get
            {
                return (HediffCompProperties_Stain_Footprint)this.props;
            }
        }

        public override void CompPostMake()
        {
            Init();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (myPawn == null)
                Init();

            if (shouldSkip)
                return;

            if (myMap.moteCounter.SaturatedLowPriority)
            {
                Tools.Warn(myPawn?.LabelShort + "mote Counter Saturated", Props.debug);
                return;
            }
            if (!myPawn.Spawned)
            {
                Tools.Warn("unspawned pawn", Props.debug);
                return;
            }

            if (RedFootprintOnlyIfInjured && !myPawn.IsBleeding() && moteFootprintDef == MyDefs.HumanBloodyFootprint)
            {
                Tools.Warn(myPawn.LabelShort + " wont footprint : Modsetting + uninjured + HumanBloodyFootprint", Props.debug);
                return;
            }
                

            if (myPawn.IsLaying())
            {
                Tools.Warn(myPawn.LabelShort + " is laying, wont footprint", Props.debug);
                return;
            }

            if (myPawn.Position == lastCell)
            {
                CellScan();
            }
            lastCell = myPawn.Position;

            if ( !IsStained || lastDrawnFootprintCell == myPawn.Position)
                return;

            if (footPrintTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    Tools.Warn(myPawn.LabelShort + " trying to place bloody " + (Props.trailLikefootprint ? "trail" : "foot print"), Props.debug);

                    if (Props.trailLikefootprint)
                        this.TryPlaceTrailPrint();
                    else
                        this.TryPlaceFootprint();

                    lastDrawnFootprintCell = myPawn.Position;
                }

                Reset();
            }
            // decrease ticks
            else
            {
                footPrintTicksLeft--;
            }
            stainedLengthTicks--;

        }

        private void CellScan()
        {
            List<Thing> thingList = myPawn.Position.GetThingList(myMap);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing curT = thingList[i];
                
                if (curT.def.category != ThingCategory.Filth)
                    continue;
                
                foreach (Footprint curFP in Props.footprint)
                {
                    foreach(ThingDef curFilth in curFP.triggerOnFilthDef)
                    {
                        if(curT.def == curFilth)
                        {
                            stainedLengthTicks += LengthPerBloodFilth * FilthPerSteppedInItMultiplier;
                            moteFootprintDef = curFP.moteDef;
                            break;
                        }
                    }
                }
            }

            // Capping the ticks
            /* if (stainedLengthTicks > StainedTicksLimit)  stainedLengthTicks = StainedTicksLimit; */
            stainedLengthTicks = Math.Min(stainedLengthTicks, StainedTicksLimit * MaxFilthCarriedMultiplier);

        }

        public bool TerrainAllowsPuddle(Pawn pawn)
        {
            TerrainDef terrain = myPawn.Position.GetTerrain(myMap);
            //return (terrain == null || terrain.IsWater || myPawn.Map.snowGrid.GetDepth(myPawn.Position) >= 0.4f);
            return !(terrain == null || terrain.IsWater);
        }
        private bool IsStained
        {
            get
            {
                return (stainedLengthTicks > 0);
            }
        }
        public void Init()
        {
            myPawn = parent.pawn;
            myMap = myPawn.Map;

            if (myMap == null)
            {
                Tools.Warn("pawn is not on map anymore", Props.debug);
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            if (Props.footprint.NullOrEmpty())
            {
                Tools.Warn("no Footprint Def found, destroying hediff", Props.debug);
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            foreach(Footprint curFP in Props.footprint)
            {
                if (curFP.triggerOnFilthDef.NullOrEmpty())
                {
                    Tools.Warn("no Filth Def found in "+ curFP.defName, Props.debug);
                    parent.Severity = 0;
                    shouldSkip = true;
                    return;
                }
            }

            if (myPawn.GetStatValue(StatDefOf.MoveSpeed) < MyDefs.HumanSpeed)
                ticksUntilFootPrint = Props.period;
            else
                ticksUntilFootPrint = (int)(Props.period / (myPawn.GetStatValue(StatDefOf.MoveSpeed) / MyDefs.HumanSpeed));

            lastCell = myPawn.Position;
        }
        void Reset()
        {
            footPrintTicksLeft = ticksUntilFootPrint;
            if (myPawn.Drafted)
                footPrintTicksLeft /= 2;
        }

        string DumpTriggeringFilths()
        {
            string result = string.Empty;
            result = "tFp list: ";
            foreach (Footprint fp in Props.footprint)
            {
                result += fp.defName + "; ";
            }
            return result;
        }


        public override string CompTipStringExtra
        {
            get
            {
                string result = string.Empty;

                if (Props.debug)
                {
                    result += "IsBloodStained: " + IsStained
                    + "\n moteSaturated" + myMap.moteCounter.SaturatedLowPriority
                    + "\n "+DumpTriggeringFilths();

                    if (IsStained)
                        result +=
                        "\n BloodStainedLengthTicks: " + stainedLengthTicks
                        + "\n Ticks: " + footPrintTicksLeft + "/" + ticksUntilFootPrint
                        + "\n MoteFootprint: " + moteFootprintDef?.defName;
                }

                return result;
            }
        }
    }
}
