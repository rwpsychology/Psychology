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
    public class Pawn_SexualityTracker : IExposable
    {
        public Pawn_SexualityTracker(Pawn pawn)
        {
            this.pawn = pawn;
            GenerateSexuality();
        }

        [LogPerformance]
        public bool IncompatibleSexualityKnown(Pawn recipient)
        {
            if(this.knownSexualities.ContainsKey(recipient))
            {
                return ((knownSexualities[recipient]-4) >= 0) != (recipient.gender == this.pawn.gender);
            }
            return false;
        }

        [LogPerformance]
        public void LearnSexuality(Pawn p)
        {
            if(p != null && PsycheHelper.PsychologyEnabled(pawn) && !knownSexualities.Keys.Contains(p))
            {
                knownSexualities.Add(p, PsycheHelper.Comp(p).Sexuality.kinseyRating);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
            Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1, false);
            Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1, false);
            Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
        }

        public void GenerateSexuality()
        {
            kinseyRating = RandKinsey();
            sexDrive = Mathf.Clamp01(Rand.Gaussian(1.1f, 0.26f));
            romanticDrive = Mathf.Clamp01(Rand.Gaussian(1.1f, 0.26f));
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
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Uniform)
            {
                return (int)(Rand.Value*6f);
            }
            throw new NotImplementedException();
        }

        public float AdjustedSexDrive
        {
            get
            {
                float ageFactor = 1f;
                if (pawn.gender == Gender.Male) {
                    ageFactor = MaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }
                else if (pawn.gender == Gender.Female)
                {
                    ageFactor = FemaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }
                return Mathf.Clamp01(ageFactor * Mathf.InverseLerp(0f, 0.5f, this.sexDrive));
            }
        }

        private static readonly SimpleCurve FemaleSexDriveCurve = new SimpleCurve
        {
            {
                new CurvePoint(10, 0f),
                true
            },
            {
                new CurvePoint(30, 3f),
                true
            },
            {
                new CurvePoint(65, 1f),
                true
            },
            {
                new CurvePoint(80, 0.6f),
                true
            }
        };

        private static readonly SimpleCurve MaleSexDriveCurve = new SimpleCurve
        {
            {
                new CurvePoint(12, 0f),
                true
            },
            {
                new CurvePoint(16, 3f),
                true
            },
            {
                new CurvePoint(21, 2f),
                true
            },
            {
                new CurvePoint(25, 1f),
                true
            },
            {
                new CurvePoint(40, 0.8f),
                true
            },
            {
                new CurvePoint(65, 0.6f),
                true
            }
        };

        public float AdjustedRomanticDrive
        {
            get
            {
                return Mathf.InverseLerp(0f, 0.5f, this.romanticDrive);
            }
        }
        
        public int kinseyRating;
        public float sexDrive;
        public float romanticDrive;
        private List<Pawn> knownSexualitiesWorkingKeys;
        private List<int> knownSexualitiesWorkingValues;
        private Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
        private Pawn pawn;
    }
}
