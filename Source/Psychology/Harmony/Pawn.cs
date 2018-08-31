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
    [HarmonyPatch(typeof(Pawn), "CheckAcceptArrest")]
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
                if((itr.opcode == OpCodes.Call && itr.operand == AccessTools.Property(typeof(Rand), "Value").GetGetMethod()) || removeIfStatement)
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
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_ArrestPatch), "NewArrestCheck", new Type[] { typeof(Pawn), typeof(Pawn) }));
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

    [HarmonyPatch(typeof(Pawn), "PreTraded")]
    public static class Pawn_PreTradedPatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(Pawn __instance, TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            if (action == TradeAction.PlayerSells)
            {
                if (__instance.RaceProps.Humanlike)
                {
                    foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive
                                             where x.IsColonist || x.IsPrisonerOfColony
                                             select x)
                    {
                        current.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.KnowPrisonerSoldBleedingHeart, null);
                    }
                }
            }
        }
    }
}
