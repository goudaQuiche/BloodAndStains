using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;


namespace BloodDripping
{
    public static class GfxEffects
    {

        private const float FootprintIntervalDist = 0.632f;
        private static readonly Vector3 FootprintOffset = new Vector3(0f, 0f, -0.3f);

        private const float LeftRightOffsetDist = 0.17f;
        private const float FootprintSplashSize = 2f;

        private static void PlaceFootprint(Vector3 loc, Map map, float rot, float scale, ThingDef Mote_FootprintDef)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority || Mote_FootprintDef == null)
                return;

            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Mote_FootprintDef, null);

            moteThrown.Scale = scale;
            moteThrown.exactRotation = rot;
            moteThrown.exactPosition = loc;
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public static void TryPlaceFootprint(this HediffComp_Stain_Footprint h, bool trailLike = false, bool debug = false)
        {

            Vector3 drawPos = h.myPawn.Drawer.DrawPos;
            Vector3 normalized = (drawPos - h.lastFootprintPlacePos).normalized;
            float rot = normalized.AngleFlat();

            Tools.Warn("base rotation: " + rot, debug);

            ThingDef myMoteDef = h.moteFootprintDef;
            if (h.Props.disabilityFootprints && h.availableDisabilityMotes && h.HasDisabilityHediff)
                myMoteDef = h.lastFootprintRight ? h.moteRightFootprintDef : h.moteLeftFootprintDef;

            if(h.HasRotation)
                if (h.lastFootprintRight)
                {
                    rot += h.Props.rightFootRotation;
                    Tools.Warn("right foot additional rotation: " + h.Props.rightFootRotation + "=>rot:" + rot, debug);
                }
                else
                {
                    rot += h.Props.leftFootRotation;
                    Tools.Warn("left foot additional rotation: " + h.Props.leftFootRotation + "=>rot:" + rot, debug);
                }

            float angle = 0;
            if (!trailLike)
                angle = (!h.lastFootprintRight) ? -90 : 90;

            Vector3 b = normalized.RotatedBy(angle) * 0.17f * Mathf.Sqrt(h.myPawn.BodySize);
            Vector3 vector = drawPos + FootprintOffset + b;
            IntVec3 c = vector.ToIntVec3();

            if (c.InBounds(h.myMap) && c.GetTerrain(h.myMap) != null)
                PlaceFootprint(vector, h.myMap, rot, h.Props.randomScale.RandomInRange, myMoteDef);

            h.lastFootprintPlacePos = drawPos;
            h.lastFootprintRight = !h.lastFootprintRight;
        }
    }
}
