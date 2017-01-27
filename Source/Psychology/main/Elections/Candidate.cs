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

        public Candidate(PsychologyPawn pawn, List<PersonalityNodeDef> nodes)
        {
            this.pawn = pawn;
            this.nodes = nodes;
        }

        public void ExposeData()
        {
            Scribe_References.LookReference(ref this.pawn, "pawn");
            Scribe_Collections.LookList(ref this.nodes, "nodes", LookMode.Def, new object[0]);
        }

        public PsychologyPawn pawn;
        public List<PersonalityNodeDef> nodes;
    }
}
