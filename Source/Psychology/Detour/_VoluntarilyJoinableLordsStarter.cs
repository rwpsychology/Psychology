using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _VoluntarilyJoinableLordsStarter
    {
        internal static FieldInfo _map;
        internal static FieldInfo _startPartyASAP;
        internal static FieldInfo _lastLordStartTick;

        internal static Map GetMap(this VoluntarilyJoinableLordsStarter _this)
        {
            if (_map == null)
            {
                _map = typeof(VoluntarilyJoinableLordsStarter).GetField("map", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_map == null)
                {
                    Log.ErrorOnce("Unable to reflect VoluntarilyJoinableLordsStarter.map!", 305432421);
                }
            }
            return (Map)_map.GetValue(_this);
        }

        internal static bool GetStartPartyASAP(this VoluntarilyJoinableLordsStarter _this)
        {
            if (_startPartyASAP == null)
            {
                _startPartyASAP = typeof(VoluntarilyJoinableLordsStarter).GetField("startPartyASAP", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_startPartyASAP == null)
                {
                    Log.ErrorOnce("Unable to reflect VoluntarilyJoinableLordsStarter.startPartyASAP!", 305432421);
                }
            }
            return (bool)_startPartyASAP.GetValue(_this);
        }

        internal static void SetStartPartyASAP(this VoluntarilyJoinableLordsStarter _this, bool val)
        {
            if (_startPartyASAP == null)
            {
                _startPartyASAP = typeof(VoluntarilyJoinableLordsStarter).GetField("startPartyASAP", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_startPartyASAP == null)
                {
                    Log.ErrorOnce("Unable to reflect VoluntarilyJoinableLordsStarter.startPartyASAP!", 305432421);
                }
            }
            _startPartyASAP.SetValue(_this, val);
        }

        internal static int GetLastLordStartTick(this VoluntarilyJoinableLordsStarter _this)
        {
            if (_lastLordStartTick == null)
            {
                _lastLordStartTick = typeof(VoluntarilyJoinableLordsStarter).GetField("lastLordStartTick", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_lastLordStartTick == null)
                {
                    Log.ErrorOnce("Unable to reflect VoluntarilyJoinableLordsStarter.lastLordStartTick!", 305432421);
                }
            }
            return (int)_lastLordStartTick.GetValue(_this);
        }

        [DetourMethod(typeof(VoluntarilyJoinableLordsStarter),"Tick_TryStartParty")]
        internal static void _Tick_TryStartParty(this VoluntarilyJoinableLordsStarter _this)
        {
            if (!_this.GetMap().IsPlayerHome)
            {
                return;
            }
            int socialiteMod = 1;
            List<Pawn> allPawnsSpawned = _this.GetMap().mapPawns.FreeColonistsSpawned.ToList();
            foreach (Pawn pawn in allPawnsSpawned)
            {
                if(pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Socialite))
                {
                    socialiteMod++;
                }
            }
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour*2 == 0)
            {
                if (Rand.MTBEventOccurs(40f, GenDate.TicksPerDay, (GenDate.TicksPerHour*2f*socialiteMod)))
                {
                    _this.SetStartPartyASAP(true);
                }
                if (_this.GetStartPartyASAP() && Find.TickManager.TicksGame - _this.GetLastLordStartTick() >= (int)(GenDate.TicksPerMonth*2 / socialiteMod) && PartyUtility.AcceptableMapConditionsToStartParty(_this.GetMap()))
                {
                    _this.TryStartParty();
                }
            }
        }
    }
}
