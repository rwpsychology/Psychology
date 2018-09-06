using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Dialog_ViewPsyche : Window
    {

        private Pawn pawn;

        public Dialog_ViewPsyche(Pawn editFor)
        {
            pawn = editFor;
        }

        [LogPerformance]
        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                flag = true;
                Event.current.Use();
            }
            Rect windowRect = inRect.ContractedBy(17f);
            Rect mainRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height - 20f);
            Rect okRect = new Rect(inRect.width / 3, mainRect.yMax + 10f, inRect.width / 3f, 30f);
            PsycheCardUtility.DrawPsycheMenuCard(mainRect, pawn);
            if (Widgets.ButtonText(okRect, "CloseButton".Translate(), true, false, true) || flag)
            {
                this.Close(true);
            }
        }
    }
}
