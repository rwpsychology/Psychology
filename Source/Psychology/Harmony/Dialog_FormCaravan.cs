using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Harmony;
using UnityEngine;


namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "AddPawnsToTransferables")]
    public static class Dialog_FormCaravan_AddPawnsToTransferables_Patch
    {
        [HarmonyPrefix]
        public static bool DoWindowContentsDisbandCaravans(Dialog_FormCaravan __instance)
        {
            /* Get rid of hanging out Lords so that those pawns can be sent on caravans easily */
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            Lord[] lords = (from l in map.lordManager.lords
                                       where (l.LordJob is LordJob_HangOut || l.LordJob is LordJob_Date)
                                       select l).ToArray();
            foreach (Lord l2 in lords)
            {
                map.lordManager.RemoveLord(l2);
            }
            return true;
        }
    }
}
