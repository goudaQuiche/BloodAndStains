/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Verse;
using RimWorld;

namespace BloodDripping
{
    public class HediffCompProperties_Stain_Footprint : HediffCompProperties
    {

        public int period = 15;

        public ThingDef mote_HumanBlood_FootprintDef = null;
        public ThingDef mote_InsectBlood_FootprintDef = null;
        public ThingDef mote_Vomit_FootprintDef = null;
        public ThingDef mote_Filth_FootprintDef = null;

        public bool debug = false;

        public HediffCompProperties_Stain_Footprint()
        {
            this.compClass = typeof(HediffComp_Stain_Footprint);
        }
    }
}