using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;


namespace BloodDripping
{
    public static class ToolsPtawn
    {
        public static bool IsRaceMember(this Pawn pawn, string raceDefName)
        {
            return pawn?.def.defName == raceDefName;
        }

        public static bool HasHediff(this Pawn pawn, HediffDef hediffDef)
        {
            return pawn.health.hediffSet.HasHediff(hediffDef);
        }

        public static bool IsHuman(this Pawn pawn)
        {
            return pawn?.def.defName == "Human";
        }

        public static bool Has_Human_Stain_Footprint(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(MyDefs.Human_Stain_Footprint_HediffDef);
        }

        public static bool Has_Human_BloodDripping(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(MyDefs.Human_Red_BloodDripping_HediffDef);
        }

        public static bool IsBleeding(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss);
        }

        public static bool IsLaying(this Pawn pawn)
        {
            return (pawn.Downed ||
                (pawn.CurJob != null &&
                (pawn.CurJob.def == JobDefOf.LayDown || pawn.CurJob.def == JobDefOf.Wait_Downed)));
        }

        public static bool HasBloodDef(this Pawn pawn)
        {
            return pawn.def.race.BloodDef != null;
        }

        public static Color GetBloodColor(this Pawn pawn)
        {
            if (pawn.HasBloodDef())
            {
                return pawn.def.race.BloodDef.graphicData.color;
            }

            return Color.red;
        }

        public static float GetBloodPumping(this Pawn pawn)
        {
            return pawn.health.capacities.GetLevel(PawnCapacityDefOf.BloodPumping);
        }
    }
}
