using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Psychology
{
    static class PsycheHelper
    {
        public static bool PsychologyEnabled(Pawn pawn)
        {
            return pawn != null && pawn.GetComp<CompPsychology>() != null && pawn.GetComp<CompPsychology>().isPsychologyPawn;
        }

        public static CompPsychology Comp(Pawn pawn)
        {
            return pawn.GetComp<CompPsychology>();
        }
    }
}
