using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology
{
    public class PersonalityNode : IExposable
    {
        public PersonalityNode()
        {
        }

        public PersonalityNode(PsychologyPawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize()
        {
            if (this.Core)
            {
                /* "Core" nodes are seeded based on a pawn's upbringing, separating pawns into 16 categories, similar to the Meyers-Brigg test.
                 * Two pawns with the same upbringing will always have the same core personality ratings.
                 * Pawns will never have conversations about core nodes, they exist only to influence child nodes.
                 */
                int defSeed = 0;
                foreach(char c in this.def.defName)
                {
                    defSeed += c;
                }
                this.rawRating = Rand.ValueSeeded(this.pawn.psyche.upbringing + defSeed + Find.World.info.Seed);
            }
            else
            {
                this.rawRating = Rand.Value;
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.def, "def");
            Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
        }

        public float AdjustForParents(float rating)
        {
            foreach (PersonalityNode parent in this.ParentNodes)
            {
                float parentRating = (def.GetModifier(parent.def) < 0 ? (1f - parent.AdjustedRating) : parent.AdjustedRating) * Mathf.Abs(def.GetModifier(parent.def));
                rating = ((rating * (2f + (1f - Mathf.Abs(def.GetModifier(parent.def))))) + parentRating) / 3f;
            }
            rating += (0.5f - this.rawRating) / 4f;
            return Mathf.Clamp01(rating);
        }

        public float AdjustForCircumstance(float rating)
        {
            if(!this.def.skillModifiers.NullOrEmpty())
            {
                int totalLearning = 0;
                foreach(SkillRecord s in this.pawn.skills.skills)
                {
                    totalLearning += s.Level;
                }
                int skillWeight = 0;
                foreach (PersonalityNodeSkillModifier skillMod in this.def.skillModifiers)
                {
                    skillWeight += this.pawn.skills.GetSkill(skillMod.skill).Level;
                }
                if(totalLearning > 0)
                {
                    float totalWeight = skillWeight / totalLearning;
                    rating += Mathf.InverseLerp(.05f, .4f, totalWeight);
                    rating = Mathf.Clamp01(rating);
                }
            }
            if(!this.def.traitModifiers.NullOrEmpty())
            {
                foreach (PersonalityNodeTraitModifier traitMod in this.def.traitModifiers)
                {
                    if (this.pawn.story.traits.HasTrait(traitMod.trait) && this.pawn.story.traits.DegreeOfTrait(traitMod.trait) == traitMod.degree)
                    {
                        rating += traitMod.modifier;
                    }
                }
                rating = Mathf.Clamp01(rating);
            }
            if (!this.def.incapableModifiers.NullOrEmpty())
            {
                foreach (PersonalityNodeIncapableModifier incapableMod in this.def.incapableModifiers)
                {
                    if (this.pawn.story.WorkTypeIsDisabled(incapableMod.type))
                    {
                        rating += incapableMod.modifier;
                    }
                }
                rating = Mathf.Clamp01(rating);
            }
            if (this.def == PersonalityNodeDefOf.Cool && RelationsUtility.IsDisfigured(this.pawn))
            {
                rating = Mathf.Clamp01(rating - 0.1f);
            }
            return rating;
        }

        public float AdjustGender(float rating)
        {
            if (this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female && this.pawn.sexuality != null && PsychologyBase.ActivateKinsey())
            {
                rating = (Rand.ValueSeeded(pawn.HashOffset()) < 0.8f ? rating * Mathf.Lerp(this.def.femaleModifier, 1f, (this.pawn.sexuality.kinseyRating / 6)) : rating);
            }
            else if(this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female)
            {
                rating = (this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? rating : rating * this.def.femaleModifier);
            }
            return rating;
        }

        public bool Core
        {
            get
            {
                return this.def.ParentNodes.NullOrEmpty();
            }
        }

        public List<PersonalityNode> ParentNodes
        {
            get
            {
                if(this.parents == null || this.pawn.IsHashIntervalTick(500))
                {
                    this.parents = new List<PersonalityNode>();
                    if(!this.def.ParentNodes.NullOrEmpty())
                    {
                        this.parents = (from p in this.pawn.psyche.PersonalityNodes
                                        where this.def.ParentNodes.Contains(p.def)
                                        select p).ToList();
                    }
                }
                return this.parents;
            }
        }

        public string PlatformIssue
        {
            get
            {
                if(this.AdjustedRating >= 0.5f)
                {
                    return this.def.platformIssueHigh;
                }
                else
                {
                    return this.def.platformIssueLow;
                }
            }
        }

        /* Hook for modding. */
        public float AdjustHook(float rating)
        {
            return rating;
        }

        public float AdjustedRating
        {
            get
            {
                if(cachedRating < 0f || this.pawn.IsHashIntervalTick(100))
                {
                    float adjustedRating = AdjustForCircumstance(this.rawRating);
                    adjustedRating = AdjustHook(adjustedRating);
                    adjustedRating = AdjustGender(adjustedRating);
                    adjustedRating = AdjustForParents(adjustedRating);
                    cachedRating = Mathf.Clamp01(adjustedRating);
                }
                return cachedRating;
            }
        }

        public PsychologyPawn pawn;
        public PersonalityNodeDef def;
        public float rawRating;
        public float cachedRating = -1f;
        private List<PersonalityNode> parents;
    }
}
