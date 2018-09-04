using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionWorker_DeepTalk), nameof(InteractionWorker_DeepTalk.RandomSelectionWeight))]
	public static class InteractionWorker_DeepTalk_SelectionWeightPatch
	{
		[HarmonyPrefix]
		public static bool PsychologyException(InteractionWorker_DeepTalk __instance, ref float __result, Pawn initiator, Pawn recipient)
		{
            __result = 0f;
			return !PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient);
		}
	}
}
