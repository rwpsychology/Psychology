using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class Thought_MemoryDynamic : Thought_Memory
    {
        public Thought_MemoryDynamic()
        {
        }

        public override void ExposeData()
        {
            if(this.def != null)
            {
                this.def.defName = "Dynamic";
            }
            base.ExposeData();
            Scribe_Values.Look(ref this.topic, "topic", "Dynamic");
            Scribe_Values.Look(ref this.label, "label", "dynamic thought");
            Scribe_Values.Look(ref this.description, "description", "a dynamic thought.");
            Scribe_Values.Look(ref this.duration, "duration", 5f);
            Scribe_Values.Look(ref this.baseMoodEffect, "realMoodEffect", 5f);
            ThoughtDef def = new ThoughtDef();
            def.defName = this.topic;
            def.label = "dynamic thought";
            def.description = this.description;
            def.durationDays = this.duration;
            def.thoughtClass = typeof(Thought_MemoryDynamic);
            def.stackedEffectMultiplier = 1f;
            def.stackLimit = 999;
            ThoughtStage stage = new ThoughtStage();
            stage.label = this.label;
            stage.baseMoodEffect = this.baseMoodEffect;
            def.stages.Add(stage);
            this.def = def;
        }

        public override void Init()
        {
            this.topic = def.defName;
            this.duration = def.durationDays;
            this.label = def.stages[0].label;
            this.description = def.stages[0].description;
            this.baseMoodEffect = def.stages[0].baseMoodEffect;
            base.Init();
        }

        public override bool GroupsWith(Thought other)
        {
            Thought_MemoryDynamic thought_MemoryDynamic = other as Thought_MemoryDynamic;
            return thought_MemoryDynamic != null && this.LabelCap == thought_MemoryDynamic.LabelCap;
        }

        private string topic;
        private string label;
        private string description;
        private float duration;
        private float baseMoodEffect;
    }
}
