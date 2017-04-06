using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class PersonalityNodeDef : Def
    {
        
        public void ReloadParents()
        {
            parentDefs.Clear();
        }

        public float GetModifier(PersonalityNodeDef def)
        {
            PersonalityNodeParent parent = parents.Find((PersonalityNodeParent p) => p.node == def);
            return parent.modifier;
        }

        public List<PersonalityNodeDef> ParentNodes
        {
            get
            {
                if(this.parentDefs == null)
                {
                    this.parentDefs = new List<PersonalityNodeDef>();
                    if(!this.parents.NullOrEmpty())
                    {
                        foreach (PersonalityNodeParent parent in this.parents)
                        {
                            this.parentDefs.Add(parent.node);
                        }
                    }
                }
                return this.parentDefs;
            }
        }

        /* Being a woman has an 80% chance to modify this node by this amount, reduced by how gay she is.
         * This models the cultural impact traditional gender roles have on their personality.
         * Even in 55XX, the patriarchy has not been vanquished.
         */
        public float femaleModifier;
        //A list of the DefNames of the parents of this node.
        public List<PersonalityNodeParent> parents;
        //What pawns talk about when they talk about this node.
        public string conversationTopic;
        //What pawns with a high rating in this node use as a platform issue.
        public string platformIssueHigh;
        //What pawns with a low rating in this node use as a platform issue.
        public string platformIssueLow;
        //A list of the skills that modify this node.
        public List<PersonalityNodeSkillModifier> skillModifiers;
        //A list of the traits that modify this node.
        public List<PersonalityNodeTraitModifier> traitModifiers;
        //A list of the work types that being incapable of modify this node.
        public List<PersonalityNodeIncapableModifier> incapableModifiers;
        //How much a difference (or similarity) in this node affects what pawns think of each other after a conversation.
        public float controversiality;
        //The hours of the day that people with a high rating in this node will prefer to go on dates.
        public List<int> preferredDateHours;
        //A list of the actual parent Defs of this node.
        [Unsaved]
        private List<PersonalityNodeDef> parentDefs;

    }
}
