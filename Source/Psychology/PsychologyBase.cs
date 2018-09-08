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
using HugsLib;
using UnityEngine;
using Harmony;

namespace Psychology
{
    public class PsychologyBase : ModBase
    {
        private static bool kinsey = true;
        private static KinseyMode mode = KinseyMode.Realistic;
        public static bool notBabyMode = true;
        public static bool elections = true;
        public static float convoDuration = 60f;
        public static bool dateLetters = true;
        public static bool benchmark = false;
        private SettingHandle<bool> toggleKinsey;
        private SettingHandle<bool> toggleEmpathy;
        private SettingHandle<KinseyMode> kinseyMode;
        private SettingHandle<bool> toggleIndividuality;
        private SettingHandle<bool> toggleElections;
        private SettingHandle<float> conversationDuration;
        private SettingHandle<bool> toggleDateLetters;
        private SettingHandle<bool> toggleBenchmarking;
        public static Backstory child = new Backstory();

        public enum KinseyMode
        {
            Realistic,
            Uniform,
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

        static public float ConvoDuration()
        {
            return convoDuration;
        }

        static public bool SendDateLetters()
        {
            return dateLetters;
        }

        static public bool EnablePerformanceTesting()
        {
            return new PsychologyBase().Settings.GetHandle<bool>("Benchmarking", "BenchmarkingTitle".Translate(), "BenchmarkingTooltip".Translate(), false).Value;
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
            convoDuration = conversationDuration.Value;
            dateLetters = toggleDateLetters.Value;
            bool oldBenchmarkVal = benchmark;
            benchmark = toggleBenchmarking.Value;
            // The game has to be restarted for benchmarking to be applied/removed.
            if (oldBenchmarkVal != benchmark)
            {
                GenCommandLine.Restart();
            }
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
                conversationDuration = Settings.GetHandle<float>("ConversationDuration", "DurationTitle".Translate(), "DurationTooltip".Translate(), 60f, (String s) => float.Parse(s) >= 15f && float.Parse(s) <= 180f);
                toggleDateLetters = Settings.GetHandle<bool>("SendDateLetters", "SendDateLettersTitle".Translate(), "SendDateLettersTooltip".Translate(), true);
                toggleBenchmarking = Settings.GetHandle<bool>("Benchmarking", "BenchmarkingTitle".Translate(), "BenchmarkingTooltip".Translate(), false);

                kinsey = toggleKinsey.Value;
                notBabyMode = toggleIndividuality.Value;
                elections = toggleElections.Value;
                dateLetters = toggleDateLetters.Value;
                benchmark = toggleBenchmarking.Value;
                convoDuration = conversationDuration.Value;

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
                ThoughtDef colonistAbandoned = AddNullifyingTraits("ColonistBanished", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandoned != null && toggleEmpathy)
                {
                    colonistAbandoned = ModifyThoughtStages(colonistAbandoned, new int[] { -2 });
                }
                ThoughtDef colonistAbandonedToDie = AddNullifyingTraits("ColonistBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandonedToDie != null && toggleEmpathy)
                {
                    colonistAbandonedToDie = ModifyThoughtStages(colonistAbandonedToDie, new int[] { -4 });
                }
                ThoughtDef prisonerAbandonedToDie = AddNullifyingTraits("PrisonerBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
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

                /* ThingDef injection reworked by notfood */
                var zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");

                IEnumerable<ThingDef> things = (
                    from def in DefDatabase<ThingDef>.AllDefs
                    where typeof(Pawn).IsAssignableFrom(def.thingClass)
                    && def.race?.intelligence == Intelligence.Humanlike
                    && !def.defName.Contains("AIPawn")
                    && (zombieThinkTree == null || def.race.thinkTreeMain != zombieThinkTree)
                    select def
                );

                List<string> registered = new List<string>();
                foreach (ThingDef t in things)
                {
                    if (t.inspectorTabsResolved == null)
                    {
                        t.inspectorTabsResolved = new List<InspectTabBase>(1);
                    }
                    t.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));

                    if (t.recipes == null)
                    {
                        t.recipes = new List<RecipeDef>(6);
                    }
                    t.recipes.Add(RecipeDefOfPsychology.TreatPyromania);
                    t.recipes.Add(RecipeDefOfPsychology.TreatChemicalInterest);
                    t.recipes.Add(RecipeDefOfPsychology.TreatChemicalFascination);
                    t.recipes.Add(RecipeDefOfPsychology.TreatDepression);
                    t.recipes.Add(RecipeDefOfPsychology.TreatInsomnia);
                    t.recipes.Add(RecipeDefOfPsychology.CureAnxiety);

                    if (t.comps == null)
                    {
                        t.comps = new List<CompProperties>(1);
                    }
                    t.comps.Add(new CompProperties_Psychology());

                    if (!t.race.hediffGiverSets.NullOrEmpty())
                    {
                        if (t.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
                        {
                            t.race.hediffGiverSets.Add(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
                        }
                    }
                    registered.Add(t.defName);
                }
                if (Prefs.DevMode && Prefs.LogVerbose)
                {
                    Log.Message("Psychology :: Registered " + string.Join(", ", registered.ToArray()));
                }

                /*
                 * Now to enjoy the benefits of having made a popular mod!
                 * This will be our little secret.
                 */
                Traverse.Create(child).Field("bodyTypeMale").SetValue("Male");
                Traverse.Create(child).Field("bodyTypeFemale").SetValue("Female");
                child.slot = BackstorySlot.Childhood;
                child.SetTitle("Child soldier", "Child soldier");
                child.SetTitleShort("Scout", "Scout");
                child.baseDesc = "[PAWN_nameDef] was born into a dictatorial outlander society on a nearby rimworld. Their chief export was war, and [PAWN_pronoun] was conscripted at a young age into the military to serve as a scout due to [PAWN_possessive] runner's build. [PAWN_pronoun] learned how to use a gun, patch wounds on the battlefield, and communicate with [PAWN_possessive] squad. It was there [PAWN_pronoun] earned [PAWN_possessive] nickname.";
                Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string,int>>().Add("Shooting", 4);
                Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Medicine", 2);
                Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
                child.requiredWorkTags = WorkTags.Violent;
                child.shuffleable = false;
                child.PostLoad();
                child.ResolveReferences();
                Backstory adultMale = new Backstory();
                Traverse.Create(adultMale).Field("bodyTypeMale").SetValue("Male");
                Traverse.Create(adultMale).Field("bodyTypeFemale").SetValue("Female");
                adultMale.slot = BackstorySlot.Adulthood;
                adultMale.SetTitle("Missing in action", "Missing in action");
                adultMale.SetTitleShort("Ex-P.O.W.", "Ex-P.O.W.");
                adultMale.baseDesc = "Eventually, [PAWN_pronoun] was captured on a mission by one of [PAWN_possessive] faction's many enemies. [PAWN_pronoun] was tortured for information, the techniques of which [PAWN_pronoun] never forgot. When they could get no more out of [PAWN_objective], [PAWN_pronoun] was sent to a prison camp, where [PAWN_pronoun] worked for years before staging an escape and fleeing into civilization.";
                Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Crafting", 4);
                Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Construction", 3);
                Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Mining", 2);
                Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
                adultMale.spawnCategories = new List<string>();
                adultMale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
                adultMale.shuffleable = false;
                adultMale.PostLoad();
                adultMale.ResolveReferences();
                Backstory adultFemale = new Backstory();
                Traverse.Create(adultFemale).Field("bodyTypeMale").SetValue("Male");
                Traverse.Create(adultFemale).Field("bodyTypeFemale").SetValue("Female");
                adultFemale.slot = BackstorySlot.Adulthood;
                adultFemale.SetTitle("Battlefield medic", "Battlefield medic");
                adultFemale.SetTitleShort("Medic", "Medic");
                adultFemale.baseDesc = "[PAWN_pronoun] continued to serve in the military, being promoted through the ranks as [PAWN_possessive] skill increased. [PAWN_pronoun] learned how to treat more serious wounds as [PAWN_possessive] role slowly transitioned from scout to medic, as well as how to make good use of army rations. [PAWN_pronoun] built good rapport with [PAWN_possessive] squad as a result.";
                Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Shooting", 4);
                Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Medicine", 3);
                Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Cooking", 2);
                Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
                adultFemale.spawnCategories = new List<string>();
                adultFemale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
                adultFemale.shuffleable = false;
                adultFemale.PostLoad();
                adultFemale.ResolveReferences();
                PawnBio male = new PawnBio();
                male.childhood = child;
                male.adulthood = adultMale;
                male.gender = GenderPossibility.Male;
                male.name = NameTriple.FromString("Jason 'Jackal' Tarai");
                male.PostLoad();
                SolidBioDatabase.allBios.Add(male);
                PawnBio female = new PawnBio();
                female.childhood = child;
                female.adulthood = adultFemale;
                female.gender = GenderPossibility.Female;
                female.name = NameTriple.FromString("Elizabeth 'Eagle' Tarai");
                female.PostLoad();
                SolidBioDatabase.allBios.Add(female);
                BackstoryDatabase.AddBackstory(child);
                BackstoryDatabase.AddBackstory(adultMale);
                BackstoryDatabase.AddBackstory(adultFemale);
            }
        }

        public override void MapLoaded(Map map)
        {
            if (ModIsActive && PsychologyBase.ActivateKinsey())
            {
                /* Remove Gay trait from pawns if Kinsey scale is enabled */
                IEnumerable<Pawn> gayPawns = (from p in map.mapPawns.AllPawns
                                       where p.story != null && p.story.traits.HasTrait(TraitDefOf.Gay)
                                       select p);
                foreach (Pawn pawn in gayPawns)
                {
                    RemoveTrait(pawn, TraitDefOf.Gay);
                    if (PsycheHelper.PsychologyEnabled(pawn) && PsycheHelper.Comp(pawn).Sexuality.kinseyRating < 5)
                    {
                        PsycheHelper.Comp(pawn).Sexuality.kinseyRating = Rand.RangeInclusive(5, 6);
                    }
                }
            }
        }

        public override void Tick(int currentTick)
        {
            //Performance reporting tick
            if (EnablePerformanceTesting() && currentTick % GenDate.TicksPerDay == 0 && PerformanceSetup.performanceTotals.Keys.Count > 0)
            {
                Dictionary<string, float> averages = PerformanceSetup.performanceTotals.ToDictionary(x => x.Key, x => (float)x.Value / (float)PerformanceSetup.performanceCalls[x.Key]);
                int topAmt = Math.Min(10, averages.Count);
                List<KeyValuePair<string, float>> avgTicks = (from avg in averages orderby avg.Value descending select avg).Take(topAmt).ToList();
                List<KeyValuePair<string, float>> topTicks = (from avg in averages orderby avg.Value*PerformanceSetup.performanceCalls[avg.Key] descending select avg).Take(topAmt).ToList();
                StringBuilder avgString = new StringBuilder();
                foreach(KeyValuePair<string, float> t in avgTicks)
                {
                    avgString.AppendLine(t.Key + " (" + t.Value + ")");
                }
                StringBuilder topString = new StringBuilder();
                foreach (KeyValuePair<string, float> t in topTicks)
                {
                    topString.AppendLine(t.Key + " (" + avgTicks.Find(x => x.Key == t.Key).Value + ")");
                }
                Log.Message("Psychology :: Performance Report :: Top " + topAmt + " average tick consumers:\n" + avgString.ToString() + "\nTop " + topAmt + " weighted tick consumers: " + topString.ToString());
            }
            //Constituent tick
            if (currentTick % GenDate.TicksPerHour*2 == 0)
            {
                Map playerFactionMap = Find.WorldObjects.SettlementBases.Find(b => b.Faction.IsPlayer).Map;
                IEnumerable<Pawn> constituents = (from p in playerFactionMap.mapPawns.FreeColonistsSpawned
                                                  where !p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && p.GetLord() == null && p.GetTimeAssignment() != TimeAssignmentDefOf.Work && p.Awake()
                                                  select p);
                if(constituents.Count() > 0)
                {
                    Pawn potentialConstituent = constituents.RandomElementByWeight(p => 0.0001f + Mathf.Pow(Mathf.Abs(0.7f - p.needs.mood.CurLevel), 2));
                    IEnumerable<Pawn> activeMayors = (from m in playerFactionMap.mapPawns.FreeColonistsSpawned
                                                      where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == potentialConstituent.Map.Tile
                                                      && m.GetTimeAssignment() != TimeAssignmentDefOf.Work && m.GetTimeAssignment() != TimeAssignmentDefOf.Sleep && m.GetLord() == null && m.Awake() && m.GetLord() == null
                                                      select m);
                    if (potentialConstituent != null && !potentialConstituent.Downed && !potentialConstituent.Drafted && potentialConstituent.health.summaryHealth.SummaryHealthPercent >= 1f && potentialConstituent.GetTimeAssignment() != TimeAssignmentDefOf.Work && activeMayors.Count() > 0)
                    {
                        Pawn mayor = activeMayors.RandomElement(); //There should only be one.
                        IntVec3 gather = default(IntVec3);
                        String found = null;
                        if (mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable != null)
                        {
                            gather = mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable.parent.Position;
                            found = "office";
                        }
                        else if (mayor.ownership != null && mayor.ownership.OwnedBed != null)
                        {
                            gather = mayor.ownership.OwnedBed.Position;
                            found = "bed";
                        }
                        if (PsycheHelper.PsychologyEnabled(potentialConstituent) && Rand.Chance((1f - PsycheHelper.Comp(potentialConstituent).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) / 5f) && (found != null || RCellFinder.TryFindPartySpot(mayor, out gather))
                            && (!mayor.Drafted && !mayor.Downed && mayor.health.summaryHealth.SummaryHealthPercent >= 1f && mayor.GetTimeAssignment() != TimeAssignmentDefOf.Work && (mayor.CurJob == null || mayor.CurJob.def != JobDefOf.TendPatient)))
                        {
                            List<Pawn> pawns = new List<Pawn>();
                            pawns.Add(mayor);
                            pawns.Add(potentialConstituent);
                            Lord meeting = LordMaker.MakeNewLord(mayor.Faction, new LordJob_VisitMayor(gather, potentialConstituent, mayor, (potentialConstituent.needs.mood.CurLevel < (potentialConstituent.mindState.mentalBreaker.BreakThresholdMinor*1.25f))), mayor.Map, pawns);
                            mayor.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                            potentialConstituent.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                            if (found == "bed")
                            {
                                mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoOffice);
                            }
                            else if (found == null)
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
                foreach (Settlement settlement in Find.WorldObjects.Settlements)
                {
                    //Self-explanatory.
                    if (!PsychologyBase.ActivateElections())
                    {
                        continue;
                    }
                    //If the base isn't owned or named by the player, no election can be held.
                    if (!settlement.Faction.IsPlayer || !settlement.namedByPlayer)
                    {
                        continue;
                    }
                    //If the base is not at least a year old, no election will be held.
                    if ((Find.TickManager.TicksGame - settlement.creationGameTicks) / GenDate.TicksPerYear < 1)
                    {
                        continue;
                    }
                    //A base must have at least 7 people in it to hold an election.
                    if (settlement.Map.mapPawns.FreeColonistsSpawnedCount < 7)
                    {
                        continue;
                    }
                    //If an election is already being held, don't start a new one.
                    if (settlement.Map.gameConditionManager.ConditionIsActive(GameConditionDefOfPsychology.Election) || settlement.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
                    {
                        continue;
                    }
                    //Elections are held in Septober (because I guess some maps don't have fall?) and during the day.
                    if (GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(settlement.Tile).x) != Quadrum.Septober || (GenLocalDate.HourOfDay(settlement.Map) < 7 || GenLocalDate.HourOfDay(settlement.Map) > 20))
                    {
                        continue;
                    }
                    //If an election has already been completed this year, don't start a new one.
                    IEnumerable<Pawn> activeMayors = (from m in settlement.Map.mapPawns.FreeColonistsSpawned
                                                      where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == settlement.Map.Tile && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).yearElected == GenLocalDate.Year(settlement.Map.Tile)
                                                      select m);
                    if (activeMayors.Count() > 0)
                    {
                        continue;
                    }
                    //Try to space out the elections so they don't all proc at once.
                    if (Rand.RangeInclusive(1, 15 - GenLocalDate.DayOfQuadrum(settlement.Map.Tile)) > 1)
                    {
                        continue;
                    }
                    IncidentParms parms = new IncidentParms();
                    parms.target = settlement.Map;
                    parms.faction = settlement.Faction;
                    FiringIncident fi = new FiringIncident(IncidentDefOfPsychology.Election, null, parms);
                    Find.Storyteller.TryFire(fi);
                }
            }
        }
    }
}
