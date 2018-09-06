using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionUtility), "CanReceiveRandomInteraction")]
	public static class InteractionUtility_CanReceive_Patch
    {
        [LogPerformance]
        [HarmonyPostfix]
		public static void PsychologyAddonsForCanReceive(ref bool __result, Pawn p)
		{
			__result = __result && !p.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation) && (p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor) == null || !p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor).ownedPawns.Contains(p));
		}
	}

	[HarmonyPatch(typeof(InteractionUtility), "CanInitiateRandomInteraction", new[] { typeof(Pawn) })]
	public static class InteractionUtility_CanInitiate_Patch
    {
        [LogPerformance]
        [HarmonyPostfix]
		public static void PsychologyAddonsForCanInitiate(ref bool __result, Pawn p)
		{
			__result = __result && !p.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation) && (p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor) == null || !p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor).ownedPawns.Contains(p));
		}
	}
}
