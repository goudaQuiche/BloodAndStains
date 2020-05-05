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
        
        public bool trailLikeFootprint = false;
        public FloatRange randomScale = new FloatRange(.75f, 1);

        public bool debug = false;

        public HediffCompProperties_Stain_Footprint()
        {
            this.compClass = typeof(HediffComp_Stain_Footprint);
        }
    }
}