using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Diagnostics;
using UnityEngine;

namespace Psychology
{
    public class Recipe_TreatPyromaniac : Recipe_Treatment
    {
        public Recipe_TreatPyromaniac() : base(TraitDefOf.Pyromaniac, HediffDefOfPsychology.RecoveringPyromaniac, "pyromania", 1f, TaleDefOfPsychology.TreatedPyromania) { }
    }
}
