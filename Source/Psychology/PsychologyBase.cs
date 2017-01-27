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
using UnityEngine;

namespace Psychology
{
    public class PsychologyBase : HugsLib.ModBase
    {
        private static bool kinsey = true;
        private static KinseyMode mode = KinseyMode.Realistic;
        public static bool detoursMedical = true;
        public static bool detoursSexual = true;
        public static bool notBabyMode = true;
        private SettingHandle<bool> toggleKinsey;
        private SettingHandle<bool> toggleEmpathy;
        private SettingHandle<KinseyMode> kinseyMode;
        private SettingHandle<bool> toggleIndividuality;

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

        public override string ModIdentifier
        {
            get
            {
                return "Psychology";
            }
        }

        private static TraitDef AddConflictingTraits(String name, TraitDef[] traits)
        {
            TraitDef trait = TraitDef.Named(name);
            if (trait != null)
            {
                if (trait.conflictingTraits == null)
                {
                    trait.conflictingTraits = new List<TraitDef>();
                }
                foreach (TraitDef conflict in traits)
                {
                    trait.conflictingTraits.Add(conflict);
                }
            }
            return trait;
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
            for(int stage = 0; stage < thought.stages.Count; stage++)
            {
                thought.stages[stage].baseMoodEffect = stages[stage];
            }
            return thought;
        }

        private static ThoughtDef ReplaceThoughtWorker(String name, Type newWorker)
        {
            ThoughtDef thought = ThoughtDef.Named(name);
            if (thought != null && thought.workerClass != null)
            {
                thought.workerClass = newWorker;
            }
            return thought;
        }

        private static void RemoveTrait(Pawn pawn, TraitDef trait)
        {
            pawn.story.traits.allTraits.RemoveAll(t => t.def == trait);
        }

        public override void SettingsChanged()
        {
            kinsey = toggleKinsey.Value;
            mode = kinseyMode.Value;
            notBabyMode = toggleIndividuality.Value;
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
                
                notBabyMode = toggleIndividuality.Value;

                /* Mod conflict detection */

                if (!detoursMedical)
                {
                    Logger.Warning("MedicalDetourDisable".Translate());
                }

                TraitDef bisexual = DefDatabase<TraitDef>.GetNamedSilentFail("Bisexual");
                TraitDef asexual = DefDatabase<TraitDef>.GetNamedSilentFail("Asexual");
                if (bisexual != null || asexual != null || !toggleKinsey || !detoursSexual)
                {
                    if (toggleKinsey)
                    {
                        Logger.Message("KinseyDisable".Translate());
                        if (!detoursSexual)
                        {
                            Logger.Warning("KinseyDetourDisable".Translate());
                            TraitDefOfPsychology.Codependent.SetCommonality(0f);
                            TraitDefOfPsychology.Lecher.SetCommonality(0f);
                            TraitDefOfPsychology.OpenMinded.SetCommonality(0f);
                            TraitDefOfPsychology.Polygamous.SetCommonality(0f);
                        }
                    }
                    kinsey = false;
                }

                if (PsychologyBase.ActivateKinsey())
                {
                    mode = kinseyMode.Value;
                    TraitDef gay = TraitDef.Named("Gay");
                    if (gay != null)
                    {
                        gay.SetCommonality(0f);
                    }
                    foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (t.thingClass == typeof(Pawn))
                        {
                            t.thingClass = typeof(PsychologyPawn);
                        }
                    }
                }
                
