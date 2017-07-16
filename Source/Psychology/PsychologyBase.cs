using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using HugsLib.Settings;
using Harmony;
using UnityEngine;

namespace Psychology
{
    public class PsychologyBase : HugsLib.ModBase
    {
        private static bool kinsey = true;
        private static KinseyMode mode = KinseyMode.Realistic;
        public static bool notBabyMode = true;
        public static bool elections = true;
        private SettingHandle<bool> toggleKinsey;
        private SettingHandle<bool> toggleEmpathy;
        private SettingHandle<KinseyMode> kinseyMode;
        private SettingHandle<bool> toggleIndividuality;
        private SettingHandle<bool> toggleElections;

        public enum KinseyMode
        {
            Realistic,
            Invisible,
            Gaypocalypse
        };

        static public bool ActivateKinsey()
        {
            return kinsey;
        }

        static public KinseyMode KinseyFormula()
        {
            return mode;
        }

        static public bool IndividualityOn()
        {
            return notBabyMode;
        }

        static public bool ActivateElections()
        {
            return elections;
        }

        public override string ModIdentifier
        {
            get
            {
                return "Psychology";
            }
        }

        private static void RemoveTrait(Pawn pawn, TraitDef trait)
        {
            pawn.story.traits.allTraits.RemoveAll(t => t.def == trait);
        }

        private static ThoughtDef AddNullifyingTraits(String name, TraitDef[] traits)
        {
            ThoughtDef thought = ThoughtDef.Named(name);
            if (thought != null)
            {
                if (thought.nullifyingTraits == null)
                {
                    thought.nullifyingTraits = new List<TraitDef>();
                }
                foreach (TraitDef conflict in traits)
                {
                    thought.nullifyingTraits.Add(conflict);
                }
            }
            return thought;
        }

        private static ThoughtDef ModifyThoughtStages(ThoughtDef thought, int[] stages)
        {
            for (int stage = 0; stage < thought.stages.Count; stage++)
            {
                thought.stages[stage].baseMoodEffect = stages[stage];
            }
            return thought;
        }

        public override void SettingsChanged()
        {
            kinsey = toggleKinsey.Value;
            mode = kinseyMode.Value;
            notBabyMode = toggleIndividuality.Value;
            elections = toggleElections.Value;
        }

