using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.CheckAcceptArrest))]
    public static class Pawn_ArrestPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SwapArrestChance(IEnumerable<CodeInstruction> instr, ILGenerator gen)
        {
            List<Label> labels = new List<Label>();
            Label skipLabel = gen.DefineLabel(); //Have to assign variable
            bool removeIfStatement = false;
            foreach(CodeInstruction itr in instr)
            {
                if((itr.opcode == OpCodes.Call && itr.operand == AccessTools.Property(typeof(Rand), nameof(Rand.Value)).GetGetMethod()) || removeIfStatement)
                {
                    if(!removeIfStatement)
                    {
                        removeIfStatement = true;
                        labels = itr.labels;
                    }
                    if (itr.opcode == OpCodes.Bge_Un)
                    {
                        skipLabel = (Label)itr.operand;
                        yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = labels };
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_ArrestPatch), nameof(Pawn_ArrestPatch.NewArrestCheck), new Type[] { typeof(Pawn), typeof(Pawn) }));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel);
                        removeIfStatement = false;
                    }
                }
                else
                {
                    yield return itr;
                }
            }
        }

        public static bool NewArrestCheck(Pawn pawn, Pawn arrester)
        {
            return (Rand.Chance(arrester.GetStatValue(StatDefOfPsychology.ArrestPeacefullyChance) * (Mathf.InverseLerp(-100f, 100f, pawn.relations.OpinionOf(arrester))) * (arrester.Faction == pawn.Faction ? 1.5f : 1f) * (pawn.InMentalState ? 0.2f : 1f)));
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreTraded))]
    public static class Pawn_PreTradedPatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(Pawn __instance, TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            if (action == TradeAction.PlayerSells)
            {
                if (__instance.RaceProps.Humanlike)
                {
                    foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
                    {
                        current.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.KnowPrisonerSoldBleedingHeart, null);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("LabelNoCount", PropertyMethod.Getter)]
    public static class Pawn_MayorLabel
    {
        [HarmonyPostfix]
        public static void AddMayorLabel(Pawn __instance, String __result)
        {
            if (__instance.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor))
            {
                __result = __instance.Name.ToStringShort + ", Mayor";
            }
        }
    }
}
