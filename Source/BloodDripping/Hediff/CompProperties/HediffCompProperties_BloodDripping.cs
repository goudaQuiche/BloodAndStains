/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace BloodDripping
{
    public class HediffCompProperties_BloodDripping : HediffCompProperties
    {
        public int period;
        public int minPeriod;
        public int maxPeriod;

        public string race;

        public List <ThingDef> puddleMoteDef;
        public FloatRange randomScale;
        public FloatRange randomRotation;

        public bool debug = false;
        public bool spammingDebug = false;

        public string deathPuddleMoteContainsString;

        public HediffCompProperties_BloodDripping()
        {
            this.compClass = typeof(HediffComp_BloodDripping);
        }
    }
}