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
    public class MentalState_Paranoia : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if(pawn.IsHashIntervalTick(1000))
            {
                string[] ramblings = { "You goddamned idiots! We're doomed!", "They're coming! They'll destroy us!", "This place is cursed!", "We're all going to die!", "Curse this place!", "Our old lives are gone. We are shadows of the dead!",
                    "This miserable place will be our tomb!", "The sooner we die, the sooner the suffering ends!", "You fools! You are puppets on strings!", "Can't you see? We are mere playthings!", "They laugh at our torment! They cackle!",
                    "Are you happy now? We are all going to die!", "You stand a stone's throw from oblivion!", "The worst is yet to come! We are not prepared!", "It's all hopeless! We're wasting our time!", "Why are we here? Just to suffer?",
                    "Our minds are not our own!", "Your foolishness has doomed us all!", "Run, while you still can!", "This accursed hellhole will be our grave!", "We are mere pawns in the hands of fate!", "I'm wasting my time. You are all blind and deaf!",
                    "It will never get better! We will never be saved!", "We have never been more alone and unwanted!", "We work, we sicken, we die, and for what?", "Any one of you could die at any moment!", "We are all better off dead!",
                    "This is just a dream, we can still wake up!", "Every moment you draw breath prolongs the agony!", "If anyone was looking out for us, do you really think we would be in such dire straits?", "Consciousness is a festering wound!" };
                if(Rand.Value < 0.75f)
                {
                    Vector3 pos = pawn.DrawPos + pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation);
                    MoteMaker.ThrowText(pos, pawn.Map, ramblings.RandomElement(), Color.Lerp(Color.black, Color.red, 0.85f), 3.85f);
                    foreach (Pawn p in pawn.Map.mapPawns.AllPawns)
                    {
                        if(p.RaceProps.Humanlike && p != pawn && (pawn.Position - p.Position).LengthHorizontalSquared <= 36f && GenSight.LineOfSight(pawn.Position, p.Position, pawn.Map, true))
                        {
                            p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.HeardParanoia);
                        }
                    }
                }
            }
        }
    }
}
