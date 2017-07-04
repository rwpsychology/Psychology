using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony.Optional
{
    [StaticConstructorOnStartup]
    class EdBPrepareCarefully
    {
        static EdBPrepareCarefully()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.psychology.prepare_carefully_patch");
            #region prepareCarefully
            {
                try
                {
                    ((Action)(() =>
                    {
                        if (AccessTools.Method(typeof(EdB.PrepareCarefully.PrepareCarefully), nameof(EdB.PrepareCarefully.PrepareCarefully.Initialize)) != null)
                        {
                            harmony.Patch(AccessTools.Method(typeof(EdB.PrepareCarefully.PanelBackstory), "DrawPanelContent"), null, new HarmonyMethod(typeof(PanelBackstoryPatch), "AddPsycheEditButton"));
                        }
                    }))();
                }
                catch (TypeLoadException) { }
            }
            #endregion
        }
    }
}
