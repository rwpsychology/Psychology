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

        public PersonalityNode(Pawn pawn)
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
                int defSeed = this.def.defName.GetHashCode();
                this.rawRating = Rand.ValueSeeded(this.pawn.GetComp<CompPsychology>().Psyche.upbringing + defSeed + Find.World.info.Seed);
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

        [LogPerformance]
        public float AdjustForParents(float rating)
        {
            foreach (PersonalityNode parent in this.ParentNodes)
            {
                float parentRating = (def.GetModifier(parent.def) < 0 ? (1f - parent.AdjustedRating) : parent.AdjustedRating) * Mathf.Abs(def.GetModifier(parent.def));
                rating = ((rating * (1f + (1f - Mathf.Abs(def.GetModifier(parent.def))))) + parentRating) / 2f;
            }
            return Mathf.Clamp01(rating);
        }

        [LogPerformance]
        public float AdjustForCircumstance(float rating)
        {
            if (this.def.traitModifiers != null && this.def.traitModifiers.Any())
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
            if (this.def.skillModifiers != null && this.def.skillModifiers.Any())
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
            if (this.def.incapableModifiers != null && this.def.incapableModifiers.Any())
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

        [LogPerformance]
        public float AdjustGender(float rating)
        {
            if (this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female && PsychologyBase.ActivateKinsey())
            {
                rating = (Rand.ValueSeeded(pawn.HashOffset()) < 0.8f ? rating * Mathf.Lerp(this.def.femaleModifier, 1f, (this.pawn.GetComp<CompPsychology>().Sexuality.kinseyRating / 6)) : rating);
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
                return this.def.ParentNodes == null || !this.def.ParentNodes.Any();
            }
        }

        public HashSet<PersonalityNode> ParentNodes
        {
            [LogPerformance]
            get
            {
                if(this.parents == null || this.pawn.IsHashIntervalTick(500))
                {
                    this.parents = new HashSet<PersonalityNode>();
                    if(this.def.ParentNodes != null && this.def.ParentNodes.Any())
                    {
                        this.parents = (from p in this.pawn.GetComp<CompPsychology>().Psyche.PersonalityNodes
                                        where this.def.ParentNodes.ContainsKey(p.def)
                                        select p) as HashSet<PersonalityNode>;
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
            [LogPerformance]
            get
            {
                if(cachedRating < 0f || this.pawn.IsHashIntervalTick(100))
                {
                    float adjustedRating = AdjustForCircumstance(this.rawRating);
                    adjustedRating = AdjustHook(adjustedRating);
                    adjustedRating = AdjustGender(adjustedRating);
                    if (this.ParentNodes != null && this.ParentNodes.Any())
                    {
                        adjustedRating = AdjustForParents(adjustedRating);
                    }
                    adjustedRating = ((3 * adjustedRating) + rawRating) / 4f; //Prevent it from being adjusted too strongly
                    cachedRating = Mathf.Clamp01(adjustedRating);
                }
                return cachedRating;
            }
        }

        public override int GetHashCode()
        {
            return this.def.defName.GetHashCode();
        }

        public Pawn pawn;
        public PersonalityNodeDef def;
        public float rawRating;
        public float cachedRating = -1f;
        private HashSet<PersonalityNode> parents;
    }
}
