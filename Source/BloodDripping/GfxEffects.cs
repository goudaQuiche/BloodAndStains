﻿using RimWorld;

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

        private static void PlaceTrail(Vector3 loc, Map map, float rot, float scale, ThingDef Mote_FootprintDef)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Mote_FootprintDef, null);

            //moteThrown.Graphic.color = Color.blue;

            moteThrown.Scale = scale;
            moteThrown.exactRotation = rot;
            moteThrown.exactPosition = loc;
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public static void TryPlaceTrailPrint(this HediffComp_Stain_Footprint h)
        {

            Vector3 drawPos = h.myPawn.Drawer.DrawPos;
            Vector3 normalized = (drawPos - h.lastFootprintPlacePos).normalized;
            float rot = normalized.AngleFlat();

            //Vector3 vector = h.myPawn.TrueCenter() + FootprintOffset;
            //float angle = (!h.lastFootprintRight) ? -90 : 90;
            float angle = 0;
            Vector3 b = normalized.RotatedBy(angle) * 0.17f * Mathf.Sqrt(h.myPawn.BodySize);
            Vector3 vector = drawPos + FootprintOffset + b;

            IntVec3 c = vector.ToIntVec3();
            if (c.InBounds(h.myMap))
            {
                TerrainDef terrain = c.GetTerrain(h.myMap);
                if (terrain != null)
                {
                    PlaceTrail(vector, h.myMap, rot, h.Props.randomScale.RandomInRange, h.moteFootprintDef);
                }
            }
            h.lastFootprintPlacePos = drawPos;
        }

        private static void PlaceFootprint(Vector3 loc, Map map, float rot, ThingDef Mote_FootprintDef)
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

        public static void TryPlaceFootprint(this HediffComp_Stain_Footprint h)
        {

            Vector3 drawPos = h.myPawn.Drawer.DrawPos;
            Vector3 normalized = (drawPos - h.lastFootprintPlacePos).normalized;

            float rot = normalized.AngleFlat();
            float angle = (!h.lastFootprintRight) ? -90 : 90;

            Vector3 b = normalized.RotatedBy(angle) * 0.17f * Mathf.Sqrt(h.myPawn.BodySize);
            Vector3 vector = drawPos + FootprintOffset + b;
            IntVec3 c = vector.ToIntVec3();
            if (c.InBounds(h.myMap))
            {
                TerrainDef terrain = c.GetTerrain(h.myMap);
                if (terrain != null)
                {
                    PlaceFootprint(vector, h.myMap, rot, h.moteFootprintDef);
                }
            }

            h.lastFootprintPlacePos = drawPos;
            h.lastFootprintRight = !h.lastFootprintRight;
        }
    }
}
