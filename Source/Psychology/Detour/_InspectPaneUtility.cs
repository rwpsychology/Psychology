using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;
using System.Reflection;
using UnityEngine;

namespace Psychology.Detour
{
    class _InspectPaneUtility
    {
        [DetourMethod(typeof(InspectPaneUtility), "DoTabs")]
        private static void _DoTabs(IInspectPane pane)
        {
            try
            {
                float y = pane.PaneTopY - 30f;
                float num = 360f;
                float width = 0f;
                bool flag = false;
                foreach (InspectTabBase current in pane.CurTabs)
                {
                    if (current.IsVisible)
                    {
                        Rect rect = new Rect(num, y, 72f, 30f);
                        width = num;
                        Text.Font = GameFont.Small;
                        if (Widgets.ButtonText(rect, current.labelKey.Translate(), true, false, true))
                        {
                            typeof(InspectPaneUtility).GetMethod("InterfaceToggleTab", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { current, pane });
                        }
                        bool flag2 = current.GetType() == pane.OpenTabType;
                        if (!flag2 && !current.TutorHighlightTagClosed.NullOrEmpty())
                        {
                            UIHighlighter.HighlightOpportunity(rect, current.TutorHighlightTagClosed);
                        }
                        if (flag2)
                        {
                            current.DoTabGUI();
                            pane.RecentHeight = 700f;
                            flag = true;
                        }
                        num -= 72f;
                        if(num < 0)
                        {
                            num = 360f;
                            y -= 30f;
                        }
                    }
                }
                if (flag)
                {
                    GUI.DrawTexture(new Rect(0f, y, width, 30f), (Texture)typeof(InspectPaneUtility).GetField("InspectTabButtonFillTex", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(ex.ToString(), 742783);
            }
        }
    }
}
