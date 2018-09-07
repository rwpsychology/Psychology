using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Grammar;
using System.Reflection;

namespace Psychology
{
    public class PlayLogEntry_InteractionConversation : PlayLogEntry_Interaction
    {
        public PlayLogEntry_InteractionConversation()
        {
        }
        public PlayLogEntry_InteractionConversation(InteractionDef intDef, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks) : base(intDef, initiator, recipient, extraSentencePacks)
        {
            FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
            this.rulesInit = (List<string>)RuleStrings.GetValue(intDef.logRulesInitiator);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            for (int i = 0; i < rulesInit.Capacity; i++)
            {
                if(i+1 > this.rulesInit.Count)
                {
                    this.rulesInit.Add("r_logentry->"+"ConversationEnd".Translate()+" [RECIPIENT_nameDef].");
                }
                string ruleText = this.rulesInit[i];
                Scribe_Values.Look(ref ruleText, "rulesInit" + i, "r_logentry->" + "ConversationEnd".Translate());
                this.rulesInit[i] = ruleText;
            }
            /*for (int i = 0; i < rulesRecip.Capacity; i++)
            {
                if (i+1 > this.rulesRecip.Count)
                {
                    this.rulesRecip.Add("logentry->" + "ConversationEnd".Translate() + " [other_nameShortIndef].");
                }
                string ruleText = this.rulesRecip[i];
                Scribe_Values.Look(ref ruleText, "rulesRecip" + i, "logentry->" + "ConversationEnd".Translate() + " [other_nameShortIndef].");
                this.rulesRecip[i] = ruleText;
            }*/
            FieldInfo IntDef = typeof(PlayLogEntry_Interaction).GetField("intDef", BindingFlags.Instance | BindingFlags.NonPublic);
            InteractionDef newIntDef = new InteractionDef();
            newIntDef.defName = "EndConversation";
            FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
            Symbol.SetValue(newIntDef, Symbol.GetValue(InteractionDefOfPsychology.EndConversation));
            newIntDef.label = "ConversationEnded".Translate();
            FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
            RulePack initPack = new RulePack();
            RuleStrings.SetValue(initPack, this.rulesInit);
            newIntDef.logRulesInitiator = initPack;
            /*RulePack recipPack = new RulePack();
            RuleStrings.SetValue(recipPack, this.rulesRecip);
            newIntDef.logRulesRecipient = recipPack;*/
            IntDef.SetValue(this, newIntDef);
        }

        protected List<string> rulesInit = new List<string>(1);
        //protected List<string> rulesRecip = new List<string>(1);
    }
}
