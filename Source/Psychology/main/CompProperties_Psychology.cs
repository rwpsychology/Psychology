using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class CompProperties_Psychology : CompProperties
    {
        public CompProperties_Psychology()
        {
            this.compClass = typeof(CompPsychology);
        }
    }
}
