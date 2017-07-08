using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Psychology
{
    public static class PsycheCardUtility
    {
        private static void DrawPersonalityNodes(Rect rect, PsychologyPawn pawn)
        {
            float width = rect.width - 26f - 3f;
            List<PersonalityNode> allNodes = pawn.psyche.PersonalityNodes;
            allNodes.SortBy(p => -p.AdjustedRating, p=> p.def.defName);
            PsycheCardUtility.nodeStrings.Clear();
            float num = 0f;
            for (int i = 0; i < allNodes.Count; i++)
            {
                int category = Mathf.RoundToInt(6f*Mathf.InverseLerp(0.16f, 0.83f, allNodes[i].AdjustedRating));
                if (/*!allNodes[i].Core && */category != 3)
                {
                    string text = (NodeDescriptions[category] == "" ? "" : ("Psyche"+NodeDescriptions[category]).Translate());
                    PsycheCardUtility.nodeStrings.Add(new Pair<string, int>(text, i));
                    num += Mathf.Max(26f, Text.CalcHeight(text, width));
                }
            }
            Rect viewRect = new Rect(0f, 0f, rect.width, num);
            viewRect.xMax *= 0.9f;
            Widgets.BeginScrollView(rect, ref PsycheCardUtility.nodeScrollPosition, viewRect);
            float num3 = 0f;
            for (int j = 0; j < PsycheCardUtility.nodeStrings.Count; j++)
            {
                string first = PsycheCardUtility.nodeStrings[j].First;
                PersonalityNode node = allNodes[PsycheCardUtility.nodeStrings[j].Second];
                float num4 = Mathf.Max(26f, Text.CalcHeight(first, width));
                Rect rect2 = new Rect(10f, num3, width/3, num4);
                Rect rect3 = new Rect(10f+width/3, num3, (2*width)/3, num4);
                int category = Mathf.RoundToInt(6f*Mathf.InverseLerp(0.16f, 0.83f, allNodes[PsycheCardUtility.nodeStrings[j].Second].AdjustedRating));
                GUI.color = NodeColors[category];
                Widgets.Label(rect2, first.ToString());
                GUI.color = Color.white;
                Widgets.DrawHighlightIfMouseover(rect3);
                Widgets.Label(rect3, node.def.label.CapitalizeFirst());
                TooltipHandler.TipRegion(rect3, () => node.def.description, 613261 + j * 612);
                num3 += num4;
            }
            GUI.EndScrollView();
        }

        private static void DrawSexuality(Rect rect, PsychologyPawn pawn, bool notOnMenu)
        {
            float width = rect.width - 26f - 3f;
            GUI.color = Color.white;
            if (PsychologyBase.ActivateKinsey())
            {
                Text.Font = GameFont.Medium;
                Rect rect2 = rect;
                rect2.yMax = rect2.y + 30f;
                Widgets.Label(rect2, "Sexuality".Translate());
                Text.Font = GameFont.Small;
                Rect rect3 = rect;
                rect3.y = rect2.yMax + RowTopPadding;
                string text = "KinseyRating".Translate() + "    " + pawn.sexuality.kinseyRating;
                float num4 = Mathf.Max(50f, Text.CalcHeight(text, width));
                rect3.yMax = rect3.y + num4;
                Widgets.Label(rect3, text);
                bool asexual = false;
                Rect rect4 = rect;
                if (pawn.sexuality.sexDrive < 0.1f)
                {
                    rect4.y = rect3.yMax;
                    string text2 = "Asexual".Translate();
                    float num5 = Mathf.Max(26f, Text.CalcHeight(text2, width));
                    rect4.yMax = rect4.y + num5;
                    Widgets.Label(rect4, text2);
                    TooltipHandler.TipRegion(rect4, () => "AsexualDescription".Translate(), 613261 + (int)(100 * pawn.sexuality.sexDrive));
                    asexual = true;
                }
                if (pawn.sexuality.romanticDrive < 0.1f)
                {
                    Rect rect5 = rect;
                    rect5.y = (asexual ? rect4.yMax : rect3.yMax);
                    string text2 = "Aromantic".Translate();
                    float num5 = Mathf.Max(26f, Text.CalcHeight(text2, width));
                    rect5.yMax = rect5.y + num5;
                    Widgets.Label(rect5, text2);
                    TooltipHandler.TipRegion(rect5, () => "AromanticDescription".Translate(), 613261 + (int)(100 * pawn.sexuality.romanticDrive));
                }
            }
            else if(notOnMenu)
            {
                GUI.color = Color.red;
                string text = "SexualityDisabledWarning".Translate();
                Widgets.Label(rect, text);
                GUI.color = Color.white;
            }
        }

        public static void DrawDebugOptions(Rect rect, PsychologyPawn pawn)
        {
            GUI.BeginGroup(rect);
            if (Widgets.ButtonText(new Rect((rect.width * 0.6f) - 80f, 0f, 100f, 22f), "EditPsyche".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
            }
            GUI.EndGroup();
        }

        public static void DrawPsycheCard(Rect rect, Pawn pawn)
        {
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if(realPawn != null)
            {
                GUI.BeginGroup(rect);
                Text.Font = GameFont.Small;
                Rect rect2 = new Rect(20f, 20f, rect.width-20f, rect.height-20f);
                Rect rect3 = rect2.ContractedBy(10f);
                Rect rect4 = rect3;
                Rect rect5 = rect3;
                rect4.width *= 0.6f;
                rect5.x = rect4.xMax + 17f;
                rect5.xMax = rect3.xMax;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(rect4.xMax, 0f, rect.height);
                GUI.color = Color.white;
                if (Prefs.DevMode)
                {
                    Rect rect6 = new Rect(0f, 5f, rect3.width, 22f);
                    PsycheCardUtility.DrawDebugOptions(rect6, realPawn);
                }
                PsycheCardUtility.DrawPersonalityNodes(rect4, realPawn);
                PsycheCardUtility.DrawSexuality(rect5, realPawn, true);
                GUI.EndGroup();
            }
        }

        public static void DrawPsycheMenuCard(Rect rect, Pawn pawn)
        {
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if (realPawn != null)
            {
                GUI.BeginGroup(rect);
                Text.Font = GameFont.Small;
                Rect rect2 = new Rect(10f, 10f, rect.width - 10f, rect.height - 10f);
                Rect rect4 = rect2;
                Rect rect5 = rect2;
                rect4.width *= 0.6f;
                rect4.xMin -= 20f;
                rect5.x = rect4.xMax + 17f;
                rect5.xMax = rect.xMax;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(rect4.xMax, 0f, rect.height);
                GUI.color = Color.white;
                PsycheCardUtility.DrawPersonalityNodes(rect4, realPawn);
                PsycheCardUtility.DrawSexuality(rect5, realPawn, false);
                GUI.EndGroup();
            }
        }

        private static string[] NodeDescriptions = { "Not", "Slightly", "Less", "", "More", "Very", "Extremely" };
        private static Color[] NodeColors = { new Color(1f, 0.2f, 0.2f, 0.6f), new Color(1f, 0.4f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.6f, 0.2f), Color.white, new Color(0.6f, 1f, 0.6f, 0.2f), new Color(0.4f, 1f, 0.4f, 0.4f), new Color(0.2f, 1f, 0.2f, 0.6f) };
        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static Vector2 nodeScrollPosition = Vector2.zero;
        private static List<Pair<string, int>> nodeStrings = new List<Pair<string, int>>();
        private const float RowLeftRightPadding = 5f;
        private const float RowTopPadding = 1f;
    }
}