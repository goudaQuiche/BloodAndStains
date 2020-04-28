using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace BloodDripping
{
    public static class Tools
    {
        public static bool OkPawn(Pawn pawn)
        {
            return ((pawn != null) && (pawn.Map != null));
        }

        public static void Warn(string warning, bool debug = false)
        {
            if(debug)
                Log.Warning(warning);
        }
    }
}
