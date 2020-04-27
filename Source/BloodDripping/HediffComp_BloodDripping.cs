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
    public class HediffComp_BloodDripping : HediffComp
    {
        Pawn myPawn = null;
        Map myMap = null;

        List <ThingDef> puddleMoteDef = null;
        private Vector3 lastPuddlePlacePos;
        //private static readonly Vector3 BloodPuddleOffset = new Vector3(0f, 0f, -0.3f);
        private static readonly Vector3 StandingOffset = new Vector3(0f, 0f, -0.5f);
        //private static readonly Vector3 Downedffset = new Vector3(0f, 0f, 0);

        private int ticksUntilPuddle = 500;
        private int puddleTicksLeft;

        private float bleedRateTotal;
        private float bleedingScale;
        private int layingTicks;
        private int layingTicksLimit = 1800;
        private float layingFactor;
        //private Job oldJob = null;

        //60 = 1 sec
        private int CheckEveryXTicks = 90;

        public HediffCompProperties_BloodDripping Props
        {
            get
            {
                return (HediffCompProperties_BloodDripping)this.props;
            }
        }

        public override void CompPostMake()
        {
            Init();
        }

        public bool TerrainAllowsPuddle(Pawn pawn)
        {
            TerrainDef terrain = myPawn.Position.GetTerrain(myMap);
            //return (terrain == null || terrain.IsWater || myPawn.Map.snowGrid.GetDepth(myPawn.Position) >= 0.4f);
            return !(terrain == null || terrain.IsWater);
        }

        public bool IsBleeding
        {
            get
            {
                return myPawn.IsBleeding();
            }
        }

        public void CalculatePuddle()
        {

            bleedRateTotal = myPawn.health.hediffSet.BleedRateTotal;
            bleedingScale = bleedRateTotal * .75f;
            if (bleedRateTotal == 0)
            {
                Tools.Warn(
                    myPawn.LabelShort + "'s bloodRate= " + bleedRateTotal +
                    "\nShould not happen since HasHediff(HediffDefOf.BloodLoss)", Props.debug);
                return;
            }
            ticksUntilPuddle = (int)(Props.period / bleedRateTotal);

            if (myPawn.IsLaying())
            {
                layingTicks++;
                if (layingTicks > layingTicksLimit)
                    layingTicks = layingTicksLimit;

                layingFactor = (float)layingTicks / (float)layingTicksLimit;

                bleedingScale *= 1.1f + layingFactor;
                ticksUntilPuddle *= 2 + (int)(2 * layingFactor);
            }
            else
            {
                layingTicks = 0;
            }

            Tools.Warn(
                "New bleed rate: " + bleedRateTotal +
                "; bleedingScale: " + bleedingScale +
                "; ticksUntilPuddle: " + ticksUntilPuddle
                , Props.debug);

        }

        public void Ticking()
        {
            // period reached, actually doing something
            if (puddleTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    Tools.Warn("Trying to place bloody puddle", Props.debug);
                    TryPlaceMote();
                }

                // even if we did not place a mote because the terrain did not allow it, the step still virtually happened
                Reset();
            }
            // counting ticks until next step
            else
            {
                //if ismoving
                puddleTicksLeft--;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (myMap.moteCounter.SaturatedLowPriority)
                return;

            if (myPawn == null || !myPawn.Spawned)
            {
                Tools.Warn("pawn null", Props.debug);
                return;
            }
            if (myPawn.Map == null)
            {
                //Tools.Warn(myPawn.Label + " - pawn.Map null", myDebug);
                return;
            }

            // no bleeding, no need to go further
            if (!IsBleeding)
                return;

            bool everySec = (Find.TickManager.TicksGame % CheckEveryXTicks == 0);
            if (everySec)
                CalculatePuddle();

            Ticking();
        }

        public void Init()
        {
            myPawn = parent.pawn;
            myMap = myPawn.Map;

            if (Props.puddleMoteDef.NullOrEmpty())
            {
                Tools.Warn("no Props.puddleMoteDef found, destroying hediff", Props.debug);
                parent.Severity = 0;
            }
                
            puddleMoteDef = Props.puddleMoteDef;
            ticksUntilPuddle = Props.period;
        }
        void Reset()
        {
            puddleTicksLeft = ticksUntilPuddle;
        }

        public static void PlacePuddle(Vector3 loc, Map map, float rot, float scale, ThingDef Mote_FootprintDef)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Mote_FootprintDef, null);
            moteThrown.Scale = scale;
            moteThrown.exactRotation = rot;
            moteThrown.exactPosition = loc;
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        private float PuddleSize()
        {
            //return Props.scale.RandomInRange * bleedingScale * myPawn.BodySize * myPawn.GetBloodPumping();
            return Props.scale.RandomInRange * bleedingScale * myPawn.GetBloodPumping();
        }

        private void TryPlaceMote()
        {

            Vector3 drawPos = myPawn.Drawer.DrawPos;
            Vector3 normalized = drawPos.normalized;
            float rot = normalized.AngleFlat();

            Vector3 vector = drawPos;
            if (!myPawn.IsLaying())
                vector += StandingOffset;

            IntVec3 c = vector.ToIntVec3();
            if (c.InBounds(myMap))
            {
                TerrainDef terrain = c.GetTerrain(myPawn.Map);
                if (terrain != null)
                {
                    PlacePuddle(vector, myMap, rot, PuddleSize(), puddleMoteDef.RandomElement());
                }
            }
            lastPuddlePlacePos = drawPos;
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
                        result += "Bleeding rate: " + bleedRateTotal
                            + "\n ticks: " + puddleTicksLeft + "/" + ticksUntilPuddle
                            + "\n bleedingScale: " + bleedingScale
                            + "\n layingFactor: " + layingFactor
                            //+ "\n BodySize: "+ myPawn.BodySize
                            + "\n BloodPumping: " + myPawn.GetBloodPumping()
                           ;
                    }
                }
                return result;
            }
        }

    }
}
