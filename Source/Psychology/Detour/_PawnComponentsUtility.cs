using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _PawnComponentsUtility
    {
        [DetourMethod(typeof(PawnComponentsUtility),"CreateInitialComponents")]
        internal static void _CreateInitialComponents(Pawn pawn)
        {
            Log.Message("Creating components for " + pawn.LabelShort);
            pawn.ageTracker = new Pawn_AgeTracker(pawn);
            pawn.health = new Pawn_HealthTracker(pawn);
            pawn.records = new Pawn_RecordsTracker(pawn);
            pawn.inventory = new Pawn_InventoryTracker(pawn);
            pawn.meleeVerbs = new Pawn_MeleeVerbs(pawn);
            pawn.verbTracker = new VerbTracker(pawn);
            pawn.carryTracker = new Pawn_CarryTracker(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            if (pawn.RaceProps.ToolUser)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                pawn.apparel = new Pawn_ApparelTracker(pawn);
            }
            if (pawn.RaceProps.Humanlike)
            {
                pawn.ownership = new Pawn_Ownership(pawn);
                pawn.skills = new Pawn_SkillTracker(pawn);
                pawn.story = new Pawn_StoryTracker(pawn);
                pawn.guest = new Pawn_GuestTracker(pawn);
                pawn.guilt = new Pawn_GuiltTracker();
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                PsychologyPawn realPawn = pawn as PsychologyPawn;
                if (PsychologyBase.ActivateKinsey())
                {
                    if (realPawn != null)
                    {
                        realPawn.sexuality = new Pawn_SexualityTracker(realPawn);
                    }
                }
                if (realPawn != null)
                {
                    realPawn.psyche = new Pawn_PsycheTracker(realPawn);
                    Log.Message("Gave " + pawn.LabelShort + " a psyche.");
                }
            }
            if (pawn.RaceProps.IsFlesh)
            {
                pawn.relations = new Pawn_RelationsTracker(pawn);
            }
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, false);
        }
    }
}
