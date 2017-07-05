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
                if (pawn != null && pawn is PsychologyPawn)
                {
                    PrepareCarefully.SaveRecordPsycheV3 psycheSave = SaveRecordPawnV3Patch.savedPawns[record];
                    PsychologyPawn realPawn = pawn as PsychologyPawn;
                    realPawn.psyche.upbringing = psycheSave.upbringing;
                    foreach (PersonalityNode node in realPawn.psyche.PersonalityNodes)
                    {
                        PersonalityNode savedNode = psycheSave.nodes.Find(n => n.def == node.def);
                        if(savedNode != null)
                        {
                            node.rawRating = savedNode.rawRating;
                        }
                    }
                    if(PsychologyBase.ActivateKinsey())
                    {
                        realPawn.sexuality.sexDrive = psycheSave.sexDrive;
                        realPawn.sexuality.romanticDrive = psycheSave.romanticDrive;
                        realPawn.sexuality.kinseyRating = psycheSave.kinseyRating;
                    }
                }
            }
        }
    }
}
