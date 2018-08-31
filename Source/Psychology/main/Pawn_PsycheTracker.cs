using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;
using UnityEngine;

namespace Psychology
{
    public class Pawn_PsycheTracker : IExposable
    {
        public Pawn_PsycheTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize()
        {
            this.upbringing = Mathf.RoundToInt(Rand.ValueSeeded(this.pawn.HashOffset()) * (PersonalityCategories-1))+1;
            this.nodes = new List<PersonalityNode>();
            foreach(PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
            {
                nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.upbringing, "upbringing", 0, false);
            Scribe_Values.Look(ref this.lastDateTick, "lastDateTick", 0, false);
            Scribe_Collections.Look(ref this.nodes, "nodes", LookMode.Deep, new object[] { this.pawn });
        }
        
        public float GetPersonalityRating(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def).AdjustedRating;
        }

        public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def);
        }

        public float GetConversationTopicWeight(PersonalityNodeDef def, Pawn otherPawn)
        {
            /* Pawns will avoid controversial topics until they know someone better.
             * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
             */
            float weight = 10f/(Mathf.Lerp(1f+(8*def.controversiality), 1f + (def.controversiality/2), Mathf.Clamp01(this.TotalThoughtOpinion(otherPawn)/75) + this.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)));
            /* Polite pawns will avoid topics they already know are contentious. */
            float knownDisagreements = 0f;
            IEnumerable<Thought_MemorySocialDynamic> allConvos = (from m in this.pawn.needs.mood.thoughts.memories.Memories.OfType<Thought_MemorySocialDynamic>()
                                                                  where m.def.defName.Contains("Conversation")
                                                                  select m);
            foreach (Thought_MemorySocialDynamic memory in allConvos)
            {
                if(memory.CurStage.label == def.defName && memory.opinionOffset < 0f)
                {
                    knownDisagreements += Mathf.Abs(memory.opinionOffset);
                }
            }
            weight *= Mathf.Clamp01(1f / (knownDisagreements / 50)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
            return weight;
        }

        public float TotalThoughtOpinion(Pawn other)
        {
            float knownThoughtOpinion = 1f;
            if (this.pawn != null)
            {
                IEnumerable<Thought_Memory> convos = (from m in this.pawn.needs.mood.thoughts.memories.Memories
                                                      where m.def.defName.Contains("Conversation") && m.otherPawn.ThingID == other.ThingID
                                                      select m);
                foreach (Thought_Memory m in convos)
                {
                    if (m == null)
                    {
                        break;
                    }
                    knownThoughtOpinion += Mathf.Abs(m.CurStage.baseOpinionOffset);
                }
            }
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
        private Pawn pawn;
        private List<PersonalityNode> nodes;
        public const int PersonalityCategories = 16;
    }
}
