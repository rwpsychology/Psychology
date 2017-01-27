using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Alert_Individuality : Alert_Thought
    {
        public Alert_Individuality()
        {
            this.defaultLabel = "Individuality".Translate();
            this.explanationKey = "IndividualityDesc";
        }

        protected override ThoughtDef Thought
        {
            get
            {
                return ThoughtDefOfPsychology.Individuality;
            }
        }

    }
}
