// Decompiled with JetBrains decompiler
// Type: RimWorld.Tradeable_Pawn
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Tradeable_Pawn
    {
        [DetourMethod(typeof(Tradeable_Pawn),"ResolveTrade")]
        internal static void _ResolveTrade(this Tradeable_Pawn t)
        {
            if (t.ActionToDo == TradeAction.PlayerSells)
            {
                List<Pawn> list = t.thingsColony.Take(-t.countToDrop).Cast<Pawn>().ToList<Pawn>();
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn pawn = list[i];
                    pawn.PreTraded(TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader);
                    TradeSession.trader.AddToStock(pawn, TradeSession.playerNegotiator);
                    if (pawn.RaceProps.Humanlike)
                    {
                        foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                                 where x.IsColonist || x.IsPrisonerOfColony
                                                 select x)
                        {
                            current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerSold, null);
                            current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KnowPrisonerSoldBleedingHeart, null);
                        }
                    }
                }
            }
            else if (t.ActionToDo == TradeAction.PlayerBuys)
            {
                List<Pawn> list2 = t.thingsTrader.Take(t.countToDrop).Cast<Pawn>().ToList<Pawn>();
                for (int j = 0; j < list2.Count; j++)
                {
                    Pawn pawn2 = list2[j];
                    TradeSession.trader.GiveSoldThingToPlayer(pawn2, pawn2, TradeSession.playerNegotiator);
                    pawn2.PreTraded(TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader);
                }
            }
        }
    }
}
