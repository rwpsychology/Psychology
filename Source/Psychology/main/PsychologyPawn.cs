using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class PsychologyPawn : Pawn
    {

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep<Pawn_SexualityTracker>(ref this.sexuality, "sexuality", new object[]
            {
                this
            });
            Scribe_Deep.LookDeep<Pawn_PsycheTracker>(ref this.psyche, "psyche", new object[]
            {
                this
            });
        }

        public Pawn_SexualityTracker sexuality;
        public Pawn_PsycheTracker psyche;
    }
}
