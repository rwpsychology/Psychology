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
    public class Recipe_TreatDepression: Recipe_Treatment
    {
        public Recipe_TreatDepression() : base(DefDatabase<TraitDef>.GetNamed("NaturalMood"), HediffDefOfPsychology.Antidepressants, "depression", 1.5f, TaleDefOfPsychology.TreatedDepression, -2) { }
    }
}
