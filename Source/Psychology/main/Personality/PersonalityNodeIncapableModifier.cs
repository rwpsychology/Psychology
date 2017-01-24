using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class PersonalityNodeIncapableModifier
    {
        //The work tag that affects this node.
        public WorkTypeDef type;
        //How much having this trait adds or subtracts from this node.
        public float modifier;
    }
}
