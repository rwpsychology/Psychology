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
    public class Pawn_SexualityTracker : IExposable
    {
        public Pawn_SexualityTracker(PsychologyPawn pawn)
        {
            this.pawn = pawn;
        }

        public bool IncompatibleSexualityKnown(Pawn recipient)
        {
            foreach(Pawn key in this.knownSexualities.Keys)
            {
                if(recipient == key)
                {
                    return ((knownSexualities[recipient]-4) >= 0) != (recipient.gender == this.pawn.gender);
                }
            }
            return false;
        }

        public void LearnSexuality(PsychologyPawn p)
        {
            if(!knownSexualities.Keys.Contains(p))
            {
                knownSexualities.Add(p, p.sexuality.kinseyRating);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.kinseyRating, "kinseyRating", 0, false);
            Scribe_Collections.LookDictionary<Pawn,int>(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
        }

        /*
         * Average roll: 0.989779
         * Percent chance of rolling each number:
         * 0: 62.4949 %
         * 1: 11.3289 %
         * 2: 9.2658 %
         * 3: 6.8466 %
         * 4: 4.522 %
         * 5: 2.7612 %
         * 6: 2.7806 %
         * Percent chance of being predominantly straight: 83.08959999999999 %
         * Percent chance of being predominantly gay: 10.0638 %
         * Percent chance of being more or less straight: 73.82379999999999 %
         * Percent chance of being more or less bisexual: 20.6344 %
         * Percent chance of being more or less gay: 5.5418 %
         * Sample size: 100,000
         */
        public int RandKinsey()
        {
            if(PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Realistic)
            {
                return Mathf.Clamp((int)Rand.GaussianAsymmetric(0f, 1f, 3.13f), 0, 6);
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Invisible)
            {
                return Mathf.Clamp((int)Rand.GaussianAsymmetric(3.5f, 1.7f, 1.7f), 0, 6);
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Gaypocalypse)
            {
                return Mathf.Clamp((int)Rand.GaussianAsymmetric(7f, 3.13f, 1f), 0, 6);
            }
            throw new NotImplementedException();
        }

        public int kinseyRating;
        private List<Pawn> knownSexualitiesWorkingKeys;
        private List<int> knownSexualitiesWorkingValues;
        private Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
        private PsychologyPawn pawn;
    }
}
