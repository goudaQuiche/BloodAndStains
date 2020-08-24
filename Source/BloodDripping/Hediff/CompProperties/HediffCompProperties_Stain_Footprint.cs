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

namespace BloodDripping
{
    public class HediffCompProperties_Stain_Footprint : HediffCompProperties
    {

        public int period;
        public string race;

        public List<Footprint> footprint;
        public bool disabilityFootprints = true;

        public bool trailLikeFootprint = false;

        public FloatRange randomScale = new FloatRange(.75f, 1);
        public float rightFootRotation = 0;
        public float leftFootRotation = 0;

        public string missingPartMote_pattern = "Mote_Human_MissingPart";
        public string peglegMote_pattern = "_Pegleg";
        public string woodenfootMote_pattern = "_Woodenfoot";

        public bool debug = false;

        public HediffCompProperties_Stain_Footprint()
        {
            this.compClass = typeof(HediffComp_Stain_Footprint);
        }
    }
}