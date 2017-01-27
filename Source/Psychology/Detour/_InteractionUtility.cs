using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _InteractionUtility
    {
        [DetourMethod(typeof(InteractionUtility),"CanReceiveRandomInteraction")]
        internal static bool _CanReceiveRandomInteraction(Pawn p)
        {
            return InteractionUtility.CanReceiveInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState && !p.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation) && (p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor) == null || !p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor).ownedPawns.Contains(p));
        }
    }
}
