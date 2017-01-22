using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;

namespace Psychology
{
    public class Alert_Homesick : Alert_Thought
    {
        public Alert_Homesick()
        {
            this.explanationKey = "HomesickDesc";
        }
        
        public override string GetLabel()
        {
            if ((int)((IEnumerable<Pawn>)typeof(Alert_Thought).GetMethod("AffectedPawns", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { })).Count() == 1)
            {
                return "ColonistHomesickAlert".Translate();
            }
            return "ColonistsHomesickAlert".Translate();
        }

        protected override ThoughtDef Thought
        {
            get
            {
                return ThoughtDefOfPsychology.Homesickness;
            }
        }

    }
}
