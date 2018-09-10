using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class ITab_Pawn_Psyche : ITab
    {
        public ITab_Pawn_Psyche()
        {
            this.size = new Vector2(500f, 470f);;
            this.labelKey = "TabPsyche";
            this.tutorTag = "Psyche";
        }

        protected override void FillTab()
        {
            PsycheCardUtility.DrawPsycheCard(new Rect(0f, 0f, this.size.x, this.size.y), this.PawnToShowInfoAbout);
        }
        
        public override bool IsVisible
        {
            get
            {
                return PsycheHelper.PsychologyEnabled(this.PawnToShowInfoAbout);
            }
        }
        
        private Pawn PawnToShowInfoAbout
        {
            get
            {
                Pawn pawn = null;
                if (base.SelPawn != null)
                {
                    pawn = base.SelPawn;
                }
                else
                {
                    Corpse corpse = base.SelThing as Corpse;
                    if (corpse != null)
                    {
                        pawn = corpse.InnerPawn;
                    }
                }
                if (pawn == null)
                {
                    Log.Error("Psyche tab found no selected pawn to display.");
                    return null;
                }
                return pawn;
            }
        }
    }
}
