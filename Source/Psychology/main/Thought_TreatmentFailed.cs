using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class Thought_TreatmentFailed : Thought_Memory
    {
        public override string LabelCap
        {
            get
            {
                return string.Format(base.CurStage.label, traitName).CapitalizeFirst();
            }
        }

        public string traitName = "mental illness";
    }
}
