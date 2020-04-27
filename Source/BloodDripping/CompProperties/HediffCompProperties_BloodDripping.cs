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

        public int period = 15;
        public List <ThingDef> puddleMoteDef;
        public FloatRange scale = new FloatRange(.5f, .8f);

        public bool debug = false;

        public HediffCompProperties_BloodDripping()
        {
            this.compClass = typeof(HediffComp_BloodDripping);
        }
    }
}