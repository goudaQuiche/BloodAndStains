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
        private int BloodStainedLengthTicks;
        private readonly int BloodStainedLimit = 10000;
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
            if (myMap.moteCounter.SaturatedLowPriority)
                return;

            if (myPawn == null || !myPawn.Spawned)
            {
                //Tools.Warn("pawn null", Props.debug);
                return;
            }
            if (myPawn.Map == null)
            {
                //Tools.Warn(myPawn.Label + " - pawn.Map null", myDebug);
                return;
            }

            if(myPawn.IsLaying())
                return;

            CellScan();

            if (!IsBloodStained)
                return;

            if (footPrintTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    Tools.Warn("Trying to place bloody foot print", Props.debug);
                    TryPlaceFootprint();
                }

                Reset();
            }
            // decrease ticks
            else
            {
                footPrintTicksLeft--;
            }
            BloodStainedLengthTicks--;

        }
        private void CellScan()
        {
            if (myPawn.Position == lastCell)
                return;

            List<Thing> thingList = myPawn.Position.GetThingList(myMap);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing curT = thingList[i];
                /*
                if (!curT.def.IsWithinCategory(MyDefs.FilthCategoryDef))
                    continue;
                */

                if (curT.def.defName == "Filth_Blood")
                {
                    BloodStainedLengthTicks += LengthPerBloodFilth;
                    moteFootprintDef = Props.mote_HumanBlood_FootprintDef;
                    break;
                }else if (curT.def.defName == "Filth_BloodInsect"){
                    BloodStainedLengthTicks += LengthPerBloodFilth;
                    moteFootprintDef = Props.mote_InsectBlood_FootprintDef;
                    break;
                }
                else if (curT.def.defName == "Filth_Vomit")
                {
                    BloodStainedLengthTicks += LengthPerBloodFilth;
                    moteFootprintDef = Props.mote_Vomit_FootprintDef;
                    break;
                }else if (curT.def.defName == "Filth_AnimalFilth")
                {
                    BloodStainedLengthTicks += LengthPerBloodFilth;
                    moteFootprintDef = Props.mote_Filth_FootprintDef;
                    break;
                }
            }

            if (BloodStainedLengthTicks > BloodStainedLimit)
                BloodStainedLengthTicks = BloodStainedLimit;

            lastCell = myPawn.Position;
        }

        public bool TerrainAllowsPuddle(Pawn pawn)
        {
            TerrainDef terrain = myPawn.Position.GetTerrain(myMap);
            //return (terrain == null || terrain.IsWater || myPawn.Map.snowGrid.GetDepth(myPawn.Position) >= 0.4f);
            return !(terrain == null || terrain.IsWater);
        }
        private bool IsBloodStained
        {
            get
            {
                return (BloodStainedLengthTicks > 0);
            }
        }
        public void Init()
        {
            myPawn = parent.pawn;
            myMap = myPawn.Map;

            if ((Props.mote_Filth_FootprintDef == null) || (Props.mote_InsectBlood_FootprintDef == null) || (Props.mote_HumanBlood_FootprintDef == null) || (Props.mote_Vomit_FootprintDef == null))
            {
                Tools.Warn("no moteFootprintDef found, destroying hediff", Props.debug);
                parent.Severity = 0;
            }

            //moteFootprintDef = Props.mote_HumanBlood_FootprintDef;
            ticksUntilFootPrint = Props.period;

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
                        result += "IsBloodStained: " + IsBloodStained +
                            "\n BloodStainedLengthTicks: " + BloodStainedLengthTicks +
                            "\n Ticks: " + footPrintTicksLeft + "/" + ticksUntilFootPrint +
                            "\n MoteFootprint: " + moteFootprintDef?.defName;
                    }
                }
                return result;
            }
        }
    }
}
