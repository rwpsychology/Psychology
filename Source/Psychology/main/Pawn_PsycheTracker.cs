using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using UnityEngine;

namespace Psychology
{
    public class Pawn_PsycheTracker : IExposable
    {
        public Pawn_PsycheTracker(PsychologyPawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize()
        {
            Rand.PushSeed();
            Rand.Seed = this.pawn.HashOffset();
            this.upbringing = Rand.RangeInclusive(1, PersonalityCategories);
            Rand.PopSeed();
            this.nodes = new List<PersonalityNode>();
            DefDatabase<PersonalityNodeDef>.AllDefsListForReading.ForEach((PersonalityNodeDef def) => nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn)));
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref this.upbringing, "upbringing", 0, false);
            Scribe_Values.LookValue(ref this.lastDateTick, "lastDateTick", 0, false);
            Scribe_Collections.LookList(ref this.nodes, "nodes", LookMode.Deep, new object[] { this.pawn });
        }
        
        public float GetPersonalityRating(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def).AdjustedRating;
        }

        public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def);
        }

        public float GetConversationTopicWeight(PersonalityNodeDef def, PsychologyPawn otherPawn)
        {
            /* Pawns will avoid controversial topics until they know someone better.
             * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
             */
            float weight = 10f/(Mathf.Lerp(1f+(4*def.controversiality), 1f + (def.controversiality/2), Mathf.Clamp01(this.TotalThoughtOpinion(otherPawn)/75) + this.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)));
            /* Polite pawns will avoid topics they already know are contentious. */
            float knownDisagreements = 0f;
            foreach(Thought_MemorySocialDynamic memory in this.pawn.needs.mood.thoughts.memories.Memories.Where(m => m is Thought_MemorySocialDynamic && m.def.defName.Contains("Conversation")))
            {
                if(memory.CurStage.label == def.defName && memory.opinionOffset < 0f)
                {
                    knownDisagreements += Mathf.Abs(memory.opinionOffset);
                }
            }
            weight *= Mathf.Clamp01(1f / (knownDisagreements / 50)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
            return weight;
        }

        public float TotalThoughtOpinion(PsychologyPawn other)
        {
            float knownThoughtOpinion = 1f;
            this.pawn.needs.mood.thoughts.memories.Memories.Where(m => m.def.defName.Contains("Conversation") && m.otherPawn.ThingID == other.ThingID).ToList().ForEach(m => knownThoughtOpinion += Mathf.Abs(m.CurStage.baseOpinionOffset));
            return knownThoughtOpinion;
        }

        public List<PersonalityNode> PersonalityNodes
        {
            get
            {
                return nodes;
            }
            set
            {
                nodes = value;
            }
        }

        public int upbringing;
        public int lastDateTick = 0;
        private PsychologyPawn pawn;
        private List<PersonalityNode> nodes;
        private const int PersonalityCategories = 16;
    }
}
