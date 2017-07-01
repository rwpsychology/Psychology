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
    public class MentalState_FellPlotting : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (target == null)
            {
                List<Pawn> rivals = (from c in pawn.Map.mapPawns.FreeColonistsSpawned
                                     where pawn.relations.OpinionOf(c) < -20
                                     select c).ToList();
                rivals.SortByDescending((Pawn x) => pawn.relations.OpinionOf(x));
                if (rivals.Count == 0)
                {
                    Log.ErrorOnce(pawn.LabelShort + " was in Fell Plotting mental break but has no rivals.", 100 + pawn.GetHashCode());
                    this.RecoverFromState();
                    return;
                }
                target = rivals.First();
            }
            else if(enactingPlot && target.Dead)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.SuccessfulPlot, target);
                this.RecoverFromState();
                return;
            }
            if(pawn.IsHashIntervalTick(500) && !enactingPlot)
            {
                string[] murmurings = { "HISCAP fault...", "This is all HIS fault...", "I can't let HIM get away with it...", "HECAP thinks HE's smart, does HE?", "I'll show HIM...", "Traitors... saboteurs... conspirators...",
                    "HECAP mocks me...", "How dare HE...", "HECAP thinks I don't notice?", "I must do this myself... no one can be trusted.", "For the good of us all...", "I'll solve this problem myself...", "[muttering]",
                    "[growling]", "[hissing]", "[murmuring]", "[incomprehensible babbling]", "Idiot!", "Fiend!", "Vulture!", "Monster!", "Liar!", "Behind my back... always behind my back...", "The source of all our problems...",
                    "HECAP will never see it coming...", "I'll plan it all out...", "I'll think of everything...", "I'll wait until the time is right...", "I will cure this place of its blight!", "If no one else will do it, I will.",
                    "None of them see it... they're all so blind...", "I can do this... I must do this...", "Even now, HE laughs at me...", "Serves HIM right...", "HECAP will be the death of me...", "I should have done this long ago..." };
                if(Rand.Value < 0.25f)
                {
                    Vector3 pos = pawn.DrawPos + pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation);
                    MoteMaker.ThrowText(pos, pawn.Map, murmurings.RandomElement().AdjustedFor(target), Color.grey, 2.5f);
                }
            }
        }

        public Pawn target;
        public bool enactingPlot = false;
    }
}
