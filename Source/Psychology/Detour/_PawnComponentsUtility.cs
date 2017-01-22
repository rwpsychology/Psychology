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
        // Token: 0x06000D28 RID: 3368 RVA: 0x000409B0 File Offset: 0x0003EBB0
        [DetourMethod(typeof(PawnComponentsUtility),"CreateInitialComponents")]
        internal static void _CreateInitialComponents(Pawn pawn)
        {
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
                if(PsychologyBase.ActivateKinsey())
                {
                    PsychologyPawn realPawn = pawn as PsychologyPawn;
                    if(realPawn != null)
                    {
                        realPawn.sexuality = new Pawn_SexualityTracker(realPawn);
                    }
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
