using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Psychology
{
    public static class PersonalityNodeMaker
    {
        public static PersonalityNode MakeNode(PersonalityNodeDef def, Pawn pawn)
        {
            PersonalityNode node = new PersonalityNode(pawn);
            node.def = def;
            node.Initialize();
            return node;
        }
    }
}
