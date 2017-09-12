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
    public class Recipe_TreatInsomnia : Recipe_Treatment
    {
        public Recipe_TreatInsomnia() : base(TraitDefOfPsychology.Insomniac, HediffDefOfPsychology.SleepingPills, "insomnia", 1f, TaleDefOfPsychology.TreatedInsomnia) { }
    }
}
