using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psychology
{
    public static class PersonalityNodeMaker
    {
        public static PersonalityNode MakeNode(PersonalityNodeDef def, PsychologyPawn pawn)
        {
            PersonalityNode node = new PersonalityNode(pawn);
            node.def = def;
            node.Initialize();
            return node;
        }
    }
}
