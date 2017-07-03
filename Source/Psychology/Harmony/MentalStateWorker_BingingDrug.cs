using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(MentalStateWorker_BingingDrug), "StateCanOccur", new Type[] { typeof(Pawn) })]
    public static class MentalStateWorker_BingingDrugPatch
    {
        [HarmonyPostfix]
        public static void DrugFreeDisable(ref bool __result, Pawn pawn)
        {
            __result = __result && !pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.DrugFree);
        }
    }
}
