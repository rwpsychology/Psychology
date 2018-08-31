using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Harmony;
using System.Reflection.Emit;

namespace Psychology.Harmony.Optional
{
    public static class PresetLoaderPatch
    {
        [HarmonyPostfix]
        public static void AddPsyche(ref EdB.PrepareCarefully.CustomPawn __result, EdB.PrepareCarefully.SaveRecordPawnV3 record)
        {
            if(SaveRecordPawnV3Patch.savedPawns.Keys.Contains(record))
            {
                Pawn pawn = __result.Pawn;
                if (pawn != null && PsycheHelper.PsychologyEnabled(pawn))
                {
                    PrepareCarefully.SaveRecordPsycheV3 psycheSave = SaveRecordPawnV3Patch.savedPawns[record];
                    PsycheHelper.Comp(pawn).Psyche.upbringing = psycheSave.upbringing;
                    foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
                    {
                        PersonalityNode savedNode = psycheSave.nodes.Find(n => n.def == node.def);
                        if(savedNode != null)
                        {
                            node.rawRating = savedNode.rawRating;
                        }
                    }
                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = psycheSave.sexDrive;
                    PsycheHelper.Comp(pawn).Sexuality.romanticDrive = psycheSave.romanticDrive;
                    PsycheHelper.Comp(pawn).Sexuality.kinseyRating = psycheSave.kinseyRating;
                }
            }
        }
    }
}
