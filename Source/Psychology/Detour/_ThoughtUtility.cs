// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtUtility
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _ThoughtUtility
    {
        [DetourMethod(typeof(ThoughtUtility), "GiveThoughtsForPawnExecuted")]
        internal static void _GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            int forcedStage = 1;
            if (victim.guilt.IsGuilty)
            {
                forcedStage = 0;
            }
            else
            {
                switch (kind)
                {
                    case PawnExecutionKind.GenericBrutal:
                        forcedStage = 2;
                        break;
                    case PawnExecutionKind.GenericHumane:
                        forcedStage = 1;
                        break;
                    case PawnExecutionKind.OrganHarvesting:
                        forcedStage = 3;
                        break;
                }
            }
            ThoughtDef def;
            ThoughtDef def2;
            if (victim.IsColonist)
            {
                def = ThoughtDefOf.KnowColonistExecuted;
                def2 = ThoughtDefOfPsychology.KnowColonistExecutedBleedingHeart;
            }
            else
            {
                def = ThoughtDefOf.KnowGuestExecuted;
                def2 = ThoughtDefOfPsychology.KnowGuestExecutedBleedingHeart;
            }
            foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                     where x.IsColonist || x.IsPrisonerOfColony
                                     select x)
            {
                current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(def, forcedStage), null);
                current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(def2, forcedStage), null);
            }
        }

        [DetourMethod(typeof(ThoughtUtility), "GiveThoughtsForPawnOrganHarvested")]
        internal static void _GiveThoughtsForPawnOrganHarvested(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            ThoughtDef thoughtDef = null;
            ThoughtDef thoughtDef2 = null;
            if (victim.IsColonist)
            {
                thoughtDef = ThoughtDefOf.KnowColonistOrganHarvested;
                thoughtDef2 = ThoughtDefOfPsychology.KnowColonistOrganHarvestedBleedingHeart;
            }
            else if (victim.HostFaction == Faction.OfPlayer)
            {
                thoughtDef = ThoughtDefOf.KnowGuestOrganHarvested;
                thoughtDef2 = ThoughtDefOfPsychology.KnowGuestOrganHarvestedBleedingHeart;
            }
            foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                     where x.IsColonist || x.IsPrisonerOfColony
                                     select x)
            {
                if (current == victim)
                {
                    current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.MyOrganHarvested, null);
                }
                else if (thoughtDef != null)
                {
                    current.needs.mood.thoughts.memories.TryGainMemoryThought(thoughtDef, null);
                    current.needs.mood.thoughts.memories.TryGainMemoryThought(thoughtDef2, null);
                }
            }
        }

        /** This function searches for a thought in a pawn's memories, caused by another pawn, if applicable. **/
        public static bool FindThoughtInMemories(Pawn pawn, ThoughtDef thought, Pawn cause = null)
        {
            foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
            {
                if (memory.def == thought)
                {
                    if (memory.otherPawn == cause || cause == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
