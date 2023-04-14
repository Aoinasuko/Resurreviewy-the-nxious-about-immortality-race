using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{

    public class ThoughtWorker_Resurrevi : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.def.defName != "Resurreviewy_Pawn")
            {
                return false;
            }
            if (!Util.isundead(p))
            {
                return false;
            }
            if (p.story.traits.HasTrait(TraitDefOf.NaturalMood, 2) | p.story.traits.HasTrait(TraitDefOf.NaturalMood, 1))
            {
                return false;
            }
            return true;
        }
    }

    public static class Util
    {
        // 傷と傷跡を治癒
        public static void HealInjury(Pawn pawn, int amount)
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

        public static void HealFirstMissingBodyPart(Pawn pawn)
        {
            List<Hediff_MissingPart> hediffs = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                pawn.health.RemoveHediff(hediffs[i]);
            }
            return;
        }

        public static void HealFirstDisease(Pawn pawn)
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
                    break;
                }
            }
            return;
        }

        public static bool isundead(Pawn pawn)
        {
            if (pawn.def.defName == "Resurrevi" | pawn.def.defName == "Resurreviewy_Pawn")
            {
                if (pawn.health.hediffSet.HasHediff(Hediff_Resur.Resur_Antidote_EternalLife))
                {
                    return false;
                }
                /*
                if (pawn.Faction != null)
                {
                    if (pawn.Faction.def == Faction_LitF.LitF_BEPFairyTech)
                    {
                        return false;
                    }
                }
                */
                return true;
            }
            if (pawn.health.hediffSet.HasHediff(Hediff_Resur.Resur_EternalLife))
            {
                return true;
            }
            return false;
        }
    }
}
