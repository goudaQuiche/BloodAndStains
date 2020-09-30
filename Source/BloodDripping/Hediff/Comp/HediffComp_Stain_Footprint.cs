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
        //public Pawn myPawn = null;
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

        int mySeed;

        readonly int FilthPerSteppedInItMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().FilthPerSteppedInItMultiplier;
        readonly int MaxFilthCarriedMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().MaxFilthCarriedMultiplier;
        readonly bool RedFootprintOnlyIfInjured = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().RedFootprintOnlyIfInjured;

        readonly bool SafeRemoval = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().SafeRemoval;

        bool shouldSkip = false;
        bool IsInitialized = false;

        public HediffCompProperties_Stain_Footprint Props => (HediffCompProperties_Stain_Footprint)props;
        bool MyDebug => Props.debug;

        bool HasLeftLegDisability => LeftLegHediff != null;
        bool HasRightLegDisability => RightLegHediff != null;
        public bool HasDisability => HasLeftLegDisability || HasRightLegDisability;

        public bool HasRotation => Props.leftFootRotation != 0 || Props.rightFootRotation != 0;

        private bool IsStained => stainedLengthTicks > 0;

        public override void CompPostMake()
        {
            //Log.Warning("CompPostMake Init");
            Init();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!IsInitialized)
                Init();

            if (shouldSkip)
                return;

            if (myMap.moteCounter.SaturatedLowPriority)
            {
                Tools.Warn(Pawn?.LabelShort + "mote Counter Saturated", MyDebug);
                return;
            }

            if (!Pawn.Spawned)
            {
                Tools.Warn("unspawned pawn", MyDebug);
                return;
            }

            if (
                RedFootprintOnlyIfInjured && !Pawn.IsBleeding() &&
                (moteFootprintDef == MyDefs.HumanBloodyFootprint) || (moteFootprintDef == MyDefs.HumanBloodyPegleg) || (moteFootprintDef == MyDefs.HumanBloodyWoodenfoot)
                )
            {
                Tools.Warn(Pawn.LabelShort + " wont footprint : Modsetting + uninjured + HumanBloodyFootprint", MyDebug);
                return;
            }

            //if (Tools.TrueEvery30Sec)
            if (Tools.TrueEvery15SecPlusSeed(mySeed))
            {
                if (availableDisabilityMotes)
                {
                    LeftLegHediff = Pawn.GetLeftLegFirstRelevantHediff(MyDebug);
                    RightLegHediff = Pawn.GetRightLegFirstRelevantHediff(MyDebug);
                }
                else
                    Tools.Warn(Pawn.LabelShort + " - Disability motes are disabled, no disability mote update", Prefs.DevMode && MyDebug);
                //Tools.Warn(myPawn.LabelShort + " - Disability motes are disabled, no disability mote update", Props.debug);
            }

            if (Pawn.IsLaying())
            {
                Tools.Warn(Pawn.LabelShort + " is laying, wont footprint", MyDebug);
                return;
            }

            //if (myPawn.Position == lastCell)
            if (Pawn.Position != lastCell)
            {
                CellScan();
            }
            lastCell = Pawn.Position;

            if (!IsStained || lastDrawnFootprintCell == Pawn.Position)
                return;

            if (footPrintTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(Pawn))
                {
                    string moteName = string.Empty;
                    if (HasDisability && availableDisabilityMotes)
                        moteName = (lastFootprintRight ? moteRightFootprintDef?.defName : moteLeftFootprintDef?.defName);
                    else
                        moteName = moteFootprintDef?.defName;
                    Tools.Warn(Pawn.LabelShort + " trying to place bloody " + (Props.trailLikeFootprint ? "trail" : "foot print") + ": " + moteName, MyDebug);

                    this.TryPlaceFootprint();
                    lastDrawnFootprintCell = Pawn.Position;
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
            if (myMap == null || !Pawn.Position.InBounds(myMap))
                return;

            List<Thing> thingList = Pawn.Position.GetThingList(myMap);
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
                            if (Props.disabilityFootprints && availableDisabilityMotes && HasDisability)
                            {
                                moteLeftFootprintDef = moteRightFootprintDef = moteFootprintDef;
                                if (HasLeftLegDisability)
                                    moteLeftFootprintDef = this.RegularMoteToDisabilityHediffMote(LeftLegHediff, curFP.moteDef, MyDebug);
                                if (HasRightLegDisability)
                                    moteRightFootprintDef = this.RegularMoteToDisabilityHediffMote(RightLegHediff, curFP.moteDef, MyDebug);

                                Tools.Warn(Pawn.LabelShort + " has disability: " + HasDisability + " - Left: " + LeftLegHediff?.def.defName + "; Right: " + RightLegHediff?.def.defName, Props.debug);
                                Tools.Warn(Pawn.LabelShort + " motes: Left: " + moteLeftFootprintDef?.defName + "; Right: " + moteRightFootprintDef?.defName, MyDebug);
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
            TerrainDef terrain = Pawn.Position.GetTerrain(myMap);
            //return (terrain == null || terrain.IsWater || myPawn.Map.snowGrid.GetDepth(myPawn.Position) >= 0.4f);
            return !(terrain == null || terrain.IsWater);
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
                else Tools.Warn("race set to: " + Props.race, MyDebug);

                if (Props.footprint.NullOrEmpty())
                {
                    Tools.Warn("no Footprint Def found, destroying hediff", MyDebug);
                    return false;
                }
                else Tools.Warn("set footprint number: " + Props.footprint.Count, MyDebug);

                foreach (Footprint curFP in Props.footprint)
                {
                    if (curFP.triggerOnFilthDef.NullOrEmpty())
                    {
                        Tools.Warn("no Filth Def found in " + curFP.defName, MyDebug);
                        return false;
                    }
                    else Tools.Warn("Found: " + curFP.defName, MyDebug);
                }

                return true;
            }
        }

        void CheckDisabilitiesConf()
        {
            if (!Props.peglegMote_pattern.NullOrEmpty() && !Props.woodenfootMote_pattern.NullOrEmpty() && !Props.missingPartMote_pattern.NullOrEmpty())
            {
                availableDisabilityMotes = this.CheckDisabilityMotes(MyDebug);
            }
            Tools.Warn("Trying to disability footprints: "+ Props.disabilityFootprints + " - Available disability motes: " + availableDisabilityMotes, MyDebug);
        }

        public void Init()
        {
            //myPawn = parent.pawn;
            myMap = Pawn.Map;

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
                Tools.Warn("pawn is not on map anymore", MyDebug);
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

            if (Props.disabilityFootprints)
            {
                CheckDisabilitiesConf();
                if (availableDisabilityMotes)
                {
                    LeftLegHediff = Pawn.GetLeftLegFirstRelevantHediff(MyDebug);
                    RightLegHediff = Pawn.GetRightLegFirstRelevantHediff(MyDebug);
                }
            }
                

            if (Pawn.GetStatValue(StatDefOf.MoveSpeed) < MyDefs.HumanSpeed)
                ticksUntilFootPrint = Props.period;
            else
                ticksUntilFootPrint = (int)(Props.period / (Pawn.GetStatValue(StatDefOf.MoveSpeed) / MyDefs.HumanSpeed));

            lastCell = Pawn.Position;
            mySeed = Pawn.thingIDNumber % 60;

            IsInitialized = true;

            Tools.Warn(Pawn.LabelShort+" passed " + Def.defName + " Init()\n--------", MyDebug);

        }

        void Reset()
        {
            footPrintTicksLeft = ticksUntilFootPrint;
            if (Pawn.Drafted)
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

                //if (Props.debug)
                if (Prefs.DevMode)
                {
                    result += "IsBloodStained: " + IsStained
                    + "\n moteSaturated" + myMap.moteCounter.SaturatedLowPriority
                    + "\n " + DumpTriggeringFilths();

                    if (IsStained)
                        result +=
                        "\n BloodStainedLengthTicks: " + stainedLengthTicks
                        + "\n Ticks: " + footPrintTicksLeft + "/" + ticksUntilFootPrint
                        + "\n MoteFootprint: " + moteFootprintDef?.defName;

                    if (HasDisability)
                    {
                        result +=
                            "\n availableDisability: " + availableDisabilityMotes +
                            "\n leftleg: " + HasLeftLegDisability + "; rightleg:" + HasRightLegDisability;
                    }
                }

                return result;
            }
        }
    }
}
