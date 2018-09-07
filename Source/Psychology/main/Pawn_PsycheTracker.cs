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

        [LogPerformance]
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

        [LogPerformance]
        public float GetPersonalityRating(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def).AdjustedRating;
        }

        public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
        {
            return nodes.Find((PersonalityNode n) => n.def == def);
        }

        [LogPerformance]
        public float GetConversationTopicWeight(PersonalityNodeDef def, Pawn otherPawn)
        {
            /* Pawns will avoid controversial topics until they know someone better.
             * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
             */
            IEnumerable<Thought_MemorySocialDynamic> convoMemories;
            float weight = 10f/(Mathf.Lerp(1f+(8*def.controversiality), 1f + (def.controversiality/2), Mathf.Clamp01(this.TotalThoughtOpinion(otherPawn, out convoMemories)/75) + this.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)));
            /* Polite pawns will avoid topics they already know are contentious. */
            Pair<string, string> disagreementKey = new Pair<string,string>(otherPawn.ThingID, def.defName);
            if (cachedDisagreementWeights.ContainsKey(disagreementKey) && !recalcNodeDisagreement[disagreementKey])
            {
                weight *= cachedDisagreementWeights[disagreementKey];
            }
            else
            {
                float knownDisagreements = (from m in convoMemories
                                            where m.opinionOffset < 0f && m.CurStage.label == def.defName
                                            select Math.Abs(m.opinionOffset)).Sum();
                float disagree = 1f;
                if (knownDisagreements > 0)
                {
                    disagree = Mathf.Clamp01(1f / (knownDisagreements / 50)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
                }
                cachedDisagreementWeights[disagreementKey] = disagree;
                recalcNodeDisagreement[disagreementKey] = false;
                weight *= disagree;
            }
            return weight;
        }

        [LogPerformance]
        public float TotalThoughtOpinion(Pawn other, out IEnumerable<Thought_MemorySocialDynamic> convoMemories)
        {
            convoMemories = (from m in this.pawn.needs.mood.thoughts.memories.Memories.OfType<Thought_MemorySocialDynamic>()
                             where m.def.defName.Contains("Conversation") && m.otherPawn == other
                             select m);
            if (cachedOpinions.ContainsKey(other.ThingID) && !recalcCachedOpinions[other.ThingID])
            {
                return cachedOpinions[other.ThingID];
            }
            float knownThoughtOpinion = 1f;
            convoMemories.Do(m => knownThoughtOpinion += Math.Abs(m.opinionOffset));
            cachedOpinions[other.ThingID] = knownThoughtOpinion;
            recalcCachedOpinions[other.ThingID] = false;
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
        private Dictionary<string, float> cachedOpinions = new Dictionary<string, float>();
        public Dictionary<string, bool> recalcCachedOpinions = new Dictionary<string, bool>();
        private Dictionary<Pair<string,string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
        public Dictionary<Pair<string,string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();
        public const int PersonalityCategories = 16;
    }
}
