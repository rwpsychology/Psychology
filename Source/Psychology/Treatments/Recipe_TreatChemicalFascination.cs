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
    public class Recipe_TreatChemicalFascination : Recipe_Treatment
    {
        public Recipe_TreatChemicalFascination() : base(TraitDefOf.DrugDesire, HediffDefOfPsychology.DrugFree, "chemical fascination", 1f, TaleDefOfPsychology.TreatedDrugDesire, 2) { }
    }
}
