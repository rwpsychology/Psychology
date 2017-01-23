using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class PersonalityNodeTraitModifier
    {
        //The trait that affects this node.
        public TraitDef trait;
        //The required degree of the trait.
        public int degree;
        //How much having this trait adds or subtracts from this node.
        public float modifier;
    }
}
