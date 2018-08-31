using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class Candidate : IExposable
    {
        public Candidate()
        {
        }

        public Candidate(Pawn pawn, List<PersonalityNodeDef> nodes)
        {
            this.pawn = pawn;
            this.nodes = nodes;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.pawn, "pawn");
            Scribe_Collections.Look(ref this.nodes, "nodes", LookMode.Def, new object[0]);
        }

        public Pawn pawn;
        public List<PersonalityNodeDef> nodes;
    }
}
