using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class PersonalityNodeSkillModifier
    {
        /* A skill that affects this node.
         * The node will get a boost based on the skill's weight among all the pawn's skill levels.
         * The higher percentage of a pawn's skill levels this skill occupies, the higher the boost.
         * All combined affecting skills must be weighted more than 10% to get any boost.
         * At 50% the node is maxed.
         */
        public SkillDef skill;
    }
}