        public override void DefsLoaded()
        {
            if (ModIsActive)
            {
                /* Mod settings */

                toggleEmpathy = Settings.GetHandle<bool>("EnableEmpathy", "EmpathyChangesTitle".Translate(), "EmpathyChangesTooltip".Translate(), true);
                toggleKinsey = Settings.GetHandle<bool>("EnableSexuality", "SexualityChangesTitle".Translate(), "SexualityChangesTooltip".Translate(), true);
                kinseyMode = Settings.GetHandle<KinseyMode>("KinseyMode", "KinseyModeTitle".Translate(), "KinseyModeTooltip".Translate(), KinseyMode.Realistic, null, "KinseyMode_");
                toggleIndividuality = Settings.GetHandle<bool>("EnableIndividuality", "IndividualityTitle".Translate(), "IndividualityTooltip".Translate(), true);
                toggleElections = Settings.GetHandle<bool>("EnableElections", "ElectionsTitle".Translate(), "ElectionsTooltip".Translate(), true);

                notBabyMode = toggleIndividuality.Value;
                elections = toggleElections.Value;

                if (PsychologyBase.ActivateKinsey())
                {
                    mode = kinseyMode.Value;
                }

                /* Mod conflict detection */

                TraitDef bisexual = DefDatabase<TraitDef>.GetNamedSilentFail("Bisexual");
                TraitDef asexual = DefDatabase<TraitDef>.GetNamedSilentFail("Asexual");
                if (bisexual != null || asexual != null || !toggleKinsey)
                {
                    if (toggleKinsey)
                    {
                        Logger.Message("KinseyDisable".Translate());
                    }
                    kinsey = false;
                }

                /* Conditional vanilla Def edits */

                ThoughtDef knowGuestExecuted = AddNullifyingTraits("KnowGuestExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowGuestExecuted != null && toggleEmpathy)
                {
                    knowGuestExecuted = ModifyThoughtStages(knowGuestExecuted, new int[] { -1, -2, -4, -5 });
                }
                ThoughtDef knowColonistExecuted = AddNullifyingTraits("KnowColonistExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistExecuted != null && toggleEmpathy)
                {
                    knowColonistExecuted = ModifyThoughtStages(knowColonistExecuted, new int[] { -1, -2, -4, -5 });
                }
                ThoughtDef knowPrisonerDiedInnocent = AddNullifyingTraits("KnowPrisonerDiedInnocent", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowPrisonerDiedInnocent != null && toggleEmpathy)
                {
                    knowPrisonerDiedInnocent = ModifyThoughtStages(knowPrisonerDiedInnocent, new int[] { -4 });
                }
                ThoughtDef knowColonistDied = AddNullifyingTraits("KnowColonistDied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistDied != null && toggleEmpathy)
                {
                    knowColonistDied = ModifyThoughtStages(knowColonistDied, new int[] { -2 });
                }
                ThoughtDef colonistAbandoned = AddNullifyingTraits("ColonistAbandoned", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandoned != null && toggleEmpathy)
                {
                    colonistAbandoned = ModifyThoughtStages(colonistAbandoned, new int[] { -2 });
                }
                ThoughtDef colonistAbandonedToDie = AddNullifyingTraits("ColonistAbandonedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandonedToDie != null && toggleEmpathy)
                {
                    colonistAbandonedToDie = ModifyThoughtStages(colonistAbandonedToDie, new int[] { -4 });
                }
                ThoughtDef prisonerAbandonedToDie = AddNullifyingTraits("PrisonerAbandonedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (prisonerAbandonedToDie != null && toggleEmpathy)
                {
                    prisonerAbandonedToDie = ModifyThoughtStages(prisonerAbandonedToDie, new int[] { -3 });
                }
                ThoughtDef knowPrisonerSold = AddNullifyingTraits("KnowPrisonerSold", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowPrisonerSold != null && toggleEmpathy)
                {
                    knowPrisonerSold = ModifyThoughtStages(knowPrisonerSold, new int[] { -4 });
                }
                ThoughtDef knowGuestOrganHarvested = AddNullifyingTraits("KnowGuestOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowGuestOrganHarvested != null && toggleEmpathy)
                {
                    knowGuestOrganHarvested = ModifyThoughtStages(knowGuestOrganHarvested, new int[] { -4 });
                }
                ThoughtDef knowColonistOrganHarvested = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistOrganHarvested != null && toggleEmpathy)
                {
                    knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
                }
                ThoughtDef beauty = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistOrganHarvested != null && toggleEmpathy)
                {
                    knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
                }


                IEnumerable<ThingDef> things = (from m in LoadedModManager.RunningMods
                                         from def in m.AllDefs.OfType<ThingDef>()
                                         where typeof(Pawn).IsAssignableFrom(def.thingClass)
                                         select def);
                foreach (ThingDef t in things)
                {
                    if (t.race.intelligence == Intelligence.Humanlike && DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie")  != null && t.race.thinkTreeMain != DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie"))
                    {
                        t.thingClass = typeof(PsychologyPawn);
                        t.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
                        t.recipes.Add(RecipeDefOfPsychology.TreatPyromania);
                        t.recipes.Add(RecipeDefOfPsychology.TreatChemicalInterest);
                        t.recipes.Add(RecipeDefOfPsychology.TreatChemicalFascination);
                        t.recipes.Add(RecipeDefOfPsychology.TreatDepression);
                        if (!t.race?.hediffGiverSets?.NullOrEmpty() ?? false)
                        {
                            if (t.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
                            {
                                t.race.hediffGiverSets.Add(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
                            }
                        }
                    }
                }

                /*
                 * Now to enjoy the benefits of having made a popular mod!
                 * This will be our little secret.
                 */
                Backstory childMe = new Backstory();
                childMe.bodyTypeMale = BodyType.Male;
                childMe.bodyTypeFemale = BodyType.Female;
                childMe.slot = BackstorySlot.Childhood;
                childMe.SetTitle("Child soldier");
                childMe.SetTitleShort("Scout");
                childMe.baseDesc = "NAME was born into a dictatorial outlander society on a nearby rimworld. Their chief export was war, and HE was conscripted at a young age into the military to serve as a scout due to HIS runner's build. HECAP learned how to use a gun, patch wounds on the battlefield, and communicate with HIS squad. It was there HE earned HIS nickname.";
                childMe.skillGains.Add("Shooting", 4);
                childMe.skillGains.Add("Medicine", 2);
                childMe.skillGains.Add("Social", 1);
                childMe.requiredWorkTags = WorkTags.Violent;
                childMe.shuffleable = false;
                childMe.PostLoad();
                childMe.ResolveReferences();
                //Disabled until I can be bothered to code it so they're actually siblings.
                /*Backstory adultMale = new Backstory();
                adultMale.bodyTypeMale = BodyType.Male;
                adultMale.bodyTypeFemale = BodyType.Female;
                adultMale.slot = BackstorySlot.Adulthood;
                adultMale.SetTitle("Missing in action");
                adultMale.SetTitleShort("P.O.W.");
                adultMale.baseDesc = "Eventually, HE was captured on a mission by one of his faction's many enemies. HECAP was tortured for information, the techniques of which HE never forgot. When they could get no more out of HIM, HE was sent to a prison camp, where HE worked for years before staging an escape and fleeing into civilization.";
                adultMale.skillGains.Add("Crafting", 4);
                adultMale.skillGains.Add("Construction", 3);
                adultMale.skillGains.Add("Mining", 2);
                adultMale.skillGains.Add("Social", 1);
                adultMale.spawnCategories = new List<string>();
                adultMale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
                adultMale.shuffleable = false;
                adultMale.PostLoad();
                adultMale.ResolveReferences();*/
                Backstory adultFemale = new Backstory();
                adultFemale.bodyTypeMale = BodyType.Male;
                adultFemale.bodyTypeFemale = BodyType.Female;
                adultFemale.slot = BackstorySlot.Adulthood;
                adultFemale.SetTitle("Battlefield medic");
                adultFemale.SetTitleShort("Medic");
                adultFemale.baseDesc = "HECAP continued to serve in the military, being promoted through the ranks as HIS skill increased. HECAP learned how to treat more serious wounds as HIS role slowly transitioned from scout to medic, as well as how to make good use of army rations. HECAP built good rapport with HIS squad as a result.";
                adultFemale.skillGains.Add("Shooting", 4);
                adultFemale.skillGains.Add("Medicine", 3);
                adultFemale.skillGains.Add("Cooking", 2);
                adultFemale.skillGains.Add("Social", 1);
                adultFemale.spawnCategories = new List<string>();
                adultFemale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
                adultFemale.shuffleable = false;
                adultFemale.PostLoad();
                adultFemale.ResolveReferences();
                /*PawnBio maleMe = new PawnBio();
                maleMe.childhood = childMe;
                maleMe.adulthood = adultMale;
                maleMe.gender = GenderPossibility.Male;
                maleMe.name = NameTriple.FromString("Nathan 'Jackal' Tarai");
                maleMe.PostLoad();
                SolidBioDatabase.allBios.Add(maleMe);*/
                PawnBio femaleMe = new PawnBio();
                femaleMe.childhood = childMe;
                femaleMe.adulthood = adultFemale;
                femaleMe.gender = GenderPossibility.Female;
                femaleMe.name = NameTriple.FromString("Elizabeth 'Eagle' Tarai");
                femaleMe.PostLoad();
                SolidBioDatabase.allBios.Add(femaleMe);
                BackstoryDatabase.AddBackstory(childMe);
                //BackstoryDatabase.AddBackstory(adultMale);
                BackstoryDatabase.AddBackstory(adultFemale);
            }
        }

        public override void MapLoaded(Map map)
        {
            if (ModIsActive && PsychologyBase.ActivateKinsey())
            {
                /* Remove Gay trait from pawns if Kinsey scale is enabled */
                IEnumerable<Pawn> gayPawns = (from p in map.mapPawns.AllPawns
                                       where p.RaceProps.Humanlike && p.story.traits.HasTrait(TraitDefOf.Gay)
                                       select p);
                foreach (Pawn pawn in gayPawns)
                {
                    RemoveTrait(pawn, TraitDefOf.Gay);
                    PsychologyPawn realPawn = pawn as PsychologyPawn;
                    if (realPawn != null && realPawn.sexuality.kinseyRating < 5)
                    {
                        realPawn.sexuality.kinseyRating = Rand.RangeInclusive(5, 6);
                    }
                }
            }
        }

        public override void Tick(int currentTick)
        {
            //Constituent tick
            if (currentTick % GenDate.TicksPerHour*2 == 0)
            {
                Map playerFactionMap = Find.WorldObjects.FactionBases.Find(b => b.Faction.IsPlayer).Map;
                IEnumerable<Pawn> constituents = (from p in playerFactionMap.mapPawns.FreeColonistsSpawned
                                                  where !p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && p.GetTimeAssignment() != TimeAssignmentDefOf.Work && p.Awake()
                                                  select p);
                if(constituents.Count() > 0)
                {
                    Pawn potentialConstituent = constituents.RandomElementByWeight(p => 0.0001f + Mathf.Pow(Mathf.Abs(0.7f - p.needs.mood.CurLevel), 2));
                    IEnumerable<Pawn> activeMayors = (from m in playerFactionMap.mapPawns.FreeColonistsSpawned
                                                      where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == potentialConstituent.Map.Tile
                                                      && m.GetTimeAssignment() != TimeAssignmentDefOf.Work && m.GetTimeAssignment() != TimeAssignmentDefOf.Sleep && m.GetLord() == null && m.Awake()
                                                      select m);
                    if (potentialConstituent != null && activeMayors.Count() > 0)
                    {
                        Pawn mayor = activeMayors.RandomElement(); //There should only be one.
                        PsychologyPawn psychologyConstituent = potentialConstituent as PsychologyPawn;
                        IntVec3 gather = default(IntVec3);
                        bool foundBed = false;
                        if (mayor.ownership != null && mayor.ownership.OwnedBed != null)
                        {
                            gather = mayor.ownership.OwnedBed.Position;
                            foundBed = true;
                        }
                        if ((psychologyConstituent == null || Rand.Value < (1f - psychologyConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) / 5f) && (foundBed || RCellFinder.TryFindPartySpot(mayor, out gather)))
                        {
                            List<Pawn> pawns = new List<Pawn>();
                            pawns.Add(mayor);
                            pawns.Add(potentialConstituent);
                            Lord meeting = LordMaker.MakeNewLord(mayor.Faction, new LordJob_VisitMayor(gather, potentialConstituent, mayor, (potentialConstituent.needs.mood.CurLevel < 0.4f)), mayor.Map, pawns);
                            if (!foundBed)
                            {
                                mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoBedroom);
                            }
                        }
                    }
                }
            }
            //Election tick
            if (currentTick % (GenDate.TicksPerDay/4f) == 0)
            {
                foreach (FactionBase factionBase in Find.WorldObjects.FactionBases)
                {
                    //If the base isn't owned or named by the player, no election can be held.
                    if (!factionBase.Faction.IsPlayer || !factionBase.namedByPlayer)
                    {
                        continue;
                    }
                    //Self-explanatory.
                    if (!PsychologyBase.ActivateElections())
                    {
                        continue;
                    }
                    //If the base is not at least a year old, no election will be held.
                    if ((Find.TickManager.TicksGame - factionBase.creationGameTicks) / GenDate.TicksPerYear < 1)
                    {
                        continue;
                    }
                    //A base must have at least 7 people in it to hold an election.
                    if (factionBase.Map.mapPawns.FreeColonistsSpawnedCount < 7)
                    {
                        continue;
                    }
                    //If an election is already being held, don't start a new one.
                    if (factionBase.Map.gameConditionManager.ConditionIsActive(GameConditionDefOfPsychology.Election) || factionBase.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
                    {
                        continue;
                    }
                    //Elections are held in the fall and during the day.
                    if (GenLocalDate.Season(factionBase.Map) != Season.Fall || (GenLocalDate.HourOfDay(factionBase.Map) < 7 || GenLocalDate.HourOfDay(factionBase.Map) > 20))
                    {
                        continue;
                    }
                    //If an election has already been completed this year, don't start a new one.
                    IEnumerable<Pawn> activeMayors = (from m in factionBase.Map.mapPawns.FreeColonistsSpawned
                                                      where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == factionBase.Map.Tile && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).yearElected == GenLocalDate.Year(factionBase.Map.Tile)
                                                      select m);
                    if (activeMayors.Count() > 0)
                    {
                        continue;
                    }
                    //Try to space out the elections so they don't all proc at once.
                    if (Rand.RangeInclusive(1, 15 - GenLocalDate.DayOfSeason(factionBase.Map.Tile)) > 1)
                    {
                        continue;
                    }
                    IncidentParms parms = new IncidentParms();
                    parms.target = factionBase.Map;
                    parms.faction = factionBase.Faction;
                    FiringIncident fi = new FiringIncident(IncidentDefOfPsychology.Election, null, parms);
                    Find.Storyteller.TryFire(fi);
                }
            }
        }
    }
}
