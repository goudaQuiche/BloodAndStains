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

        public static bool TrueEveryNTicks(int NTicks=60)
        {
                return Find.TickManager.TicksGame % NTicks == 0;
        }
        public static bool TrueEverySec
        {
            get
            {
                return TrueEveryNTicks();
            }
        }
        public static bool TrueEvery5Sec
        {
            get
            {
                return TrueEveryNTicks(300);
            }
        }
        public static bool TrueEvery30Sec
        {
            get
            {
                return TrueEveryNTicks(1800);
            }
        }

        public static void Warn(string warning, bool debug = false)
        {
            if(debug)
                Log.Warning(warning);
        }
    }
}
