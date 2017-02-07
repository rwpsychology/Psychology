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
                Rand.PushSeed();
                int defSeed = 0;
                this.def.defName.ToList().ForEach((char c) => defSeed += c);
                Rand.Seed = this.pawn.psyche.upbringing+defSeed+Find.World.info.Seed;
                this.rawRating = Rand.Value;
                Rand.PopSeed();
            }
            else
            {
                this.rawRating = Rand.Value;
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.LookDef(ref this.def, "def");
            Scribe_Values.LookValue(ref this.rawRating, "rawRating", -1f, false);
        }

        public float AdjustForParents(float rating)
        {
            foreach (PersonalityNode parent in this.ParentNodes)
            {
                rating = ((rating * 2) + parent.AdjustedRating) / 3;
            }
            return Mathf.Clamp01(rating);
        }

        public float AdjustForCircumstance(float rating)
        {
            if(!this.def.skillModifiers.NullOrEmpty())
            {
                int totalLearning = 0;
                this.pawn.skills.skills.ForEach((SkillRecord s) => totalLearning += s.Level);
                int skillWeight = 0;
                this.def.skillModifiers.ForEach((PersonalityNodeSkillModifier skillMod) => skillWeight += this.pawn.skills.GetSkill(skillMod.skill).Level);
                float totalWeight = skillWeight / totalLearning;
                rating += Mathf.InverseLerp(.05f, .4f, totalWeight);
                rating = Mathf.Clamp01(rating);
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
                rating = Mathf.Clamp01(rating - 10);
            }
            return rating;
        }

        public float AdjustGender(float rating)
        {
            if (this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female && this.pawn.sexuality != null && PsychologyBase.ActivateKinsey())
            {
                Rand.PushSeed();
                Rand.Seed = this.pawn.HashOffset();
                rating = (Rand.Value < 0.8f ? rating * Mathf.Lerp(this.def.femaleModifier, 1f, (this.pawn.sexuality.kinseyRating / 6)) : rating);
                Rand.PopSeed();
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
                    float adjustedRating = AdjustForParents(this.rawRating);
                    adjustedRating = AdjustForCircumstance(adjustedRating);
                    adjustedRating = AdjustHook(adjustedRating);
                    adjustedRating = AdjustGender(adjustedRating);
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
