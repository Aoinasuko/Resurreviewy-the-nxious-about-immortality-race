using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{
    // 治療品
    public class compProperties_Healer : CompProperties
    {
        // 回復する傷と傷跡の回復量
        public int healamount = 10;
        // 失った部位を治癒できるか
        public bool canhealparts = false;
        // どのhpの部位まで治癒できるか
        public int maxhealparts = 10;
        // 致死的な病気を治癒できるか
        public bool canhealdisease = false;
        // 副作用確率
        public int sideeffect = 10;
        public compProperties_Healer()
        {
            compClass = typeof(comp_Healer);
        }
    }

    public class comp_Healer : ThingComp
    {
        public compProperties_Healer Props => (compProperties_Healer)props;

        public void heal(Pawn pawn)
        {
            // 傷と傷跡を治癒する
            HealInjury(pawn, Props.healamount);
            // 欠損の治癒
            if (Props.canhealparts)
            {
                HealMissingBodyPart(pawn, Props.maxhealparts);
            }
            // 致死的な病気の治癒
            if (Props.canhealdisease)
            {
                HealDisease(pawn);
            }
            if (Props.sideeffect > Rand.Range(0,100))
            {
                if (pawn.needs.mood != null)
                {
                    ThoughtDef named = DefDatabase<ThoughtDef>.GetNamed("Resur_PsychologicalEffect");
                    pawn.needs.mood.thoughts.memories.TryGainMemory(named);
                    Messages.Message("Resur.Incident.BackEffect".Translate(), pawn, MessageTypeDefOf.NegativeEvent, false);                    
                }
            }
        }

        // 傷と傷跡を治癒
        private static void HealInjury(Pawn pawn, int amount)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
                if (hediff_Injury != null && hediff_Injury.Visible && hediff_Injury.def.everCurableByItem)
                {
                    hediff_Injury.Heal(amount);
                }
            }
            return;
        }

        private static void HealMissingBodyPart(Pawn pawn, int maxheal)
        {
            List<Hediff_MissingPart> hediffs = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i].Part.def.GetMaxHealth(pawn) > maxheal)
                {
                    continue;
                }
                pawn.health.RemoveHediff(hediffs[i]);
            }
            return;
        }

        private static void HealDisease(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (!hediffs[i].Visible || !hediffs[i].def.everCurableByItem || hediffs[i].FullyImmune())
                {
                    continue;
                }
                if (hediffs[i].def.lethalSeverity >= 0f)
                {
                    pawn.health.RemoveHediff(hediffs[i]);
                }
            }
            return;
        }

    }
}
