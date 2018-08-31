using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Dialog_EditPsyche : Window
    {
        private Pawn pawn;

        public Dialog_EditPsyche(Pawn editFor)
        {
            pawn = editFor;
            if(PsychologyBase.ActivateKinsey())
            {
                pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            }
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                cachedList.Add(new Pair<string, float>(node.def.label.CapitalizeFirst(), node.rawRating));
                try
                {
                    descriptions.Add(node.def.label.CapitalizeFirst(), node.def.description);
                }
                catch(ArgumentException e)
                {
                    Log.Error("[Psychology] "+"DuplicateDefLabel".Translate(node.def.defName));
                    descriptions.Add(node.def.defName.CapitalizeFirst(), node.def.description);
                }
            }
            cachedList.SortBy(n => n.First);
        }

        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            bool flag2 = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                flag2 = true;
                Event.current.Use();
            }
            Rect windowRect = inRect.ContractedBy(17f);
            Rect mainRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height - 20f);
            Rect okRect = new Rect(inRect.width/4 - 20f, mainRect.yMax+10f, inRect.width / 4f, 30f);
            Rect cancelRect = new Rect(okRect.xMax + 40f, mainRect.yMax+10f, inRect.width / 4f, 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(mainRect, "PsycheEditor".Translate(pawn.LabelShort));
            mainRect.yMin += 35f;
            Text.Font = GameFont.Small;
            float warningSize = Mathf.Max(50f, Text.CalcHeight("PersonalityNodeWarning".Translate(), mainRect.width));
            Widgets.Label(mainRect, "PersonalityNodeWarning".Translate());
            mainRect.yMin += warningSize;
            float labelSize = Mathf.Max(26f, Text.CalcHeight("SexDrive".Translate(), mainRect.width));
            Rect nodeRect = new Rect(mainRect.x, mainRect.y, mainRect.width, mainRect.height - labelSize*3 - 20f);
            Rect sexDriveRect = new Rect(mainRect.x, nodeRect.yMax + 10f, mainRect.width, labelSize);
            Rect romanticDriveRect = new Rect(mainRect.x, sexDriveRect.yMax, mainRect.width, labelSize);
            Rect kinseyRect = new Rect(mainRect.x, romanticDriveRect.yMax, mainRect.width, labelSize+10f);
            Widgets.DrawLineHorizontal(nodeRect.x, nodeRect.yMax, nodeRect.width);
            float num = 0f;
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                num += Mathf.Max(26f, Text.CalcHeight(node.def.label, nodeRect.width));
            }
            Rect viewRect = new Rect(0f, 0f, nodeRect.width-20f, num);
            Widgets.BeginScrollView(nodeRect, ref nodeScrollPosition, viewRect);
            float num3 = 0f;
            for(int i = 0; i < cachedList.Count; i++)
            {
                string label = cachedList[i].First;
                float num4 = Mathf.Max(26f, Text.CalcHeight(label, viewRect.width));
                Rect rect = new Rect(10f, num3, viewRect.width / 3, num4);
                Rect rect2 = new Rect(10f + viewRect.width / 3, num3, ((2 * viewRect.width) / 3)-20f, num4);
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.Label(rect, label);
                TooltipHandler.TipRegion(rect, () => descriptions[label], 436532 + Mathf.RoundToInt(num3));
                float newVal = Widgets.HorizontalSlider(rect2, cachedList[i].Second, 0f, 1f, true);
                cachedList[i] = new Pair<string, float>(cachedList[i].First, newVal);
                num3 += num4;
            }
            Widgets.EndScrollView();
            if(PsychologyBase.ActivateKinsey())
            {
                Rect sexDriveLabelRect = new Rect(sexDriveRect.x, sexDriveRect.y, sexDriveRect.width / 3, sexDriveRect.height);
                Rect sexDriveSliderRect = new Rect(sexDriveRect.x + (sexDriveRect.width / 3), sexDriveRect.y, (2 * sexDriveRect.width) / 3, sexDriveRect.height);
                Widgets.Label(sexDriveLabelRect, "SexDrive".Translate());
                pawnSexDrive = Widgets.HorizontalSlider(sexDriveSliderRect, pawnSexDrive, 0f, 1f, true);
                Rect romanticDriveLabelRect = new Rect(romanticDriveRect.x, romanticDriveRect.y, romanticDriveRect.width / 3, romanticDriveRect.height);
                Rect romanticDriveSliderRect = new Rect(romanticDriveRect.x + (romanticDriveRect.width / 3), romanticDriveRect.y, (2 * romanticDriveRect.width) / 3, romanticDriveRect.height);
                Widgets.Label(romanticDriveLabelRect, "RomanticDrive".Translate());
                pawnRomanticDrive = Widgets.HorizontalSlider(romanticDriveSliderRect, pawnRomanticDrive, 0f, 1f, true);
                Rect kinseyRatingLabelRect = new Rect(kinseyRect.x, kinseyRect.y, kinseyRect.width / 3, kinseyRect.height);
                Rect kinseyRatingSliderRect = new Rect(kinseyRect.x + (kinseyRect.width / 3), kinseyRect.y, (2 * kinseyRect.width) / 3, kinseyRect.height);
                Widgets.Label(kinseyRatingLabelRect, "KinseyRating".Translate());
                pawnKinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(kinseyRatingSliderRect, pawnKinseyRating, 0f, 6f, true, leftAlignedLabel: "0", rightAlignedLabel: "6"));
            }
            else
            {
                GUI.color = Color.red;
                Rect warningRect = new Rect(mainRect.x, nodeRect.yMax + 10f, mainRect.width, labelSize*3);
                Widgets.Label(warningRect, "SexualityDisabledWarning".Translate());
                GUI.color = Color.white;
            }
            if (Widgets.ButtonText(okRect, "AcceptButton".Translate(), true, false, true) || flag)
            {
                foreach(PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
                {
                    node.rawRating = (from n in cachedList
                                      where n.First == node.def.label.CapitalizeFirst()
                                      select n).First().Second;
                }
                if(PsychologyBase.ActivateKinsey())
                {
                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = pawnSexDrive;
                    PsycheHelper.Comp(pawn).Sexuality.romanticDrive = pawnRomanticDrive;
                    PsycheHelper.Comp(pawn).Sexuality.kinseyRating = pawnKinseyRating;
                }
                this.Close(false);
            }
            if (Widgets.ButtonText(cancelRect, "CancelButton".Translate(), true, false, true) || flag2)
            {
                this.Close(true);
            }
        }

        private static Vector2 nodeScrollPosition = Vector2.zero;
        private List<Pair<string, float>> cachedList = new List<Pair<string, float>>();
        private Dictionary<string, string> descriptions = new Dictionary<string, string>();
        private int pawnKinseyRating = 0;
        private float pawnSexDrive = 0f;
        private float pawnRomanticDrive = 0f;
    }
}
