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
        Pawn myPawn = null;
        Map myMap = null;

        ThingDef moteFootprintDef = null;

        private Vector3 lastFootprintPlacePos;
        private bool lastFootprintRight;
        private const float FootprintIntervalDist = 0.632f;
        private static readonly Vector3 FootprintOffset = new Vector3(0f, 0f, -0.3f);
        private const float LeftRightOffsetDist = 0.17f;
        private const float FootprintSplashSize = 2f;

        private int ticksUntilFootPrint;
        private int footPrintTicksLeft;
        private int stainedLengthTicks;
        private readonly int StainedTicksLimit = 10000;
        private int LengthPerBloodFilth = 1000;

        private IntVec3 lastCell;

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

            if (myPawn.IsLaying())
            {
                Tools.Warn(myPawn.LabelShort + " is laying, wont footprint", Props.debug);
                return;
            }

            if (myPawn.Position == lastCell)
                CellScan();

            lastCell = myPawn.Position;

            if (!IsStained)
                return;

            if (footPrintTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    Tools.Warn(myPawn.LabelShort + " trying to place bloody foot print", Props.debug);
                    TryPlaceFootprint();
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
                /*
                if (!curT.def.IsWithinCategory(MyDefs.FilthCategoryDef))
                    continue;
                */

                foreach (Footprint curFP in Props.footprint)
                {
                    foreach(ThingDef curFilth in curFP.triggerOnFilthDef)
                    {
                        if(curT.def == curFilth)
                        {
                            stainedLengthTicks += LengthPerBloodFilth;
                            moteFootprintDef = curFP.moteDef;
                            break;
                        }
                    }
                }
            }

            if (stainedLengthTicks > StainedTicksLimit)
                stainedLengthTicks = StainedTicksLimit;

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

            if (Props.footprint.NullOrEmpty())
            {
                Tools.Warn("no Footprint Def found, destroying hediff", Props.debug);
                parent.Severity = 0;
            }

            foreach(Footprint curFP in Props.footprint)
            {
                if (curFP.triggerOnFilthDef.NullOrEmpty())
                {
                    Tools.Warn("no Filth Def found in "+ curFP.defName, Props.debug);
                    parent.Severity = 0;
                }
            }

            ticksUntilFootPrint = (int)(Props.period / (myPawn.GetStatValue(StatDefOf.MoveSpeed) / MyDefs.HumanSpeed));
            lastCell = myPawn.Position;
        }
        void Reset()
        {
            footPrintTicksLeft = ticksUntilFootPrint;
        }

        public static void PlaceFootprint(Vector3 loc, Map map, float rot, ThingDef Mote_FootprintDef)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Mote_FootprintDef, null);
            moteThrown.Scale = 0.5f;
            moteThrown.exactRotation = rot;
            moteThrown.exactPosition = loc;
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }
        private void TryPlaceFootprint()
        {

            Vector3 drawPos = myPawn.Drawer.DrawPos;
            Vector3 normalized = (drawPos - lastFootprintPlacePos).normalized;
            float rot = normalized.AngleFlat();
            float angle = (float)((!lastFootprintRight) ? -90 : 90);
            Vector3 b = normalized.RotatedBy(angle) * 0.17f * Mathf.Sqrt(myPawn.BodySize);
            Vector3 vector = drawPos + FootprintOffset + b;
            IntVec3 c = vector.ToIntVec3();
            if (c.InBounds(myMap))
            {
                TerrainDef terrain = c.GetTerrain(myPawn.Map);
                if (terrain != null)
                {
                    PlaceFootprint(vector, myMap, rot, moteFootprintDef);
                }
            }
            lastFootprintPlacePos = drawPos;
            lastFootprintRight = !lastFootprintRight;
        }

        public override string CompTipStringExtra
        {
            get
            {
                string result = string.Empty;
                if (Props.debug)
                {
                    if (Props.debug)
                    {
                        result += "IsBloodStained: " + IsStained
                        + "\n moteSaturated" + myMap.moteCounter.SaturatedLowPriority;

                        if (IsStained)
                            result +=
                            "\n BloodStainedLengthTicks: " + stainedLengthTicks
                            + "\n Ticks: " + footPrintTicksLeft + "/" + ticksUntilFootPrint
                            + "\n MoteFootprint: " + moteFootprintDef?.defName;
                    }
                }
                return result;
            }
        }
    }
}