                /* Vanilla Def edits */
                AddConflictingTraits("Nudist", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddConflictingTraits("Bloodlust", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddConflictingTraits("Psychopath", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized, TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("Cannibal", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Gourmet });
                AddConflictingTraits("Ascetic", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddConflictingTraits("Neurotic", new TraitDef[] { TraitDefOfPsychology.HeavySleeper });
                AddConflictingTraits("DislikesMen", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("DislikesWomen", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("Prosthophobe", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                
                AddNullifyingTraits("AteLavishMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteFineMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteAwfulMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteRawFood", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteInsectMeatAsIngredient", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteInsectMeatDirect", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteRottenFood", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("SleepDisturbed", new TraitDef[] { TraitDefOfPsychology.HeavySleeper });
                AddNullifyingTraits("ObservedLayingCorpse", new TraitDef[] { TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("WitnessedDeathAlly", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("WitnessedDeathNonAlly", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("FeelingRandom", new TraitDef[] { TraitDefOfPsychology.Unstable });
                AddNullifyingTraits("ApparelDamaged", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddNullifyingTraits("EnvironmentDark", new TraitDef[] { TraitDefOfPsychology.Photosensitive });
                AddNullifyingTraits("DeadMansApparel", new TraitDef[] { TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("Naked", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddNullifyingTraits("ColonistLeftUnburied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                AddNullifyingTraits("CheatedOnMe", new TraitDef[] { TraitDefOfPsychology.Polygamous });
                AddNullifyingTraits("Affair", new TraitDef[] { TraitDefOfPsychology.Polygamous });
                AddNullifyingTraits("Disfigured", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("Pretty", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("Ugly", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("SleptOutside", new TraitDef[] { TraitDefOfPsychology.Outdoorsy });
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

                ReplaceThoughtWorker("CabinFever", typeof(ThoughtWorker_CabinFever));
                ReplaceThoughtWorker("Disfigured", typeof(ThoughtWorker_Disfigured));
                ReplaceThoughtWorker("Ugly", typeof(ThoughtWorker_Ugly));
                ReplaceThoughtWorker("AnnoyingVoice", typeof(ThoughtWorker_AnnoyingVoice));
                ReplaceThoughtWorker("CreepyBreathing", typeof(ThoughtWorker_CreepyBreathing));
                ReplaceThoughtWorker("Pretty", typeof(ThoughtWorker_Pretty));
                ThoughtDef depressive = ReplaceThoughtWorker("MoodOffsetDepressive", typeof(ThoughtWorker_AlwaysActiveDepression));
                ThoughtStage treated = new ThoughtStage();
                treated.label = "depressive";
                treated.description = "Natural penalty from Depressive trait.";
                treated.baseMoodEffect = -6f;
                depressive.stages.Add(treated);

                InteractionDef chitChat = InteractionDefOf.Chitchat;
                if(chitChat != null)
                {
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    RulePack rulePack = new RulePack();
                    List<string> strings = new List<string>();
                    strings.Add("logentry->Exchanged pleasantries with [other_nameShortIndef].");
                    RuleStrings.SetValue(rulePack, strings);
                    chitChat.logRulesInitiator = rulePack;
                }

                MentalBreakDef berserk = DefDatabase<MentalBreakDef>.GetNamed("Berserk");
                if(berserk != null)
                {
                    berserk.baseCommonality = 0f;
                }
                MentalStateDef fireStartingSpree = DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree");
                if(fireStartingSpree != null)
                {
                    fireStartingSpree.workerClass = typeof(MentalStateWorker_FireStartingSpree);
                }
                IEnumerable<MentalStateDef> drugBinges = (from def in DefDatabase<MentalStateDef>.AllDefsListForReading
                                                          where def.workerClass == typeof(MentalStateWorker_BingingDrug)
                                                          select def);
                foreach (MentalStateDef binge in drugBinges)
                {
                    binge.workerClass = typeof(MentalStateWorker_BingingDrugPsychology);
                }

                /* New race-specific options
                 * Code adapted from code by FluffierThanThou */
                var livingRaces = DefDatabase<ThingDef>
                    .AllDefsListForReading
                    .Where(t => !t.race?.hediffGiverSets?.NullOrEmpty() ?? false);

                foreach (ThingDef alive in livingRaces)
                {
                    if (alive.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
                    {
                        alive.inspectorTabs.Add(typeof(ITab_Pawn_Psyche));
                        try
                        {
                            alive.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Concat(new object[]
                            {
                            "Could not instantiate inspector tab of type ",
                            typeof(ITab_Pawn_Psyche),
                            ": ",
                            ex
                            }));
                        }
                        alive.race.hediffGiverSets.Add(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
                        alive.recipes.Add(RecipeDefOfPsychology.TreatPyromania);
                        alive.recipes.Add(RecipeDefOfPsychology.TreatChemicalInterest);
                        alive.recipes.Add(RecipeDefOfPsychology.TreatChemicalFascination);
                        alive.recipes.Add(RecipeDefOfPsychology.TreatDepression);
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
                List<Pawn> gayPawns = (from p in map.mapPawns.AllPawns
                                       where p.RaceProps.Humanlike && p.story.traits.HasTrait(TraitDefOf.Gay)
                                       select p).ToList();
                foreach (Pawn pawn in gayPawns)
                {
                    RemoveTrait(pawn, TraitDefOf.Gay);
                    PsychologyPawn realPawn = pawn as PsychologyPawn;
                    if (realPawn != null && realPawn.sexuality.kinseyRating < 5)
                    {
                        realPawn.sexuality.kinseyRating = Rand.RangeInclusive(5, 6);
                    }
                }
                /* Fix Anxiety not being located in the brain */
                List<Pawn> anxiousPawns = (from p in map.mapPawns.AllPawns
                                       where p.health.hediffSet.HasHediff(HediffDefOfPsychology.Anxiety)
                                       select p).ToList();
                foreach (Pawn pawn in anxiousPawns)
                {
                    Hediff anxiety = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
                    if(anxiety.Part != pawn.health.hediffSet.GetBrain())
                    {
                        anxiety.Part = pawn.health.hediffSet.GetBrain();
                    }
                }
                /* Give all old Psychology pawns the new Psyche system. */
                List<PsychologyPawn> psychelessPawns = (from p in map.mapPawns.AllPawns
                                                        where p.RaceProps.Humanlike && p is PsychologyPawn && ((PsychologyPawn)p).psyche == null
                                                        select p as PsychologyPawn).ToList();
                foreach (PsychologyPawn pawn in psychelessPawns)
                {
                    pawn.psyche = new Pawn_PsycheTracker(pawn);
                    pawn.psyche.Initialize();
                }
            }
        }

        public override void Tick(int currentTick)
        {
            //Constituent tick
            if (currentTick % 6400 == 0)
            {
                Map playerFactionMap = Find.WorldObjects.FactionBases.Find(b => b.Faction.IsPlayer).Map;
                Pawn potentialConstituent = (from p in playerFactionMap.mapPawns.FreeColonistsSpawned
                                             where !p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor)
                                             select p).ToList().RandomElementByWeight(p => Mathf.Pow(Mathf.Abs(0.6f - p.needs.mood.CurLevel),2));
                List<Pawn> activeMayors = (from m in playerFactionMap.mapPawns.FreeColonistsSpawned
                                           where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == potentialConstituent.Map.Tile
                                           select m).ToList();
                if (potentialConstituent != null && potentialConstituent.Awake() && activeMayors.Count > 0)
                {
                    Pawn mayor = activeMayors.RandomElement(); //There should only be one.
                    PsychologyPawn psychologyConstituent = potentialConstituent as PsychologyPawn;
                    IntVec3 gather = default(IntVec3);
                    if(mayor.ownership != null && mayor.ownership.OwnedBed != null)
                    {
                        gather = mayor.ownership.OwnedBed.Position;
                    }
                    if(potentialConstituent.GetTimeAssignment() != TimeAssignmentDefOf.Work && mayor.GetTimeAssignment() != TimeAssignmentDefOf.Work && mayor.GetTimeAssignment() != TimeAssignmentDefOf.Sleep && mayor.GetLord() == null && (psychologyConstituent == null || Rand.Value > psychologyConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)*1.5f) && (gather != default(IntVec3) || RCellFinder.TryFindPartySpot(mayor, out gather)))
                    {
                        List<Pawn> pawns = new List<Pawn>();
                        pawns.Add(mayor);
                        pawns.Add(potentialConstituent);
                        LordMaker.MakeNewLord(mayor.Faction, new LordJob_VisitMayor(gather, potentialConstituent, mayor, (potentialConstituent.needs.mood.CurLevel < 0.4f)), mayor.Map, pawns);

                    }
                }
            }
            //Election tick
            if (currentTick % 15000 == 0)
            {
                foreach (FactionBase factionBase in Find.WorldObjects.FactionBases)
                {
                    //If the base isn't owned or named by the player, no election can be held.
                    if (!factionBase.Faction.IsPlayer || !factionBase.namedByPlayer)
                    {
                        continue;
                    }
                    //If the base is not at least a year old, no election will be held.
                    if ((Find.TickManager.TicksGame - factionBase.creationGameTicks) / (60000f * 60f) < 1)
                    {
                        continue;
                    }
                    //A base must have at least 7 people in it to hold an election.
                    if (factionBase.Map.mapPawns.FreeColonistsSpawnedCount < 7)
                    {
                        continue;
                    }
                    //If an election is already being held, don't start a new one.
                    if (factionBase.Map.mapConditionManager.ConditionIsActive(MapConditionDefOfPsychology.Election) || factionBase.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
                    {
                        continue;
                    }
                    //Elections are held in the fall and during the day.
                    if (GenLocalDate.Season(factionBase.Map) != Season.Fall || (GenLocalDate.HourOfDay(factionBase.Map) < 7 || GenLocalDate.HourOfDay(factionBase.Map) > 20))
                    {
                        continue;
                    }
                    //If an election has already been completed this year, don't start a new one.
                    List<Pawn> activeMayors = (from m in factionBase.Map.mapPawns.FreeColonistsSpawned
                                               where !m.Dead && m.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).worldTileElectedOn == factionBase.Map.Tile && ((Hediff_Mayor)m.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Mayor)).yearElected == GenLocalDate.Year(factionBase.Map.Tile)
                                               select m).ToList();
                    if (activeMayors.Count > 0)
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
