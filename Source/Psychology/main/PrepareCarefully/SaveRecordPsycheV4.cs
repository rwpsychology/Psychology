using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology.PrepareCarefully
{
    public class SaveRecordPsycheV4 : IExposable
    {
        public HashSet<PersonalityNode> nodes = new HashSet<PersonalityNode>();
        private Dictionary<PersonalityNodeDef, PersonalityNode> nodeDict;
        public int upbringing;
        public float sexDrive = 1f;
        public float romanticDrive = 1f;
        public int kinseyRating = 0;

        public SaveRecordPsycheV4()
        {
        }

        public SaveRecordPsycheV4(Pawn pawn)
        {
            if(PsycheHelper.PsychologyEnabled(pawn))
            {
                nodes = PsycheHelper.Comp(pawn).Psyche.PersonalityNodes;
                upbringing = PsycheHelper.Comp(pawn).Psyche.upbringing;
                if(PsychologyBase.ActivateKinsey())
                {
                    sexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                    romanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
                    kinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                }
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["personality"] != null)
            {
                Scribe_Collections.Look(ref nodes, "personality", LookMode.Deep);
            }
            Scribe_Values.Look(ref upbringing, "upbringing");
            if(PsychologyBase.ActivateKinsey())
            {
                if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["sexDrive"] != null)
                {
                    Scribe_Values.Look(ref sexDrive, "sexDrive", 1f);
                }
                if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["romanticDrive"] != null)
                {
                    Scribe_Values.Look(ref romanticDrive, "romanticDrive", 1f);
                }
                if (Scribe.mode == LoadSaveMode.Saving || Scribe.loader.curXmlParent["kinseyRating"] != null)
                {
                    Scribe_Values.Look(ref kinseyRating, "kinseyRating", 0);
                }
            }
        }

        public Dictionary<PersonalityNodeDef, PersonalityNode> NodeDict
        {
            get
            {
                if (this.nodeDict == null)
                {
                    this.nodeDict = new Dictionary<PersonalityNodeDef, PersonalityNode>();
                    if (this.nodes != null && this.nodes.Count > 0)
                    {
                        foreach (PersonalityNode parent in this.nodes)
                        {
                            this.nodeDict.Add(parent.def, parent);
                        }
                    }
                }
                return this.nodeDict;
            }
        }
    }
}