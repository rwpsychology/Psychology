using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace Psychology
{
    public class PawnGraphicSet_Invisible : PawnGraphicSet
    {
        public new void ResolveAllGraphics()
        {
            this.ClearCache();
            if (this.pawn.RaceProps.Humanlike)
            {
                this.nakedGraphic = new Graphic_Invisible();
                this.rottingGraphic = new Graphic_Invisible();
                this.dessicatedGraphic = new Graphic_Invisible();
                this.headGraphic = null;
                this.desiccatedHeadGraphic = new Graphic_Invisible();
                this.skullGraphic = new Graphic_Invisible();
                this.hairGraphic = new Graphic_Invisible();
                this.ResolveApparelGraphics();
            }
            else
            {
                PawnKindLifeStage curKindLifeStage = this.pawn.ageTracker.CurKindLifeStage;
                this.nakedGraphic = new Graphic_Invisible();
                this.rottingGraphic = new Graphic_Invisible();
                if (this.pawn.RaceProps.packAnimal)
                {
                    this.packGraphic = new Graphic_Invisible();
                }
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    this.dessicatedGraphic = new Graphic_Invisible();
                }
            }
        }

        public new void ResolveApparelGraphics()
        {
            this.ClearCache();
            this.apparelGraphics.Clear();
        }

        public PawnGraphicSet_Invisible(Pawn pawn) : base(pawn)
        {
            this.pawn = pawn;
            this.ResolveAllGraphics();
        }
    }
}
