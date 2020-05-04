using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;


namespace BloodDripping
{
    public class BloodDripping_Settings : ModSettings
    {
        public int PuddleSizeMultiplier = 1;
        public int FilthPerSteppedInItMultiplier = 1;
        public int MaxFilthCarriedMultiplier = 1;
        public bool RedFootprintOnlyIfInjured = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PuddleSizeMultiplier, "PuddleSizeMultiplier");
            Scribe_Values.Look(ref FilthPerSteppedInItMultiplier, "FilthPerSteppedInItMultiplier");
            Scribe_Values.Look(ref MaxFilthCarriedMultiplier, "MaxFilthCarriedMultiplier");
            Scribe_Values.Look(ref RedFootprintOnlyIfInjured, "RedFootprintOnlyIfInjured");
        }

    }

    public class BloodDrippingMod : Mod
    {
        BloodDripping_Settings settings;

        public BloodDrippingMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<BloodDripping_Settings>();
        }

        public override string SettingsCategory()
        {
            return "Blood an stains";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Blood puddles");
            listing.GapLine();

            listing.Label("Size multiplier: " + settings.PuddleSizeMultiplier);
            settings.PuddleSizeMultiplier = (int)listing.Slider(settings.PuddleSizeMultiplier, 1f, 10f);

            listing.Gap();

            listing.Label("Footprints");
            listing.GapLine();

            listing.Label("Filth per stepped in it multiplier - More footprints per each filth your pawn stepped in : " + settings.FilthPerSteppedInItMultiplier+"x");
            listing.Label("Filth duration per step in it : " + (int)(HediffComp_Stain_Footprint.LengthPerBloodFilth * settings.FilthPerSteppedInItMultiplier).TicksToSeconds() + " seconds");
            settings.FilthPerSteppedInItMultiplier = (int)listing.Slider(settings.FilthPerSteppedInItMultiplier, 1f, 5f);

            listing.Label("Max filth carried multiplier - Will augment the limit of footprints if pawns keeps on stepping it : " + settings.MaxFilthCarriedMultiplier+"x");
            listing.Label("Limit : " + (HediffComp_Stain_Footprint.StainedTicksLimit * settings.MaxFilthCarriedMultiplier).TicksToSeconds()+" seconds");
            settings.MaxFilthCarriedMultiplier = (int)listing.Slider(settings.MaxFilthCarriedMultiplier, 1f, 5f);

            listing.CheckboxLabeled("Red footprints only if injured (Humans only): ", ref settings.RedFootprintOnlyIfInjured);

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
