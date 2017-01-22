using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    class _SocialCardUtility
    {
        public static void DrawKinseyValue(Rect rect, Pawn pawn)
        {
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if(realPawn != null)
            {
                float width = rect.width - 26f - 3f;
                string kinsey = "Kinsey rating: " + realPawn.sexuality.kinseyRating;
                float num4 = Mathf.Max(26f, Text.CalcHeight(kinsey, width));
                Rect rect2 = new Rect(rect.width - 120f, 4f, width, num4);
                Widgets.Label(rect2, kinsey);
            }
        }

        // Token: 0x06002003 RID: 8195 RVA: 0x000A9504 File Offset: 0x000A7704
        [DetourMethod(typeof(SocialCardUtility),"DrawSocialCard")]
        public static void DrawSocialCard(Rect rect, Pawn pawn)
        {
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Small;
            Rect rect2 = new Rect(0f, 20f, rect.width, rect.height - 20f);
            Rect rect3 = rect2.ContractedBy(10f);
            Rect rect4 = rect3;
            Rect rect5 = rect3;
            Rect rect7 = rect3;
            rect4.height *= 0.63f;
            rect5.y = rect4.yMax + 17f;
            rect5.yMax = rect3.yMax;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(0f, (rect4.yMax + rect5.y) / 2f, rect.width);
            GUI.color = Color.white;
            if (Prefs.DevMode)
            {
                Rect rect6 = new Rect(5f, 5f, rect.width, 22f);
                typeof(SocialCardUtility).GetMethod("DrawDebugOptions", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { rect6, pawn });
            }
            if (PsychologyBase.ActivateKinsey() && pawn is PsychologyPawn && ((PsychologyPawn)pawn).sexuality != null)
            {
                DrawKinseyValue(rect7, pawn);
            }
            SocialCardUtility.DrawRelationsAndOpinions(rect4, pawn);
            typeof(SocialCardUtility).GetMethod("DrawInteractionsLog", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { rect5, pawn });
            GUI.EndGroup();
        }

    }
}
