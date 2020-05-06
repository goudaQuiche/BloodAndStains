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

        public ThingDef moteRightFootprintDef = null;
        public ThingDef moteLeftFootprintDef = null;

        public  Vector3 lastFootprintPlacePos;
        public  bool lastFootprintRight;

        private int ticksUntilFootPrint;
        private int footPrintTicksLeft;
        private int stainedLengthTicks;
        public static readonly int StainedTicksLimit = 1800;
        public static readonly int LengthPerBloodFilth = 300;

        private IntVec3 lastCell;
        private IntVec3 lastDrawnFootprintCell;

        Hediff LeftLegHediff = null;
        Hediff RightLegHediff = null;
        public bool availableDisabilityMotes = false;

        readonly int FilthPerSteppedInItMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().FilthPerSteppedInItMultiplier;
        readonly int MaxFilthCarriedMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().MaxFilthCarriedMultiplier;
        readonly bool RedFootprintOnlyIfInjured = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().RedFootprintOnlyIfInjured;

        readonly bool SafeRemoval = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().SafeRemoval;

        bool shouldSkip = false;

        public HediffCompProperties_Stain_Footprint Props
        {
            get
            {
                return (HediffCompProperties_Stain_Footprint)props;
            }
        }

        public override void CompPostMake()
        {
            Init();
        }

        bool HasLeftLegHediff
        {
            get
            {
                return LeftLegHediff != null;
            }
        }
        bool HasRightLegHediff
        {
            get
            {
                return RightLegHediff != null;
            }
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

            if (
                RedFootprintOnlyIfInjured && !myPawn.IsBleeding() &&
                (moteFootprintDef == MyDefs.HumanBloodyFootprint) || (moteFootprintDef == MyDefs.HumanBloodyPegleg) || (moteFootprintDef == MyDefs.HumanBloodyWoodenfoot)
                )
            {
                Tools.Warn(myPawn.LabelShort + " wont footprint : Modsetting + uninjured + HumanBloodyFootprint", Props.debug);
                return;
            }

            if (Tools.TrueEvery30Sec)
                if (availableDisabilityMotes)
                {
                    LeftLegHediff = myPawn.GetLeftLegFirstRelevantHediff(Props.debug);
                    RightLegHediff = myPawn.GetRightLegFirstRelevantHediff(Props.debug);
                }
                else
                    Tools.Warn(myPawn.LabelShort + " - Disability motes are disabled, no disability mote update", Props.debug);

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

            if (!IsStained || lastDrawnFootprintCell == myPawn.Position)
                return;

            if (footPrintTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    string moteName = string.Empty;
                    if (HasDisabilityHediff && availableDisabilityMotes)
                        moteName = (lastFootprintRight ? moteRightFootprintDef?.defName : moteLeftFootprintDef?.defName);
                    else
                        moteName = moteFootprintDef?.defName;
                    Tools.Warn(myPawn.LabelShort + " trying to place bloody " + (Props.trailLikeFootprint ? "trail" : "foot print") + ": " + moteName, Props.debug);

                    this.TryPlaceFootprint(Props.trailLikeFootprint);
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
        public bool HasRotation
        {
            get
            {
                return (Props.leftFootRotation != 0 || Props.rightFootRotation != 0);
            }
        }
        public bool HasDisabilityHediff
        {
            get
            {
                return HasLeftLegHediff || HasRightLegHediff;
            }
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
                    foreach (ThingDef curFilth in curFP.triggerOnFilthDef)
                    {
                        if (curT.def == curFilth)
                        {
                            stainedLengthTicks += LengthPerBloodFilth * FilthPerSteppedInItMultiplier;

                            moteFootprintDef = curFP.moteDef;
                            if (Props.disabilityFootprints && availableDisabilityMotes && HasDisabilityHediff)
                            {
                                moteLeftFootprintDef = moteRightFootprintDef = moteFootprintDef;
                                if (HasLeftLegHediff)
                                    moteLeftFootprintDef = this.RegularMoteToDisabilityHediffMote(LeftLegHediff, curFP.moteDef, Props.debug);
                                if (HasRightLegHediff)
                                    moteRightFootprintDef = this.RegularMoteToDisabilityHediffMote(RightLegHediff, curFP.moteDef, Props.debug);

                                Tools.Warn(myPawn.LabelShort + " has disability: " + HasDisabilityHediff + " - Left: " + LeftLegHediff?.def.defName + "; Right: " + RightLegHediff?.def.defName, Props.debug);
                                Tools.Warn(myPawn.LabelShort + " motes: Left: " + moteLeftFootprintDef?.defName + "; Right: " + moteRightFootprintDef?.defName, Props.debug);
                            }

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

        bool CheckBaseConf
        {
            get
            {
                //if (!myPawn.IsHuman() && Props.race.NullOrEmpty())
                if (Props.race.NullOrEmpty())
                {
                    Log.Error("no race set in the footprint hediffCompProps, you need one for an alien race, destroying hediff");

                    return false;
                }
                else Tools.Warn("race set to: " + Props.race, Props.debug);

                if (Props.footprint.NullOrEmpty())
                {
                    Tools.Warn("no Footprint Def found, destroying hediff", Props.debug);
                    return false;
                }
                else Tools.Warn("set footprint number: " + Props.footprint.Count, Props.debug);

                foreach (Footprint curFP in Props.footprint)
                {
                    if (curFP.triggerOnFilthDef.NullOrEmpty())
                    {
                        Tools.Warn("no Filth Def found in " + curFP.defName, Props.debug);
                        return false;
                    }
                    else Tools.Warn("Found: " + curFP.defName, Props.debug);
                }

                return true;
            }
        }

        void CheckDisabilitiesConf()
        {
            if (!Props.peglegMote_pattern.NullOrEmpty() && !Props.woodenfootMote_pattern.NullOrEmpty() && !Props.missingPartMote_pattern.NullOrEmpty())
            {
                availableDisabilityMotes = this.CheckDisabilityMotes(Props.debug);
            }
            Tools.Warn("Trying to disability footprints: "+ Props.disabilityFootprints + " - Available disability motes: " + availableDisabilityMotes, Props.debug);
        }

        public void Init()
        {
            myPawn = parent.pawn;
            myMap = myPawn.Map;

            if (SafeRemoval)
            {
                Log.Warning("SafeModRemoval activated");
                parent.Severity = 0;
                shouldSkip = true;
                parent.PostRemoved();
                return;
            }

            if (myMap == null)
            {
                Tools.Warn("pawn is not on map anymore", Props.debug);
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            if (!CheckBaseConf)
            {
                parent.Severity = 0;
                shouldSkip = true;
                parent.PostRemoved();
            }

            if(Props.disabilityFootprints)
                CheckDisabilitiesConf();

            if (myPawn.GetStatValue(StatDefOf.MoveSpeed) < MyDefs.HumanSpeed)
                ticksUntilFootPrint = Props.period;
            else
                ticksUntilFootPrint = (int)(Props.period / (myPawn.GetStatValue(StatDefOf.MoveSpeed) / MyDefs.HumanSpeed));

            lastCell = myPawn.Position;

            Tools.Warn(myPawn.LabelShort+" passed " + Def.defName + " Init()\n--------", Props.debug);

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
