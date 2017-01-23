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
            this.size = CharacterCardUtility.PawnCardSize + new Vector2(17f, 17f) * 2f;
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
                return this.PawnToShowInfoAbout is PsychologyPawn && ((PsychologyPawn)this.PawnToShowInfoAbout).psyche != null;
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
                    Log.Error("Character tab found no selected pawn to display.");
                    return null;
                }
                return pawn;
            }
        }
    }
}
