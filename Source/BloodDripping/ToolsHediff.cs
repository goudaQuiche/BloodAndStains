using RimWorld;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace BloodDripping
{
    public static class ToolsHediff
    {
        static List<HediffDef> relevantHediffList = new List<HediffDef> { HediffDefOf.MissingBodyPart, HediffDefOf.PegLeg, MyDefs.WoodenFootHediffDef };
        /*
        static List<BodyPartDef> bpList = new List<BodyPartDef> { MyDefs.LegDef, MyDefs.FootDef };
        static List<string> legCustomLabel = new List<string> { "left leg", "right leg"};
        static List<string> footCustomLabel = new List<string> { "left foot", "right foot" };
        */

        public static Hediff HasRelevantHediff(this Pawn pawn, BodyPartDef BPDef, string customLabel, bool debug = false)
        {
            IEnumerable<BodyPartRecord> IEBpr = pawn.RaceProps.body.GetPartsWithDef(BPDef).Where(bpr => bpr.customLabel == customLabel);
            if (IEBpr.EnumerableNullOrEmpty())
            {
                Tools.Warn("Could not find " + BPDef.defName + " with custom label " + customLabel);
                return null;
            }

            BodyPartRecord AimedBpr = IEBpr.First();
            foreach (HediffDef hDef in relevantHediffList)
            {
                Hediff myH = null;
                IEnumerable<Hediff> hList = pawn.health.hediffSet.hediffs.Where(h => h.Part == AimedBpr).Where(hD => hD.def == hDef);
                if (hList.EnumerableNullOrEmpty())
                    continue;

                if ((myH = hList.First()) != null)
                    return myH;
            }

            Tools.Warn(pawn.LabelShort + " - no disability found", debug);
            return null;
        }

        public static Hediff GetLeftLegFirstRelevantHediff(this Pawn pawn, bool debug = false)
        {
            Hediff firstH = null;

            foreach (HediffDef curH in relevantHediffList)
                if ((firstH = HasRelevantHediff(pawn, MyDefs.LegDef, "left leg", debug)) != null)
                    return firstH;

            foreach (HediffDef curH in relevantHediffList)
                if ((firstH = HasRelevantHediff(pawn, MyDefs.FootDef, "left foot", debug)) != null)
                    return firstH;

            return null;
        }

        public static Hediff GetRightLegFirstRelevantHediff(this Pawn pawn, bool debug = false)
        {
            Hediff firstH = null;

            foreach (HediffDef curH in relevantHediffList)
                if ((firstH = HasRelevantHediff(pawn, MyDefs.LegDef, "right leg", debug)) != null)
                    return firstH;

            foreach (HediffDef curH in relevantHediffList)
                if ((firstH = HasRelevantHediff(pawn, MyDefs.FootDef, "right foot", debug)) != null)
                    return firstH;

            return null;
        }

        public static ThingDef RegularMoteToDisabilityHediffMote(this HediffComp_Stain_Footprint hFP, Hediff h, ThingDef baseMoteDef, bool myDebug = false)
        {
            string baseHDef = baseMoteDef.defName;
            string newDefName = string.Empty;

            IEnumerable<ThingDef> newMoteIE = null;

            if (h.def == HediffDefOf.MissingBodyPart)
            {
                if (h.Part.def == BodyPartDefOf.Leg)
                    return null;

                newMoteIE = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.label== "Mote" && b.defName.Contains(hFP.Props.missingPartMote_pattern));
                if (newMoteIE.EnumerableNullOrEmpty())
                {
                    Tools.Warn("RegularMoteToHediffMote - Did not find Mote_Human_MissingPart", myDebug);
                    return baseMoteDef;
                }

                return newMoteIE.RandomElement();
            }
            else if (h.def == HediffDefOf.PegLeg)
            {
                newDefName = Regex.Replace(baseHDef, "_Footprint$", hFP.Props.peglegMote_pattern);

            } else if (h.def == MyDefs.WoodenFootHediffDef)
            {
                newDefName = Regex.Replace(baseHDef, "_Footprint$", hFP.Props.woodenfootMote_pattern);
            } else {
                Tools.Warn("RegularMoteToHediffMote - empty hediff case - should not happen", myDebug);
                return baseMoteDef;
            }

            newMoteIE = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.label == "Mote" && b.defName == newDefName);
            if (newMoteIE.EnumerableNullOrEmpty())
            {
                Tools.Warn("RegularMoteToHediffMote - Did not find " + newDefName, myDebug);
                return baseMoteDef;
            }
            return newMoteIE.First();
        }

        public static bool CheckDisabilityMotes(this HediffComp_Stain_Footprint h, bool myDebug = false)
        {
            IEnumerable<ThingDef> disabilityMoteIE = null;
            disabilityMoteIE = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.defName.Contains(h.Props.missingPartMote_pattern));

            if (disabilityMoteIE.EnumerableNullOrEmpty())
            {
                Tools.Warn("Cant find missingPart_Mote: " + h.Props.missingPartMote_pattern, myDebug);
                return false;
            }
            else
            {
                string missingPart = string.Empty;
                foreach (ThingDef moteDef in disabilityMoteIE)
                    missingPart += moteDef + ", ";

                Tools.Warn("Found missingPart_Mote: " + disabilityMoteIE.Count() + "; " + missingPart, myDebug);
            }

            // checking pegleg motes for each regular footprint in Props
            foreach (Footprint footprint in h.Props.footprint)
            {
                string guessDefName = string.Empty;
                guessDefName = Regex.Replace(footprint.moteDef.defName, "_Footprint$", h.Props.peglegMote_pattern);
                disabilityMoteIE = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.defName == guessDefName);
                if (disabilityMoteIE.EnumerableNullOrEmpty())
                {
                    Tools.Warn("no pegleg Mote found :" + guessDefName, myDebug);
                    return false;
                }
                else
                    Tools.Warn("found pegleg Mote: " + guessDefName, myDebug);
            }

            // checking wooden foot motes for each regular footprint in Props
            foreach (Footprint footprint in h.Props.footprint)
            {
                string guessDefName = string.Empty;
                guessDefName = Regex.Replace(footprint.moteDef.defName, "_Footprint$", h.Props.woodenfootMote_pattern);
                disabilityMoteIE = DefDatabase<ThingDef>.AllDefs.Where((ThingDef b) => b.defName == guessDefName);
                if (disabilityMoteIE.EnumerableNullOrEmpty())
                {
                    Tools.Warn("no pegleg Mote found :" + guessDefName, myDebug);
                    return false;
                }
                else
                    Tools.Warn("found woodenfoot Mote: " + guessDefName, myDebug);
            }

            return true;
        }

    }
}
