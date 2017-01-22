// Decompiled with JetBrains decompiler
// Type: RimWorld.JobGiver_GetRest
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using System.Reflection;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _JobGiver_GetRest
    {
        internal static FieldInfo _minCategory;

        internal static RestCategory MinCategory(this JobGiver_GetRest _this)
        {
            if (_minCategory == null)
            {
                _minCategory = typeof(JobGiver_GetRest).GetField("minCategory", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_minCategory == null)
                {
                    Log.ErrorOnce("Unable to reflect JobGiver_GetRest.minCategory!", 0x12348765);
                }
            }
            return (RestCategory)_minCategory.GetValue(_this);
        }

        [DetourMethod(typeof(JobGiver_GetRest),"GetPriority")]
        internal static float _GetPriority(this JobGiver_GetRest j, Pawn pawn)
        {
            Need_Rest rest = pawn.needs.rest;
            if (rest == null)
            {
                return 0f;
            }
            if (rest.CurCategory < j.MinCategory())
            {
                return 0f;
            }
            if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
            {
                return 0f;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
            {
                return 0f;
            }
            TimeAssignmentDef timeAssignmentDef;
            if (pawn.RaceProps.Humanlike)
            {
                timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
            }
            else
            {
                int num = GenLocalDate.HourOfDay(pawn);
                if (num < 7 || num > 21)
                {
                    timeAssignmentDef = TimeAssignmentDefOf.Sleep;
                }
                else
                {
                    timeAssignmentDef = TimeAssignmentDefOf.Anything;
                }
            }
            float curLevel = rest.CurLevel;
            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
            {
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
                {
                    if(curLevel < 0.3f)
                    {
                        return 1f;
                    }
                    return 0f;
                }
                else
                {
                    if (curLevel < 0.3f)
                    {
                        return 8f;
                    }
                    return 0f;
                }
            }
            else
            {
                if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                {
                    return 0f;
                }
                if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
                    {
                        if (curLevel < 0.3f)
                        {
                            return 3f;
                        }
                        return 0f;
                    }
                    else
                    {
                        if (curLevel < 0.3f)
                        {
                            return 8f;
                        }
                        return 0f;
                    }
                }
                else
                {
                    if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
                    {
                        throw new NotImplementedException();
                    }
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
                    {
                        if (curLevel < 0.75f)
                        {
                            return 3f;
                        }
                        return 0f;
                    }
                    else
                    {
                        if (curLevel < 0.75f)
                        {
                            return 8f;
                        }
                        return 0f;
                    }
                }
            }
        }
    }
}
