using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class DirectPawnRelationDynamic : DirectPawnRelation, IExposable
    {
        public DirectPawnRelationDynamic()
        {
        }
        
        public DirectPawnRelationDynamic(PawnRelationDef def, Pawn otherPawn, int startTicks)
        {
            this.def = def;
            this.otherPawn = otherPawn;
            this.startTicks = startTicks;
        }

        new public void ExposeData()
        {
            if(this.def != null)
            {
                this.def.defName = "PawnRelationDynamic";
            }
            Scribe_Defs.LookDef(ref this.def, "def");
            Scribe_References.LookReference(ref this.otherPawn, "otherPawn", true);
            Scribe_Values.LookValue(ref this.startTicks, "startTicks", 0, false);
            Scribe_Values.LookValue(ref this.name, "trueName", "PawnRelationDynamic");
            Scribe_Values.LookValue(ref this.label, "label", "dynamic relation");
            Scribe_Values.LookValue(ref this.opinionOffset, "opinion", 0);
            Scribe_Values.LookValue(ref this.importance, "importance", 0f);
            PawnRelationDef def = new PawnRelationDef();
            def.defName = this.name;
            def.label = this.label;
            def.opinionOffset = this.opinionOffset;
            def.importance = this.importance;
            def.implied = false;
            def.reflexive = false;
            def.workerClass = typeof(DirectPawnRelationDynamic);
            this.def = def;
        }
        
        private string name;
        private string label;
        private int opinionOffset;
        private float importance;
    }
}
