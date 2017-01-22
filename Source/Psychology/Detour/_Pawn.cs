using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HugsLib.Source.Detour;
using System.Reflection;
using UnityEngine;

namespace Psychology.Detour
{
    internal static class _Pawn
    {
        internal static FieldInfo _factionInt;

        internal static Faction GetFactionInt(this Pawn _this)
        {
            if (_factionInt == null)
            {
                _factionInt = typeof(Pawn).GetField("factionInt", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_factionInt== null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_JobTracker.factionInt!", 0x12348765);
                }
            }
            return (Faction)_factionInt.GetValue(_this);
        }

        //Gee, sure wish I could override this.
        [DetourMethod(typeof(Pawn),"CheckAcceptArrest")]
        internal static bool _CheckAcceptArrest(this Pawn _this, Pawn arrester)
        {
            if (_this.health.hediffSet.HasHediff(HediffDefOfPsychology.Saboteur))
            {
                _this.health.hediffSet.hediffs.RemoveAll(h => h.def == HediffDefOfPsychology.Saboteur);
                Faction faction = Find.FactionManager.RandomEnemyFaction();
                _this.SetFaction(faction);
                List<Pawn> thisPawn = new List<Pawn>();
                thisPawn.Add(_this);
                IncidentParms parms = new IncidentParms();
                parms.faction = faction;
                parms.spawnCenter = _this.Position;
                Lord lord = LordMaker.MakeNewLord(faction, RaidStrategyDefOf.ImmediateAttack.Worker.MakeLordJob(parms, _this.Map), _this.Map, thisPawn);
                AvoidGridMaker.RegenerateAvoidGridsFor(faction, _this.Map);
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
                if (faction != null)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelSabotage".Translate(), "SaboteurRevealedFaction".Translate(new object[] { _this.LabelShort, faction.Name }).AdjustedFor(_this), LetterType.BadUrgent, _this, null);
                }
                else
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelSabotage".Translate(), "SaboteurRevealed".Translate(new object[] { _this.LabelShort }).AdjustedFor(_this), LetterType.BadUrgent, _this, null);
                }
            }
            if (_this.health.Downed)
            {
                return true;
            }
            if (_this.story != null && _this.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return true;
            }
            if (_this.Faction != null && _this.Faction != arrester.GetFactionInt())
            {
                _this.Faction.Notify_MemberCaptured(_this, arrester.Faction);
            }
            if (Rand.Value < (arrester.GetStatValue(StatDefOfPsychology.ArrestPeacefullyChance) * (Mathf.InverseLerp(-100f, 100f, _this.relations.OpinionOf(arrester))) * (arrester.Faction == _this.Faction ? 1.5 : 1)))
            {
                return true;
            }
            Messages.Message("MessageRefusedArrest".Translate(new object[]
            {
                _this.LabelShort
            }), _this, MessageSound.SeriousAlert);
            if (_this.Faction == null || !arrester.HostileTo(_this))
            {
                _this.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
            }
            return false;
        }

        //This too.
        [DetourMethod(typeof(Pawn),"ThreatDisabled")]
        internal static bool ThreatDisabled(this Pawn _this)
        {
            return !_this.Spawned || (!_this.InMentalState && _this.GetTraderCaravanRole() == TraderCaravanRole.Carrier && !(_this.jobs.curDriver is JobDriver_AttackMelee)) || _this.Position.Fogged(_this.Map) || _this.Downed || _this.health.hediffSet.HasHediff(HediffDefOfPsychology.Thief);
        }
    }
}
