using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace BloodDripping
{
    public static class ToolsComp
    {
        public static bool Is_Stain_Footprint_HediffComp(HediffDef hediffdef, out HediffCompProperties_Stain_Footprint hediffCompProperties_Stain_Footprint )
        {
            IEnumerable<HediffCompProperties> IEmaybe = hediffdef.comps;
            if (IEmaybe.EnumerableNullOrEmpty())
            {
                hediffCompProperties_Stain_Footprint = null;
                return false;
            }

            hediffCompProperties_Stain_Footprint = (HediffCompProperties_Stain_Footprint)IEmaybe.First();
            return true;
        }

        public static bool Get_Stain_Footprint_HediffComp_debug_and_race(HediffDef hediffdef, out bool debug, out string raceDefName)
        {
            HediffCompProperties_Stain_Footprint maybe;
            if (!Is_Stain_Footprint_HediffComp(hediffdef, out maybe))
            {
                debug = false;
                raceDefName = string.Empty;

                return false;
            }

            debug = maybe.debug;
            raceDefName = maybe.race;
            return true;
        }

        public static bool Is_BloodDripping_HediffComp(HediffDef hediffdef, out HediffCompProperties_BloodDripping hediffCompProperties_BloodDripping)
        {
            IEnumerable<HediffCompProperties> IEmaybe = hediffdef.comps;
            if (IEmaybe.EnumerableNullOrEmpty())
            {
                hediffCompProperties_BloodDripping = null;
                return false;
            }

            hediffCompProperties_BloodDripping = (HediffCompProperties_BloodDripping)IEmaybe.First();
            return true;
        }

        public static bool Get_BloodDripping_HediffComp_debug_and_race(HediffDef hediffdef, out bool debug, out string raceDefName)
        {
            HediffCompProperties_BloodDripping maybe;
            if (!Is_BloodDripping_HediffComp(hediffdef, out maybe))
            {
                debug = false;
                raceDefName = string.Empty;

                return false;
            }

            debug = maybe.debug;
            raceDefName = maybe.race;
            return true;
        }

    }
}
