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
    public class Recipe_TreatChemicalInterest : Recipe_Treatment
    {
        public Recipe_TreatChemicalInterest() : base(TraitDefOf.DrugDesire, HediffDefOfPsychology.DrugFree, "chemical interest", 1.25f, TaleDefOfPsychology.TreatedDrugDesire, 1) { }
    }
}
