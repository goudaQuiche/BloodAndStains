using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;


namespace BloodDripping
{
    public static class ToolsJob
    {
        public static bool ChangedOfJob(Pawn pawn,Job oldJob)
        {
            if (pawn.CurJob != oldJob)
                return true;

            return false;
        }
    }
}
