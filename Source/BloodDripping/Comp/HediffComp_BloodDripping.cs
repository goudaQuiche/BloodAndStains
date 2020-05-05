using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace BloodDripping
{
    public class HediffComp_BloodDripping : HediffComp
    {
        Pawn myPawn = null;
        Map myMap = null;

        List <ThingDef> puddleMoteDef = null;
        private Vector3 lastPuddlePlacePos;
        //private static readonly Vector3 BloodPuddleOffset = new Vector3(0f, 0f, -0.3f);
        private static readonly Vector3 StandingOffset = new Vector3(0f, 0f, -0.7f);
        //private static readonly Vector3 Downedffset = new Vector3(0f, 0f, 0);

        private int ticksUntilPuddle = 500;
        private int puddleTicksLeft;

        private float bleedRateTotal;
        private float bleedingScale;
        private int layingTicks;
        private int layingTicksLimit = 1800;
        private float layingFactor;

        //60 = 1 sec
        private readonly int CheckEveryXTicks = 90;

        float PuddleSizeMultiplier = LoadedModManager.GetMod<BloodDrippingMod>().GetSettings<BloodDripping_Settings>().PuddleSizeMultiplier;
        bool shouldSkip = false;

        ThingDef DeathPuddleMote = null;

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
            //return !(terrain == null || terrain.IsWater);
            return !(terrain == null);
        }

        public bool IsBleeding
        {
            get
            {
                return myPawn.IsBleeding();
            }
        }

        public bool WasBleeding
        {
            get
            {
                return (bleedRateTotal > 0);
            }
        }

        public void CalculatePuddle()
        {

            bleedRateTotal = myPawn.health.hediffSet.BleedRateTotal;
            //=(1-EXP(-A10/2))*2

            bleedingScale = (1 - (float)Math.Exp(-bleedRateTotal / 6)) * 3f;

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
                layingTicks += 15;
                // if layingTicks over limit, then limit
                layingTicks = Math.Min(layingTicks, layingTicksLimit);

                layingFactor = (float)layingTicks / (float)layingTicksLimit;

                bleedingScale *= 1.1f + layingFactor;
                ticksUntilPuddle *= 2 + (int)(2 * layingFactor);
            }
            else
            {
                layingTicks = 0;
            }
            // if ticksUntil over limit, then maxlimit
            ticksUntilPuddle = Math.Min(ticksUntilPuddle, Props.maxPeriod);
            // if ticksUntil under limit, then minlimit
            ticksUntilPuddle = Math.Max(ticksUntilPuddle, Props.minPeriod);

            Tools.Warn(
                "New bleed rate: " + bleedRateTotal +
                "; bleedingScale: " + bleedingScale +
                "; ticksUntilPuddle: " + ticksUntilPuddle
                , Props.spammingDebug);

        }

        public void Ticking()
        {
            // period reached, actually doing something
            if (puddleTicksLeft <= 0)
            {
                if (TerrainAllowsPuddle(myPawn))
                {
                    Tools.Warn(myPawn.LabelShort + " trying to place bloody puddle", Props.spammingDebug);
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
                Tools.Warn("pawn unspawned", Props.debug);
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
            Tools.Warn("Entering Init", Props.debug);
            myPawn = parent.pawn;
            myMap = myPawn.Map;

            if (!myPawn.IsHuman() && Props.race.NullOrEmpty())
            {
                Log.Error("no race set in the bloodDripping hediffCompProps, you need one for an alien race, destroying hediff");
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            if(myPawn == null || myMap == null)
            {
                Tools.Warn("Null pawn or map", Props.debug);
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            if (Props.puddleMoteDef.NullOrEmpty())
            {
                Tools.Warn("no Props.puddleMoteDef found, destroying hediff", Props.debug);
                parent.Severity = 0;
                shouldSkip = true;
                return;
            }

            //DeathPuddleMote = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.defName.Contains("BNS_Mote_DeathPuddle_R"));
            if(!Props.deathPuddleMoteContainsString.NullOrEmpty())
                DeathPuddleMote = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.defName.Contains(Props.deathPuddleMoteContainsString))?.RandomElement();

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
            float answer = Props.randomScale.RandomInRange * bleedingScale * myPawn.GetBloodPumping() * PuddleSizeMultiplier;

            if (myPawn.BodySize > 0)
                return (answer * myPawn.BodySize);

            return answer;
        }

        public void TryPlaceDeathMote()
        {
            if (!Pawn.health.Dead)
            {
                Tools.Warn(myPawn + " is not dead, leave him alone, leaving TryPlaceDeathMote", Props.debug);
                return;
            }
            if (DeathPuddleMote == null)
            {
                Tools.Warn("No death mote, give <deathPuddleMoteContainsString> a value in Hediff_Red_BloodDripping", Props.debug);
                return;
            }

            Tools.Warn(myPawn + " entering TryPlaceDeathMote", Props.debug);

            float puddleSize = myPawn.BodySize;
            FloatRange randRotation = new FloatRange(-30f, 30f);
            ThingDef deathMote = DeathPuddleMote;

            Vector3 drawPos = myPawn.Corpse.DrawPos;
            Vector3 normalized = drawPos.normalized;
            float rot = normalized.AngleFlat();

            Vector3 vector = drawPos;
            IntVec3 c = vector.ToIntVec3();

            if (c.InBounds(myMap))
            {
                TerrainDef terrain = c.GetTerrain(myPawn.Corpse.Map);
                if (terrain != null)
                {
                    PlacePuddle(vector, myMap, rot, puddleSize, deathMote);
                }
            }
        }

        public void TryPlaceMote()
        {
            Vector3 drawPos = myPawn.Drawer.DrawPos;
            Vector3 normalized = drawPos.normalized;
            float rot = normalized.AngleFlat();

            if(!myPawn.IsLaying())
                rot += Props.randomRotation.RandomInRange;

            Vector3 vector = drawPos;
            if (!myPawn.IsLaying())
                vector += StandingOffset;

            Tools.Warn(myPawn.LabelShort+" rot: "+rot, Props.spammingDebug);
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
                        result +=
                            " moteSaturated: " + myMap.moteCounter.SaturatedLowPriority + "\n------"
                            + "\n ticks: " + puddleTicksLeft + "/" + ticksUntilPuddle
                            + "\n Bleeding rate: " + bleedRateTotal
                            + "\n layingFactor: " + layingFactor
                            + "\n BodySize: "+ myPawn?.BodySize
                            + "\n BloodPumping: " + myPawn.GetBloodPumping()
                            + "\n---------->"
                            + "\n bleedingScale: " + bleedingScale
                           ;
                    }
                }
                return result;
            }
        }

    }
}
