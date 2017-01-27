using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class Thought_MemorySocialConversation : Thought_MemorySocial
    {
        public Thought_MemorySocialConversation()
        {
        }

        public override void ExposeData()
        {
            if(this.def != null)
            {
                this.def.defName = "Conversation";
            }
            base.ExposeData();
            Scribe_Values.LookValue(ref this.topic, "topic", "Conversation");
            Scribe_Values.LookValue(ref this.label, "label", "conversation");
            Scribe_Values.LookValue(ref this.baseOpinionOffset, "realOpinionOffset", 5);
            ThoughtDef def = new ThoughtDef();
            def.defName = this.topic;
            def.label = "conversation";
            def.durationDays = 60f;
            def.thoughtClass = typeof(Thought_MemorySocialConversation);
            ThoughtStage stage = new ThoughtStage();
            stage.label = this.label;
            stage.baseOpinionOffset = this.baseOpinionOffset;
            def.stages.Add(stage);
            this.def = def;
        }

        public override void Init()
        {
            this.topic = def.defName;
            this.label = def.stages[0].label;
            this.baseOpinionOffset = def.stages[0].baseOpinionOffset;
            base.Init();
        }

        private string topic;
        private string label;
        private float baseOpinionOffset;
    }
}
