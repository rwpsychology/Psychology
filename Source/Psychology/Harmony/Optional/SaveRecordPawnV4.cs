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
    public static class SaveRecordPawnV4Patch
    {
        [HarmonyPostfix]
        public static void ExposePsycheData(EdB.PrepareCarefully.SaveRecordPawnV4 __instance)
        {
            if(customPawns.Keys.Contains(__instance.id) && Scribe.mode == LoadSaveMode.Saving) {
                EdB.PrepareCarefully.CustomPawn pawn = customPawns[__instance.id] as EdB.PrepareCarefully.CustomPawn;
                if(PsycheHelper.PsychologyEnabled(pawn.Pawn))
                {
                    PrepareCarefully.SaveRecordPsycheV4 psycheSave = new PrepareCarefully.SaveRecordPsycheV4(pawn.Pawn);
                    psycheSave.ExposeData();
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                PrepareCarefully.SaveRecordPsycheV4 psycheSave = new PrepareCarefully.SaveRecordPsycheV4();
                psycheSave.ExposeData();
                savedPawns.Add(__instance, psycheSave);
            }
        }

        public static Dictionary<object, PrepareCarefully.SaveRecordPsycheV4> savedPawns = new Dictionary<object, PrepareCarefully.SaveRecordPsycheV4>();
        public static Dictionary<string, object> customPawns = new Dictionary<string, object>();
    }
}
